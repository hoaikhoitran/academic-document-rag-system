"""
app/services/vector_store_service.py
====================================

STEP 4 of the RAG pipeline: persist chunk vectors in a database so we
can find the most relevant ones later, in milliseconds, even when
there are thousands of chunks.

What is a vector database?
--------------------------
A vector database stores `(embedding, metadata, text)` rows and supports
"nearest-neighbor search": given a query embedding, return the K rows
whose vectors are closest (most similar) to it.

We use ChromaDB locally because:
  * it runs entirely in-process (no separate server to install)
  * it persists to a folder on disk (./chroma_db)
  * its API is small and beginner-friendly

Metadata stored per chunk
-------------------------
  documentId, courseCode, chapter, fileName, fileType,
  pageNumber, chunkIndex, source
"""

from __future__ import annotations

import logging
import threading
from typing import Any

import chromadb
from chromadb.config import Settings as ChromaSettings

from app.core.config import settings
from app.utils.file_utils import ensure_directory

logger = logging.getLogger(__name__)


class VectorStoreService:
    """Thin wrapper around a persistent ChromaDB collection."""

    _instance: "VectorStoreService | None" = None
    _instance_lock = threading.Lock()

    def __init__(self) -> None:
        # Make sure the persistence directory exists so Chroma can write to it.
        ensure_directory(settings.chroma_persist_path)

        # PersistentClient -> data survives across restarts.
        # anonymized_telemetry=False -> no outbound network calls.
        self._client = chromadb.PersistentClient(
            path=settings.chroma_persist_path,
            settings=ChromaSettings(anonymized_telemetry=False),
        )

        # get_or_create_collection is idempotent: first run creates it,
        # later runs reopen the same collection.
        # `hnsw:space=cosine` matches the similarity BGE-M3 is trained for.
        self._collection = self._client.get_or_create_collection(
            name=settings.CHROMA_COLLECTION_NAME,
            metadata={"hnsw:space": "cosine"},
        )

        logger.info(
            "Chroma collection '%s' ready at %s",
            settings.CHROMA_COLLECTION_NAME,
            settings.chroma_persist_path,
        )

    # ------------------------------------------------------------------
    # Singleton accessor
    # ------------------------------------------------------------------
    @classmethod
    def instance(cls) -> "VectorStoreService":
        if cls._instance is None:
            with cls._instance_lock:
                if cls._instance is None:
                    cls._instance = cls()
        return cls._instance

    # ------------------------------------------------------------------
    # Writes
    # ------------------------------------------------------------------
    def add_chunks(
        self,
        *,
        document_id: str,
        course_code: str,
        chapter: str,
        file_name: str,
        file_type: str,
        chunks: list[dict[str, Any]],
        embeddings: list[list[float]],
    ) -> int:
        """
        Persist a batch of chunks + their embeddings.

        Each `chunks[i]` must have:  chunkText, pageNumber, chunkIndex.
        `embeddings[i]` must be the vector for `chunks[i]`.

        Returns the number of items added.
        """
        if not chunks:
            return 0
        if len(chunks) != len(embeddings):
            raise ValueError("`chunks` and `embeddings` must have the same length.")

        ids: list[str] = []
        documents: list[str] = []
        metadatas: list[dict[str, Any]] = []

        for chunk in chunks:
            chunk_index = int(chunk["chunkIndex"])
            page_number = chunk.get("pageNumber")

            ids.append(f"{document_id}::chunk::{chunk_index}")
            documents.append(chunk["chunkText"])
            metadatas.append(
                {
                    "documentId": document_id,
                    "courseCode": course_code,
                    "chapter": chapter or "",
                    "fileName": file_name,
                    "fileType": file_type,
                    # ChromaDB metadata values must be primitive types.
                    # Use -1 as a sentinel for "no page" so filtering works.
                    "pageNumber": int(page_number) if page_number is not None else -1,
                    "chunkIndex": chunk_index,
                    "source": file_name,
                }
            )

        self._collection.add(
            ids=ids,
            documents=documents,
            metadatas=metadatas,
            embeddings=embeddings,
        )
        return len(ids)

    def delete_document(self, document_id: str) -> int:
        """
        Remove every chunk whose `documentId` matches.
        Returns the number of chunks removed (best-effort).
        """
        # Count first so we can report a meaningful number.
        existing = self._collection.get(where={"documentId": document_id})
        existing_ids = existing.get("ids", []) or []

        if existing_ids:
            self._collection.delete(ids=existing_ids)

        return len(existing_ids)

    # ------------------------------------------------------------------
    # Reads
    # ------------------------------------------------------------------
    def get_document_status(self, document_id: str) -> dict[str, Any]:
        """Return whether a document is indexed and how many chunks it has."""
        result = self._collection.get(where={"documentId": document_id})
        ids = result.get("ids", []) or []
        return {
            "documentId": document_id,
            "indexed": len(ids) > 0,
            "totalChunks": len(ids),
        }

    def search(
        self,
        *,
        question_embedding: list[float],
        document_id: str,
        course_code: str,
        top_k: int,
        max_distance: float | None = None,
    ) -> list[dict[str, Any]]:
        """
        Find the `top_k` chunks closest to `question_embedding`, filtered
        to one document inside one course AND to chunks whose cosine
        distance is no greater than `max_distance` (relevance filter).

        Why a distance filter?
        ----------------------
        ChromaDB always returns the K *nearest* chunks even when none of
        them are actually similar to the query. Without a distance cap,
        an out-of-scope question (e.g. "What is React Native?" on a
        course about MVC) would still pull back the MVC chunk and
        pretend it's relevant. The cap throws those non-relevant hits
        out so the caller can answer "I don't know" cleanly.

        `max_distance` defaults to `settings.RAG_MAX_DISTANCE` when None.
        For cosine distance: smaller == more similar.

        Returns a list of dicts (already filtered by distance):
            {"id": str, "text": str, "metadata": {...}, "distance": float}
        """
        threshold = (
            max_distance if max_distance is not None else settings.RAG_MAX_DISTANCE
        )

        where_clause: dict[str, Any] = {
            "$and": [
                {"documentId": {"$eq": document_id}},
                {"courseCode": {"$eq": course_code}},
            ]
        }

        result = self._collection.query(
            query_embeddings=[question_embedding],
            n_results=top_k,
            where=where_clause,
        )

        # Chroma returns each field as a list-of-lists (one inner list per
        # query). We only sent one query, so we always take index [0].
        ids_list = (result.get("ids") or [[]])[0]
        documents_list = (result.get("documents") or [[]])[0]
        metadatas_list = (result.get("metadatas") or [[]])[0]
        distances_list = (result.get("distances") or [[]])[0]

        hits: list[dict[str, Any]] = []
        for i in range(len(ids_list)):
            distance = distances_list[i] if i < len(distances_list) else None

            # Apply the relevance cap. If `distance` is missing for some
            # reason we keep the hit (better to surface than silently drop),
            # but in practice Chroma always returns a distance for a vector
            # query.
            if distance is not None and distance > threshold:
                continue

            hits.append(
                {
                    "id": ids_list[i],
                    "text": documents_list[i] if i < len(documents_list) else "",
                    "metadata": metadatas_list[i] if i < len(metadatas_list) else {},
                    "distance": distance,
                }
            )

        return hits


# Module-level convenience accessor.
vector_store_service: VectorStoreService = VectorStoreService.instance()

"""
app/services/embedding_service.py
=================================

STEP 3 of the RAG pipeline: turn text into vectors ("embeddings") so we
can compare two pieces of text by mathematical distance instead of
exact word matching.

What is an embedding?
---------------------
An embedding is a fixed-length list of floats (e.g. 1024 numbers for
BAAI/bge-m3). Two semantically similar texts map to nearby points in
this high-dimensional space, even if they use completely different
words (e.g. "MVC pattern" and "Mô hình MVC").

This service exposes ONE singleton model and two simple functions:
  * embed_text(text)         -> list[float]
  * embed_texts(texts)       -> list[list[float]]

Why a singleton?
----------------
Loading BAAI/bge-m3 from disk takes several seconds and ~2GB of RAM.
We MUST load it once at startup and reuse it for every request.
"""

from __future__ import annotations

import logging
import threading
from typing import Any

from app.core.config import settings

logger = logging.getLogger(__name__)


class EmbeddingService:
    """
    Singleton wrapper around BAAI/bge-m3.

    We try the official BAAI library (`FlagEmbedding.BGEM3FlagModel`)
    first because it implements the exact training-time tokenization
    and pooling. If that fails (e.g. a torch / dependency mismatch),
    we automatically fall back to `sentence-transformers`, which works
    on the same checkpoint with slightly different defaults.
    """

    _instance: "EmbeddingService | None" = None
    _instance_lock = threading.Lock()

    def __init__(self) -> None:
        self._model: Any = None
        self._backend: str = ""  # "flag" or "sentence-transformers"
        self._load_lock = threading.Lock()

    # ------------------------------------------------------------------
    # Singleton accessor
    # ------------------------------------------------------------------
    @classmethod
    def instance(cls) -> "EmbeddingService":
        if cls._instance is None:
            with cls._instance_lock:
                if cls._instance is None:
                    cls._instance = cls()
        return cls._instance

    # ------------------------------------------------------------------
    # Lazy model loading
    # ------------------------------------------------------------------
    def _ensure_loaded(self) -> None:
        """
        Load the model on first use. Subsequent calls are cheap.
        Thread-safe so concurrent requests don't load twice.
        """
        if self._model is not None:
            return

        with self._load_lock:
            if self._model is not None:
                return

            model_name = settings.EMBEDDING_MODEL_NAME

            # --- Try FlagEmbedding (preferred) ---
            try:
                from FlagEmbedding import BGEM3FlagModel  # type: ignore

                logger.info("Loading embedding model via FlagEmbedding: %s", model_name)
                # use_fp16=True if a CUDA GPU is available; safe default False.
                self._model = BGEM3FlagModel(model_name, use_fp16=False)
                self._backend = "flag"
                logger.info("FlagEmbedding model loaded.")
                return
            except Exception as flag_exc:  # noqa: BLE001
                logger.warning(
                    "FlagEmbedding unavailable (%s). Falling back to sentence-transformers.",
                    flag_exc,
                )

            # --- Fallback: sentence-transformers ---
            try:
                from sentence_transformers import SentenceTransformer  # type: ignore

                logger.info(
                    "Loading embedding model via sentence-transformers: %s", model_name
                )
                self._model = SentenceTransformer(model_name)
                self._backend = "sentence-transformers"
                logger.info("sentence-transformers model loaded.")
            except Exception as st_exc:  # noqa: BLE001
                raise RuntimeError(
                    "Could not load the embedding model '"
                    f"{model_name}'. Make sure FlagEmbedding or "
                    "sentence-transformers is installed and the model is "
                    "reachable. Underlying error: "
                    f"{st_exc}"
                ) from st_exc

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------
    def embed_texts(self, texts: list[str]) -> list[list[float]]:
        """
        Convert a batch of strings into a list of float vectors.
        We always pass batches when possible — embedding 100 texts in one
        call is much faster than 100 separate calls.
        """
        if not texts:
            return []

        self._ensure_loaded()

        if self._backend == "flag":
            # FlagEmbedding's encode() returns a dict; "dense_vecs" is the
            # 1024-d numeric vector for each input string.
            result = self._model.encode(
                texts,
                return_dense=True,
                return_sparse=False,
                return_colbert_vecs=False,
            )
            dense = result["dense_vecs"]
            return [vec.tolist() for vec in dense]

        # sentence-transformers backend
        # normalize_embeddings=True makes vectors unit-length so cosine
        # similarity matches what BGE-M3 was trained for.
        vectors = self._model.encode(
            texts,
            normalize_embeddings=True,
            convert_to_numpy=True,
            show_progress_bar=False,
        )
        return [vec.tolist() for vec in vectors]

    def embed_text(self, text: str) -> list[float]:
        """Convenience wrapper around embed_texts() for a single string."""
        vectors = self.embed_texts([text])
        return vectors[0] if vectors else []


# Module-level convenience accessor.
embedding_service: EmbeddingService = EmbeddingService.instance()

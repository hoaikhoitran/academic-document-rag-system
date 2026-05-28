"""
app/repositories/vector_repository.py
=====================================

Thin pass-through over VectorStoreService.

We keep this layer so that, if we ever swap ChromaDB for another vector
DB (Qdrant, Weaviate, PostgreSQL+pgvector, ...), only this file changes
— the RAG service and the API routes stay untouched.

For now it just re-exposes the singleton; no extra logic needed.
"""

from __future__ import annotations

from app.services.vector_store_service import vector_store_service


def get_vector_repository():
    """Return the shared vector store. Imported by rag_service."""
    return vector_store_service

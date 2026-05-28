"""
app/models/rag_response.py
==========================

Pydantic response models. They give Swagger a precise schema and make
the response shape consistent for the .NET client.
"""

from __future__ import annotations

from pydantic import BaseModel, Field


class HealthResponse(BaseModel):
    """Response for GET /health."""

    status: str = Field(..., examples=["ok"])
    service: str = Field(..., examples=["Retrieval-Augmented-Generation-PRN222"])


class IndexDocumentResponse(BaseModel):
    """Response for POST /rag/index-document."""

    documentId: str
    status: str = Field(..., examples=["indexed"])
    totalChunks: int = Field(..., examples=[25])
    message: str = Field(..., examples=["Document indexed successfully."])


class SourceItem(BaseModel):
    """A single retrieved chunk that supports an answer."""

    documentId: str
    fileName: str
    pageNumber: int | None = Field(
        default=None,
        description="1-based page index (PDFs) or slide index (PPTX). None for TXT/DOCX.",
    )
    chunkIndex: int = Field(..., description="0-based position of this chunk in the document.")
    text: str = Field(..., description="Plain-text preview of the chunk.")
    # Optional, for debugging / tuning RAG_MAX_DISTANCE.
    # Lower distance == more relevant (cosine distance).
    distance: float | None = Field(
        default=None,
        description="Cosine distance between the question and this chunk (lower is better).",
    )


class AskResponse(BaseModel):
    """Response for POST /rag/ask."""

    answer: str
    sources: list[SourceItem] = Field(default_factory=list)


class DocumentStatusResponse(BaseModel):
    """Response for GET /rag/documents/{documentId}/status."""

    documentId: str
    indexed: bool
    totalChunks: int = 0


class DeleteDocumentResponse(BaseModel):
    """Response for DELETE /rag/documents/{documentId}."""

    documentId: str
    status: str = Field(..., examples=["deleted"])

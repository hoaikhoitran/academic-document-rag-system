"""
app/models/rag_request.py
=========================

Pydantic request models. They:
  * validate the incoming JSON body
  * auto-generate clear Swagger / OpenAPI documentation
  * give the rest of the code typed objects to work with
"""

from __future__ import annotations

from pydantic import BaseModel, Field


class IndexDocumentRequest(BaseModel):
    """Request body for POST /rag/index-document."""

    documentId: str = Field(
        ...,
        description="Stable, unique ID of the document (used as ChromaDB key).",
        examples=["doc_001"],
    )
    courseCode: str = Field(
        ...,
        description="Course code (e.g. PRN222). Used to filter searches.",
        examples=["PRN222"],
    )
    chapter: str = Field(
        default="",
        description="Optional chapter / section label (e.g. 'Chapter 1').",
        examples=["Chapter 1"],
    )
    filePath: str = Field(
        ...,
        description="Local path to the document on disk (PDF/DOCX/PPTX/TXT).",
        examples=["./storage/documents/sample.pdf"],
    )
    fileName: str = Field(
        ...,
        description="Original file name (shown later in answer sources).",
        examples=["PRN222_Chapter_1.pdf"],
    )


class ConversationTurn(BaseModel):
    """One previous question-answer turn from the same chat session."""

    question: str = Field(default="", description="Previous user question.")
    answer: str = Field(default="", description="Previous assistant answer.")


class AskRequest(BaseModel):
    """Request body for POST /rag/ask."""

    sessionId: str = Field(default="", description="Chat session ID (optional).")
    userId: str = Field(default="", description="User ID (optional).")
    courseCode: str = Field(
        ...,
        description="Course code to restrict the search to.",
        examples=["PRN222"],
    )
    documentId: str = Field(
        ...,
        description="Document ID to restrict the search to.",
        examples=["doc_001"],
    )
    question: str = Field(
        ...,
        min_length=1,
        description="The student question (Vietnamese or English).",
        examples=["MVC trong ASP.NET Core là gì?"],
    )
    topK: int | None = Field(
        default=None,
        ge=1,
        le=50,
        description="How many chunks to retrieve. Defaults to DEFAULT_TOP_K.",
        examples=[5],
    )
    conversationHistory: list[ConversationTurn] = Field(
        default_factory=list,
        description="Previous turns from the same chat session, oldest first.",
    )

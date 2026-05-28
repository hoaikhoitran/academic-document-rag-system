"""
app/services/chunking_service.py
================================

STEP 2 of the RAG pipeline: split each loaded page into smaller pieces
("chunks") so that:

  1. Each chunk fits comfortably inside an embedding model's context window.
  2. The retrieval step can return small, focused passages instead of an
     entire page (better answers, less noise).

What is "fixed-size chunking with overlap"?
-------------------------------------------
We slide a window of size `chunk_size` over the text. After each window
we move forward by `chunk_size - chunk_overlap` characters, so the next
chunk repeats the last `chunk_overlap` characters of the previous one.

Why overlap?
------------
Without overlap, an important sentence could be cut in half between two
chunks — the embedding for either half would no longer carry the full
meaning. Overlap guarantees that any sentence near a boundary appears
fully inside at least one chunk.

Defaults
--------
  chunk_size    = 1500  (characters)
  chunk_overlap = 250   (characters)
"""

from __future__ import annotations

from typing import Any

from app.core.config import settings


def _split_text(text: str, chunk_size: int, chunk_overlap: int) -> list[str]:
    """
    Split a single string into overlapping windows.

    Edge cases handled
    ------------------
    * Empty / whitespace-only text -> returns [].
    * Text shorter than chunk_size -> returns [text] (one chunk).
    * chunk_overlap >= chunk_size  -> raises ValueError to avoid an
      infinite loop. The caller (chunk_pages) sanitizes this, but we
      double-check defensively.
    """
    text = (text or "").strip()
    if not text:
        return []

    if chunk_overlap >= chunk_size:
        raise ValueError("chunk_overlap must be smaller than chunk_size.")

    if len(text) <= chunk_size:
        return [text]

    # `stride` is how far the window moves forward each step.
    # Example: size 1500, overlap 250 -> stride 1250.
    stride = chunk_size - chunk_overlap
    chunks: list[str] = []

    start = 0
    while start < len(text):
        end = start + chunk_size
        piece = text[start:end].strip()
        if piece:
            chunks.append(piece)
        if end >= len(text):
            # We just consumed the tail — stop.
            break
        start += stride

    return chunks


def chunk_pages(
    pages: list[dict[str, Any]],
    chunk_size: int | None = None,
    chunk_overlap: int | None = None,
) -> list[dict[str, Any]]:
    """
    Convert a list of pages (output of document_loader.load_document)
    into a flat list of chunks.

    Input shape (per page):
        {"text": "...", "pageNumber": 1 | None, "source": "file.pdf"}

    Output shape (per chunk):
        {"chunkText": "...", "pageNumber": 1 | None, "chunkIndex": 0}

    chunkIndex is the GLOBAL position of this chunk across the whole
    document — 0 for the first chunk, increasing monotonically.
    """
    size = chunk_size if chunk_size is not None else settings.CHUNK_SIZE
    overlap = chunk_overlap if chunk_overlap is not None else settings.CHUNK_OVERLAP

    # Defensive sanitization — never let overlap >= size sneak in via env.
    if overlap >= size:
        overlap = max(0, size // 4)

    chunks: list[dict[str, Any]] = []
    global_index = 0

    for page in pages:
        page_text = page.get("text", "")
        page_number = page.get("pageNumber")

        for piece in _split_text(page_text, size, overlap):
            if not piece.strip():
                # Never store empty chunks.
                continue
            chunks.append(
                {
                    "chunkText": piece,
                    "pageNumber": page_number,
                    "chunkIndex": global_index,
                }
            )
            global_index += 1

    return chunks

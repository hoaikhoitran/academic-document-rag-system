"""
app/utils/text_utils.py
=======================

Small text-cleaning helpers used by document_loader and chunking_service.
"""

from __future__ import annotations

import re


def normalize_whitespace(text: str) -> str:
    """
    Collapse runs of whitespace (spaces, tabs, newlines) into single
    spaces and trim the result. Many PDF/DOCX readers produce stray
    newlines or non-breaking spaces; this gives us a clean string.
    """
    if not text:
        return ""
    # Replace any whitespace run with a single space, then strip.
    return re.sub(r"\s+", " ", text).strip()


def preview(text: str, max_len: int = 240) -> str:
    """Return at most `max_len` characters of `text`, with an ellipsis if cut."""
    if text is None:
        return ""
    text = text.strip()
    if len(text) <= max_len:
        return text
    return text[: max_len - 1].rstrip() + "…"

"""
app/utils/file_utils.py
=======================

Small helpers around the local filesystem.
"""

from __future__ import annotations

import os
from pathlib import Path

# File extensions we can read. Lowercase, including the leading dot.
SUPPORTED_EXTENSIONS = {".pdf", ".docx", ".pptx", ".txt"}


def get_file_extension(path: str) -> str:
    """Return the lowercase extension of `path`, including the dot (e.g. '.pdf')."""
    return Path(path).suffix.lower()


def is_supported_file(path: str) -> bool:
    """True if the file's extension is one we know how to parse."""
    return get_file_extension(path) in SUPPORTED_EXTENSIONS


def file_exists(path: str) -> bool:
    """Robust 'is this an actual file on disk?' check."""
    return os.path.isfile(path)


def ensure_directory(path: str) -> None:
    """Create the directory if it does not exist (no error if it does)."""
    os.makedirs(path, exist_ok=True)

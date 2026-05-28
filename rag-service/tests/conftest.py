"""
tests/conftest.py
=================

Test setup shared by every test module.

We do TWO things here:
  1. Make the project root importable (so `from app...` works).
  2. Stub out the heavy embedding model and ChromaDB so tests run
     instantly and do NOT need to download BAAI/bge-m3.
"""

from __future__ import annotations

import os
import sys
import tempfile
from pathlib import Path

# 1) Make `import app...` work when pytest is invoked from anywhere.
PROJECT_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(PROJECT_ROOT))

# 2) Force test-friendly environment BEFORE importing anything else.
os.environ.setdefault("APP_ENV", "test")
os.environ.setdefault("API_KEY", "")               # no auth in tests
os.environ.setdefault("MOCK_LLM", "true")
# Use a throwaway directory for Chroma so we never touch the real ./chroma_db.
os.environ["CHROMA_PERSIST_DIR"] = tempfile.mkdtemp(prefix="chroma_test_")

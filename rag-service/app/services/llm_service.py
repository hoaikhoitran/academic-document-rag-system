"""
app/services/llm_service.py
===========================

STEP 5 (final) of the RAG pipeline: GENERATION.

After retrieval, we have:
  * the user's question
  * a small set of "contexts" (the most relevant chunks from the DB)

The job of this file is to turn (question, contexts) into a final
human-readable ANSWER.

Two modes
---------
1. MOCK_LLM = true   (default for local dev)
   ----------------------------------------
   No external API call. We assemble a deterministic answer from the
   retrieved chunks themselves. This is safe (no hallucination), free,
   and runs offline.

2. MOCK_LLM = false
   ----------------
   The skeleton is ready for a real provider (OpenAI, Gemini, Ollama,
   ...). To keep the code beginner-friendly, only the dispatch +
   provider switch is wired up; the actual HTTP integration for each
   provider is left as a clearly marked TODO so you can add the
   provider you actually pay for, when you have a key.

Mandatory system prompt
-----------------------
Per the project spec, every real LLM call MUST include the system
prompt below. It binds the model to answer only from the retrieved
context and to refuse politely when context is insufficient.
"""

from __future__ import annotations

import logging
from typing import Any

from app.core.config import settings
from app.utils.text_utils import preview

logger = logging.getLogger(__name__)


# Mandatory system prompt for any real LLM provider.
SYSTEM_PROMPT = (
    "You are an academic assistant for students. "
    "Answer only based on the provided document context. "
    "If the answer cannot be found in the context, say: "
    "'Không đủ thông tin trong tài liệu để trả lời câu hỏi này.' "
    "Always cite the source document and page number when possible. "
    "Do not invent information outside the provided context."
)

# The standard "not enough context" reply, in Vietnamese, per spec.
INSUFFICIENT_CONTEXT_REPLY = (
    "Không đủ thông tin trong tài liệu để trả lời câu hỏi này."
)


def _format_contexts_for_prompt(contexts: list[dict[str, Any]]) -> str:
    """
    Lay out the retrieved chunks as a numbered list with source + page,
    so the LLM (or the mock) can see WHERE each fact comes from.
    """
    lines: list[str] = []
    for i, ctx in enumerate(contexts, start=1):
        meta = ctx.get("metadata") or {}
        file_name = meta.get("fileName", "unknown")
        page_number = meta.get("pageNumber", -1)
        page_label = f"page {page_number}" if page_number and page_number > 0 else "n/a"
        text = (ctx.get("text") or "").strip()
        lines.append(f"[{i}] (source: {file_name}, {page_label})\n{text}")
    return "\n\n".join(lines)


def _mock_answer(question: str, contexts: list[dict[str, Any]]) -> str:
    """
    Build a simple, faithful answer without calling any external API.

    Strategy: stitch the top retrieved snippets together, mention which
    file/page they came from, and explicitly hand control back to the
    user. We never invent content that isn't already in `contexts`.
    """
    if not contexts:
        return INSUFFICIENT_CONTEXT_REPLY

    # Show at most the 3 best snippets to keep the answer compact.
    top = contexts[:3]
    snippet_lines: list[str] = []
    for i, ctx in enumerate(top, start=1):
        meta = ctx.get("metadata") or {}
        file_name = meta.get("fileName", "tài liệu")
        page_number = meta.get("pageNumber", -1)
        page_label = (
            f" (trang {page_number})" if page_number and page_number > 0 else ""
        )
        snippet_lines.append(
            f"{i}. Trích từ {file_name}{page_label}: {preview(ctx.get('text', ''), 400)}"
        )

    body = "\n".join(snippet_lines)
    return (
        f"Dựa trên tài liệu được truy xuất cho câu hỏi: \"{question.strip()}\", "
        "đây là các đoạn liên quan nhất:\n\n"
        f"{body}\n\n"
        "(Câu trả lời được tổng hợp trực tiếp từ nội dung tài liệu — "
        "không có thông tin bổ sung nằm ngoài ngữ cảnh.)"
    )


def generate_answer(question: str, contexts: list[dict[str, Any]]) -> str:
    """
    Public entry point used by rag_service.

    Parameters
    ----------
    question : str
        The student's question (already validated as non-empty).
    contexts : list of dict
        Retrieved chunks. Each must have at least `text` and `metadata`.
    """
    # Rule #1 (applies to BOTH mock and real LLM paths):
    # if retrieval found nothing, do NOT call the LLM at all and do NOT
    # try to "guess" an answer. We return the mandatory Vietnamese
    # fallback so the .NET API can show the student a clear message.
    # The same fallback is also returned by `_mock_answer` defensively
    # if it is ever invoked with an empty list.
    if not contexts:
        return INSUFFICIENT_CONTEXT_REPLY

    # Mock mode = local, deterministic, no external calls.
    if settings.MOCK_LLM:
        return _mock_answer(question, contexts)

    provider = (settings.LLM_PROVIDER or "").lower().strip()
    logger.info("Calling LLM provider: %s", provider or "<none>")

    # Build the prompt that any real provider would receive.
    user_prompt = (
        f"Question:\n{question.strip()}\n\n"
        f"Context:\n{_format_contexts_for_prompt(contexts)}\n\n"
        "Answer:"
    )

    # ------------------------------------------------------------------
    # Real-provider dispatch. Implement the body for the provider you
    # actually have a key for. The interface (system + user prompt) is
    # already correct.
    # ------------------------------------------------------------------
    if provider in {"", "mock"}:
        # Fallback: behave like mock if MOCK_LLM was set to false but no
        # provider has been configured yet.
        return _mock_answer(question, contexts)

    if provider == "openai":
        # TODO: implement OpenAI Chat Completions here using
        # settings.LLM_API_KEY and settings.LLM_MODEL_NAME.
        # Include SYSTEM_PROMPT as the system message.
        raise NotImplementedError(
            "LLM_PROVIDER='openai' is not wired up yet. "
            "Set MOCK_LLM=true to run locally, or add the OpenAI call here."
        )

    if provider == "gemini":
        # TODO: implement Google Gemini call here.
        raise NotImplementedError(
            "LLM_PROVIDER='gemini' is not wired up yet."
        )

    if provider == "ollama":
        # TODO: implement Ollama (local) call via httpx.
        raise NotImplementedError(
            "LLM_PROVIDER='ollama' is not wired up yet."
        )

    raise NotImplementedError(f"Unknown LLM_PROVIDER: {provider!r}")

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
import re
from typing import Any

from app.core.config import settings
from app.utils.text_utils import preview

logger = logging.getLogger(__name__)


SYSTEM_PROMPT = (
    "You are an academic assistant for students. "
    "Answer only based on the provided document context. "
    "If the answer cannot be found in the context, say: "
    "'Không đủ thông tin trong tài liệu để trả lời câu hỏi này.' "
    "Always cite the source document and page number when possible. "
    "Do not invent information outside the provided context."
)

INSUFFICIENT_CONTEXT_REPLY = (
    "Không đủ thông tin trong tài liệu để trả lời câu hỏi này."
)


def _format_contexts_for_prompt(contexts: list[dict[str, Any]]) -> str:
    """
    Lay out the retrieved chunks as a numbered list with source + page,
    so the LLM can see where each fact comes from.
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


def _extract_question_terms(question: str) -> list[str]:
    """
    Extract important terms from the user's question.

    We remove common Vietnamese/English question words so the mock answer
    focuses on real content terms such as "Golden", "Retriever", "Poodle".
    """
    stopwords = {
        "tài",
        "liệu",
        "noi",
        "nói",
        "gi",
        "gì",
        "ve",
        "về",
        "cho",
        "chó",
        "trong",
        "này",
        "theo",
        "document",
        "what",
        "about",
        "tell",
        "say",
        "does",
        "this",
        "that",
    }

    terms = [
        term.lower()
        for term in re.findall(r"[A-Za-zÀ-ỹ0-9]+", question)
        if len(term) >= 4 and term.lower() not in stopwords
    ]

    return terms


def _extract_relevant_clause(sentence: str, question_terms: list[str]) -> str:
    """
    Extract the most relevant clause from a sentence.

    Example source sentence:
    "Ví dụ, chó Becgie thường được huấn luyện làm chó nghiệp vụ,
     chó Golden Retriever hiền lành và thân thiện,
     còn chó Poodle nhỏ nhắn, thông minh, phù hợp nuôi trong nhà."

    If the question asks about Poodle, this returns only:
    "chó Poodle nhỏ nhắn, thông minh, phù hợp nuôi trong nhà."
    """
    sentence = sentence.strip()

    if not sentence:
        return ""

    if not question_terms:
        return sentence

    clauses = re.split(r"\s*,\s*", sentence)

    for index, clause in enumerate(clauses):
        clause_clean = clause.strip()
        clause_lower = clause_clean.lower()

        if any(term in clause_lower for term in question_terms):
            selected_clauses = [clause_clean]

            for next_clause in clauses[index + 1:]:
                next_clause_clean = next_clause.strip()
                next_clause_lower = next_clause_clean.lower()

                if not next_clause_clean:
                    continue

                # Stop when the next clause starts a new dog breed/topic.
                # This prevents Golden answer from including Poodle content.
                if (
                    next_clause_lower.startswith("chó ")
                    or next_clause_lower.startswith("còn chó ")
                    or next_clause_lower.startswith("con chó ")
                ):
                    break

                selected_clauses.append(next_clause_clean)

            result = ", ".join(selected_clauses).strip()

            # Clean leading connector for nicer Vietnamese output.
            result = re.sub(r"^còn\s+", "", result, flags=re.IGNORECASE).strip()

            if result and not result.endswith((".", "!", "?")):
                result += "."

            return result

    return sentence


def _extract_relevant_sentences(
    question: str,
    text: str,
    max_sentences: int = 2,
) -> str:
    """
    Extract the most relevant sentence or clause from a retrieved chunk.

    This keeps MOCK_LLM answers short and focused without calling an external LLM.
    """
    text = (text or "").strip()

    if not text:
        return INSUFFICIENT_CONTEXT_REPLY

    question_terms = _extract_question_terms(question)

    sentences = re.split(r"(?<=[.!?])\s+", text)

    scored_sentences: list[tuple[int, str]] = []

    for sentence in sentences:
        sentence = sentence.strip()

        if not sentence:
            continue

        sentence_lower = sentence.lower()

        score = sum(1 for term in question_terms if term in sentence_lower)

        if score > 0:
            scored_sentences.append((score, sentence))

    if scored_sentences:
        scored_sentences.sort(key=lambda item: item[0], reverse=True)

        selected_results: list[str] = []

        for _, sentence in scored_sentences[:max_sentences]:
            relevant_clause = _extract_relevant_clause(sentence, question_terms)

            if relevant_clause:
                selected_results.append(relevant_clause)

        return " ".join(selected_results)

    return preview(text, 250)


def _mock_answer(question: str, contexts: list[dict[str, Any]]) -> str:
    """
    Build a short, focused answer from the most relevant retrieved context.

    This mock mode does not call any external LLM, so it does not truly
    paraphrase like ChatGPT/Gemini. It only extracts the most relevant
    sentences/clauses from the retrieved document chunk.
    """
    if not contexts:
        return INSUFFICIENT_CONTEXT_REPLY

    ctx = contexts[0]
    meta = ctx.get("metadata") or {}

    file_name = meta.get("fileName", "tài liệu")
    page_number = meta.get("pageNumber", -1)

    page_label = (
        f", trang {page_number}"
        if page_number and page_number > 0
        else ""
    )

    relevant_text = _extract_relevant_sentences(
        question=question,
        text=ctx.get("text", ""),
        max_sentences=2,
    )

    return f"Theo tài liệu {file_name}{page_label}, {relevant_text}"


def generate_answer(question: str, contexts: list[dict[str, Any]]) -> str:
    """
    Public entry point used by rag_service.

    Parameters
    ----------
    question : str
        The student's question.
    contexts : list of dict
        Retrieved chunks. Each must have at least `text` and `metadata`.
    """
    if not contexts:
        return INSUFFICIENT_CONTEXT_REPLY

    if settings.MOCK_LLM:
        return _mock_answer(question, contexts)

    provider = (settings.LLM_PROVIDER or "").lower().strip()

    logger.info("Calling LLM provider: %s", provider or "<none>")

    user_prompt = (
        f"Question:\n{question.strip()}\n\n"
        f"Context:\n{_format_contexts_for_prompt(contexts)}\n\n"
        "Answer:"
    )

    if provider in {"", "mock"}:
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
        # TODO: implement Ollama local call via httpx.
        raise NotImplementedError(
            "LLM_PROVIDER='ollama' is not wired up yet."
        )

    raise NotImplementedError(f"Unknown LLM_PROVIDER: {provider!r}")
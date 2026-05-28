"""
app/main.py
===========

FastAPI application entry point.

Run locally:
    uvicorn app.main:app --reload
"""

from __future__ import annotations

import logging

from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.api.rag_routes import health_router, router as rag_router
from app.core.config import settings

# Basic logging — INFO is a sane default for a backend service.
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)-7s %(name)s :: %(message)s",
)
logger = logging.getLogger(__name__)


def create_app() -> FastAPI:
    """Application factory. Lets tests build an isolated app if needed."""
    application = FastAPI(
        title=settings.APP_NAME,
        version="1.0.0",
        description=(
            "Independent RAG service for the PRN222 .NET project. "
            "Indexes course documents (PDF/DOCX/PPTX/TXT) and answers "
            "student questions grounded in those documents."
        ),
    )

    # CORS: open by default since this service usually sits behind a .NET API
    # on the same private network. Tighten in production via a reverse proxy.
    application.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=False,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    # Mount routers.
    application.include_router(health_router)
    application.include_router(rag_router)

    # Catch-all exception handler so we never leak a raw stack trace.
    @application.exception_handler(Exception)
    async def unhandled_exception_handler(
        request: Request, exc: Exception
    ) -> JSONResponse:
        logger.exception("Unhandled exception on %s %s", request.method, request.url)
        return JSONResponse(
            status_code=500,
            content={"detail": "Internal server error.", "error": str(exc)},
        )

    return application


# Uvicorn / Docker imports this `app` object.
app: FastAPI = create_app()

"""
app/core/security.py
====================

A tiny, beginner-friendly API-key dependency.

Behavior
--------
* If `settings.API_KEY` is empty -> the check is disabled, requests pass
  through (this is the default for local development).
* If `settings.API_KEY` is set    -> every request must include the
  header `X-API-Key: <value>`. Mismatched / missing keys -> HTTP 401.
"""

from fastapi import Header, HTTPException, status

from app.core.config import settings


async def verify_api_key(x_api_key: str | None = Header(default=None)) -> None:
    """
    FastAPI dependency that validates the inbound X-API-Key header.

    Use it on a route or a router with `Depends(verify_api_key)`.
    """
    if not settings.is_api_key_required:
        # Local development mode: no key required, let everything through.
        return

    if not x_api_key or x_api_key != settings.API_KEY:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid or missing X-API-Key header.",
        )

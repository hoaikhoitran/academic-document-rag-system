# Dockerfile for Retrieval-Augmented-Generation-PRN222
#
# Build:
#   docker build -t rag-prn222 .
# Run:
#   docker run --rm -p 8000:8000 --env-file .env rag-prn222

FROM python:3.11-slim

# Faster Python in a container.
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1

# OS-level deps PyMuPDF / chromadb may need at runtime.
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        build-essential \
        libgl1 \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Install Python dependencies first (Docker layer cache friendly).
COPY requirements.txt /app/requirements.txt
RUN pip install --upgrade pip \
 && pip install -r requirements.txt

# Copy the application source. .env stays OUT of the image (env_file at runtime).
COPY app /app/app
COPY storage /app/storage
COPY chroma_db /app/chroma_db

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]

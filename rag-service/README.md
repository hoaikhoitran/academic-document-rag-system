# Retrieval-Augmented-Generation-PRN222

Independent **Python RAG service** for the PRN222 .NET project.
This service indexes course documents (PDF / DOCX / PPTX / TXT) and lets the
.NET API answer student questions grounded in those documents.

It is intentionally **beginner-friendly**: small files, lots of comments, and
no external paid API required (mock LLM mode is on by default).

---

## Table of contents

1. [What is RAG?](#what-is-rag)
2. [Key concepts](#key-concepts)
3. [Architecture overview](#architecture-overview)
4. [Folder structure](#folder-structure)
5. [Install & run locally](#install--run-locally)
6. [Run with Docker](#run-with-docker)
7. [API contract](#api-contract)
8. [curl examples](#curl-examples)
9. [How the .NET API should call this service](#how-the-net-api-should-call-this-service)
10. [Troubleshooting](#troubleshooting)

---

## What is RAG?

**RAG = Retrieval-Augmented Generation**. Instead of asking an LLM to answer
from its own memory (which can hallucinate), we:

1. **Retrieve** the most relevant passages from your own documents.
2. **Augment** the LLM prompt with those passages as context.
3. **Generate** the final answer using only that context.

The benefit: the answers stay grounded in your course materials, and you can
cite the exact file + page for every answer.

## Key concepts

### Chunking
LLMs cannot read a 200-page PDF in one go, and embeddings work better on small,
focused passages. So we cut every document into **chunks** of a fixed size.
We also keep a small **overlap** between neighboring chunks so we don't split
sentences in half.

Defaults:
- `CHUNK_SIZE = 1500` characters
- `CHUNK_OVERLAP = 250` characters

### Retrieval relevance threshold (important!)

`top_k` alone is **not** enough. ChromaDB always returns the K *nearest*
chunks even when none of them are actually similar to the question — so a
totally out-of-scope question (e.g. "What is React Native?" against a
course on MVC) would still pull back the closest MVC chunk and pretend
it's relevant.

To prevent that, the service filters chunks by **cosine distance**:

- `RAG_MAX_DISTANCE = 0.45` — keep chunks where distance ≤ this value
  (smaller = more similar). Anything farther is discarded.
- `RAG_MIN_RELEVANT_CHUNKS = 1` — minimum number of chunks that must
  pass the filter; otherwise the service returns the standard
  Vietnamese fallback `"Không đủ thông tin trong tài liệu để trả lời câu hỏi này."`
  and `sources == []`. The LLM is **not** called in that case.

The right threshold depends on your documents and your embedding model:

| Symptom | Adjustment |
| --- | --- |
| Off-topic questions still get confident answers | **Lower** `RAG_MAX_DISTANCE` (e.g. 0.35) |
| Legitimate questions get the fallback too often | **Raise** `RAG_MAX_DISTANCE` (e.g. 0.55) |
| You want at least N strong supporting chunks | **Raise** `RAG_MIN_RELEVANT_CHUNKS` |

The `sources[].distance` field is exposed in the `/rag/ask` response to
make tuning easier.

### Embedding
An **embedding** is a list of floats (a vector) that represents the *meaning*
of a text. Two texts with similar meaning land near each other in vector
space, even if they use different words. We use **BAAI/bge-m3** — a strong
multilingual embedding model that handles Vietnamese + English very well.

### Vector database
A **vector database** stores `(embedding, metadata, text)` rows and answers
"give me the K nearest neighbors of this query vector" queries in
milliseconds. We use **ChromaDB** locally — it runs in-process, persists to a
folder on disk, and has no extra server to install.

### Why BAAI/bge-m3?
- multilingual (works well in Vietnamese and English)
- strong on academic content
- 1024-dimensional dense vectors (compact + fast)

### Why ChromaDB?
- no external server, runs in your Python process
- persists to a folder (`./chroma_db`)
- simple API perfect for an MVP

---

## Architecture overview

```
PDF/DOCX/PPTX/TXT
       │
       ▼
[document_loader]  ── pages ──►  [chunking_service]
                                       │
                                       ▼
                                  text chunks
                                       │
                                       ▼
                              [embedding_service]
                                       │
                                       ▼
                           vectors + metadata
                                       │
                                       ▼
                            [vector_store_service]
                                       │
                                       ▼
                                   ChromaDB

   Question ── embed ── search ──► top-K chunks ── prompt ──► [llm_service] ──► answer
```

## Folder structure

```
Retrieval-Augmented-Generation-PRN222/
├── app/
│   ├── main.py                     # FastAPI app entry point
│   ├── api/
│   │   └── rag_routes.py           # HTTP routes (thin controllers)
│   ├── core/
│   │   ├── config.py               # Loads .env into a typed Settings object
│   │   └── security.py             # Optional X-API-Key dependency
│   ├── models/
│   │   ├── rag_request.py          # Pydantic request models
│   │   └── rag_response.py         # Pydantic response models
│   ├── services/
│   │   ├── document_loader.py      # Reads PDF/DOCX/PPTX/TXT
│   │   ├── chunking_service.py     # Fixed-size chunking with overlap
│   │   ├── embedding_service.py    # Wraps BAAI/bge-m3 (singleton)
│   │   ├── vector_store_service.py # ChromaDB persistence + search
│   │   ├── llm_service.py          # Mock / real LLM generation
│   │   └── rag_service.py          # MAIN PIPELINE (index + ask)
│   ├── repositories/
│   │   └── vector_repository.py    # Pass-through over the vector store
│   └── utils/
│       ├── file_utils.py
│       └── text_utils.py
├── storage/documents/              # Place document files here
├── chroma_db/                      # ChromaDB persistence directory
├── tests/                          # pytest tests
├── dotnet-integration-samples/     # C# client for the PRN222 .NET API
├── .env.example
├── requirements.txt
├── Dockerfile
└── docker-compose.yml
```

### Important files in one sentence

| File | Purpose |
| --- | --- |
| `app/main.py` | Boots FastAPI, mounts routers, sets CORS + error handler. |
| `app/core/config.py` | Reads every setting from `.env` (single source of truth). |
| `app/services/document_loader.py` | Turns any supported file into a list of pages. |
| `app/services/chunking_service.py` | Splits pages into overlapping chunks. |
| `app/services/embedding_service.py` | Loads BAAI/bge-m3 once and embeds text. |
| `app/services/vector_store_service.py` | Persists + searches vectors in ChromaDB. |
| `app/services/llm_service.py` | Generates the final answer (mock or real LLM). |
| `app/services/rag_service.py` | Orchestrates the whole RAG pipeline. |
| `app/api/rag_routes.py` | HTTP layer — only translates JSON ↔ service calls. |

---

## Install & run locally

> Requires **Python 3.10+** (3.11 recommended).

```bash
# 1. Clone & enter the project
cd Retrieval-Augmented-Generation-PRN222

# 2. Create a virtual environment
python -m venv .venv
# Windows:   .venv\Scripts\activate
# macOS/Lin: source .venv/bin/activate

# 3. Install dependencies (this can take several minutes the first time
#    because torch / FlagEmbedding are heavy).
pip install -r requirements.txt

# 4. Copy the example env file and edit if needed
cp .env.example .env

# 5. Run the server (reload mode for development)
uvicorn app.main:app --reload --port 8000
```

Visit:
- API docs:    http://localhost:8000/docs
- Health:      http://localhost:8000/health

> The first call that needs the embedding model will download **BAAI/bge-m3**
> (~2 GB). This is normal and happens only once.

### Run tests

```bash
pytest
```

The tests mock the heavy parts (no model download, no real ChromaDB writes
outside a temp directory).

---

## Run with Docker

```bash
# Build & start
docker compose up --build

# Stop
docker compose down
```

`./chroma_db` and `./storage` are mounted into the container so your indexed
data persists across rebuilds.

---

## API contract

All RAG routes are under `/rag`. If you set `API_KEY` in `.env`, you must send
the header `X-API-Key: <value>` on every call.

### `GET /health`

Liveness probe.

```json
{ "status": "ok", "service": "Retrieval-Augmented-Generation-PRN222" }
```

### `POST /rag/index-document`

Read a document from disk, chunk it, embed the chunks, and store them.
Re-indexes idempotently (old chunks of the same `documentId` are removed first).

Request:
```json
{
  "documentId": "doc_001",
  "courseCode": "PRN222",
  "chapter": "Chapter 1",
  "filePath": "./storage/documents/sample.pdf",
  "fileName": "PRN222_Chapter_1.pdf"
}
```

Response:
```json
{
  "documentId": "doc_001",
  "status": "indexed",
  "totalChunks": 25,
  "message": "Document indexed successfully."
}
```

Errors:
- `400` — invalid path, unsupported extension, etc.
- `422` — file exists but cannot be parsed.

### `POST /rag/ask`

Answer a question grounded in the indexed documents (filtered by
`documentId` + `courseCode`).

Request:
```json
{
  "sessionId": "session_001",
  "userId": "user_001",
  "courseCode": "PRN222",
  "documentId": "doc_001",
  "question": "MVC trong ASP.NET Core là gì?",
  "topK": 5
}
```

Response:
```json
{
  "answer": "MVC là mô hình chia ứng dụng thành Model, View và Controller...",
  "sources": [
    {
      "documentId": "doc_001",
      "fileName": "PRN222_Chapter_1.pdf",
      "pageNumber": 3,
      "chunkIndex": 4,
      "text": "MVC separates an application into Model, View and Controller..."
    }
  ]
}
```

If retrieval finds nothing, the answer is exactly:
`Không đủ thông tin trong tài liệu để trả lời câu hỏi này.`

### `GET /rag/documents/{documentId}/status`

```json
{ "documentId": "doc_001", "indexed": true, "totalChunks": 25 }
```

### `DELETE /rag/documents/{documentId}`

```json
{ "documentId": "doc_001", "status": "deleted" }
```

---

## curl examples

Health:
```bash
curl -X GET http://localhost:8000/health
```

Index a document:
```bash
curl -X POST http://localhost:8000/rag/index-document \
  -H "Content-Type: application/json" \
  -d '{
    "documentId": "doc_001",
    "courseCode": "PRN222",
    "chapter": "Chapter 1",
    "filePath": "./storage/documents/sample.pdf",
    "fileName": "sample.pdf"
  }'
```

Ask a question:
```bash
curl -X POST http://localhost:8000/rag/ask \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "session_001",
    "userId": "user_001",
    "courseCode": "PRN222",
    "documentId": "doc_001",
    "question": "MVC là gì?",
    "topK": 5
  }'
```

If you set `API_KEY=secret` in `.env`, add `-H "X-API-Key: secret"` to every
curl above.

---

## How the .NET API should call this service

See `dotnet-integration-samples/`:

- `IRagClient.cs` — the contract
- `RagClient.cs` — `HttpClient` implementation (handles `X-API-Key`)
- `RagIndexRequest.cs` / `RagIndexResponse.cs`
- `RagAskRequest.cs` / `RagAskResponse.cs`

Register in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IRagClient, RagClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Rag:BaseUrl"]!);
    var apiKey = builder.Configuration["Rag:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});
```

`appsettings.json`:

```json
{
  "Rag": {
    "BaseUrl": "http://localhost:8000",
    "ApiKey": ""
  }
}
```

The .NET API only needs **four** endpoints from this service:
1. `POST /rag/index-document` — when a teacher uploads a document.
2. `POST /rag/ask` — when a student asks a question.
3. `GET /rag/documents/{id}/status` — to display indexing state.
4. `DELETE /rag/documents/{id}` — to remove a document.

---

## Troubleshooting

### `pip install` fails on torch
`FlagEmbedding` and `sentence-transformers` both depend on PyTorch. On
Windows without a CUDA GPU, install the CPU build first:

```bash
pip install torch --index-url https://download.pytorch.org/whl/cpu
pip install -r requirements.txt
```

### The first request takes forever
The embedding model **BAAI/bge-m3** (~2 GB) is downloaded on the first call
that needs it. The download happens once and is cached under
`~/.cache/huggingface/`. Subsequent calls are fast.

### `File not found` when calling `/rag/index-document`
The `filePath` is resolved by the **Python service**, not the .NET API.
- If you run Python natively, the path must be valid on your machine.
- If you run Python via Docker, the path must be valid inside the container
  (mount your storage folder via `docker-compose.yml`).

### ChromaDB seems to lose data after restart
Make sure `CHROMA_PERSIST_DIR` is a **persistent** directory:
- locally: a real folder (`./chroma_db`).
- Docker: mount it as a volume (already done in `docker-compose.yml`).

### `401 Unauthorized`
You set `API_KEY` in `.env` but forgot to include the `X-API-Key` header
on the request. Either remove `API_KEY` (local development) or add the header.

### "No relevant context found" / fallback Vietnamese reply
The retrieval step returned nothing **relevant enough** for this
`documentId` + `courseCode`. Check, in order:

1. Did `/rag/index-document` complete successfully for the same `documentId`?
2. Are you passing the **exact same** `courseCode` you used at indexing time?
3. Is `GET /rag/documents/{id}/status` reporting `indexed: true`?
4. Is `RAG_MAX_DISTANCE` too strict? Temporarily set it to e.g. `0.6` in
   `.env` and try again. If the answer now appears, your real corpus
   simply has a higher baseline distance than 0.45 — pick a value
   between the two and use that.
5. Is `RAG_MIN_RELEVANT_CHUNKS` set higher than the number of relevant
   chunks your document actually contains for that question?

### "Could not load the embedding model"
Both `FlagEmbedding` and `sentence-transformers` failed to import the model.
Most common causes:
- `pip install` did not finish — re-run it.
- Disk full (the model is ~2 GB).
- No internet on the machine that runs the service the first time (the model
  has to be downloaded once).

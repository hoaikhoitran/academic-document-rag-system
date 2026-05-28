# API Overview

The system exposes two HTTP surfaces:

1. The **Python RAG service** at `http://localhost:8000` — a JSON API consumed
   by the .NET app (or any future client).
2. The **ASP.NET Core MVC** routes — HTML pages, server-rendered, used by
   browsers.

---

## Python RAG service

Base URL: `http://localhost:8000`
Interactive docs (Swagger): `http://localhost:8000/docs`

All `/rag/*` routes require the header `X-API-Key: <value>` **only when**
`API_KEY` is set in `rag-service/.env`. The `/health` route is always public.

### `GET /health`

Liveness probe.

```json
{ "status": "ok", "service": "Retrieval-Augmented-Generation-PRN222" }
```

### `POST /rag/index-document`

Read a document from disk, chunk it, embed every chunk, and persist them in
ChromaDB. Idempotent — re-indexing the same `documentId` removes its previous
chunks first.

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

| Code | Meaning |
| --- | --- |
| 400 | Missing/invalid `filePath`, unsupported extension, file not found. |
| 422 | File exists but cannot be parsed (corrupted, password-protected, …). |
| 500 | Unexpected server error (see logs). |

### `POST /rag/ask`

Answer a question grounded in the documents already indexed for a given
`(courseCode, documentId)` pair.

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
      "text": "MVC separates an application into Model, View and Controller...",
      "distance": 0.21
    }
  ]
}
```

If retrieval finds nothing **relevant enough** (controlled by
`RAG_MAX_DISTANCE` and `RAG_MIN_RELEVANT_CHUNKS`), the service returns the
fixed Vietnamese fallback:

```json
{
  "answer": "Không đủ thông tin trong tài liệu để trả lời câu hỏi này.",
  "sources": []
}
```

The LLM is **not** called in that case.

### `GET /rag/documents/{documentId}/status`

```json
{ "documentId": "doc_001", "indexed": true, "totalChunks": 25 }
```

### `DELETE /rag/documents/{documentId}`

Remove every chunk + vector belonging to a document.

```json
{ "documentId": "doc_001", "status": "deleted" }
```

### Quick curl examples

```bash
# Health
curl http://localhost:8000/health

# Index a document (add  -H "X-API-Key: <key>"  if API_KEY is set)
curl -X POST http://localhost:8000/rag/index-document \
  -H "Content-Type: application/json" \
  -d '{
    "documentId": "doc_001",
    "courseCode": "PRN222",
    "chapter": "Chapter 1",
    "filePath": "./storage/documents/sample.pdf",
    "fileName": "sample.pdf"
  }'

# Ask
curl -X POST http://localhost:8000/rag/ask \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "s1",
    "userId": "u1",
    "courseCode": "PRN222",
    "documentId": "doc_001",
    "question": "MVC là gì?",
    "topK": 5
  }'
```

---

## ASP.NET Core MVC routes

The MVC app is a server-rendered web UI (no public JSON API). The default
route is `{controller=Auth}/{action=Login}/{id?}`.

| Method | Path | Controller | Purpose |
| --- | --- | --- | --- |
| GET | `/Auth/Login` | `AuthController` | Login form. |
| POST | `/Auth/Login` | `AuthController` | Validate credentials, set session. |
| GET | `/Auth/Logout` | `AuthController` | Clear session. |
| GET | `/Courses` | `CoursesController` | List courses. |
| GET/POST | `/Courses/Create` | `CoursesController` | Create a course. |
| GET/POST | `/Courses/Edit/{id}` | `CoursesController` | Edit a course. |
| GET | `/Courses/Delete/{id}` | `CoursesController` | Delete a course. |
| GET | `/Documents/Upload` | `DocumentsController` | Upload form. |
| POST | `/Documents/Upload` | `DocumentsController` | Save file + call RAG `/rag/index-document`. |
| GET | `/Chat` | `ChatController` | List indexed documents to chat with. |
| GET | `/Chat/Ask?documentId=…` | `ChatController` | Question form. |
| POST | `/Chat/Ask` | `ChatController` | Call RAG `/rag/ask`, persist `ChatSession` + `ChatMessage`, render answer view. |
| GET | `/Home/Index` | `HomeController` | Landing page. |

### How the MVC layer calls the RAG service

[`RagApiClient`](../dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.Services/RagIntegration/RagApiClient.cs)
is registered via `AddHttpClient<IRagClient, RagApiClient>(...)` in
[`DependencyInjection.cs`](../dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.Services/DependencyInjection.cs).
It reads `RagService:BaseUrl` from `appsettings.json` and sets a 180-second
HTTP timeout (indexing large PDFs can be slow on the first model-load run).

> **TODO** — wiring the `RagService:ApiKey` value as a default
> `X-API-Key` header in `AddHttpClient(...)` is not yet implemented in
> `DependencyInjection.cs`. If you set `API_KEY` in the Python service,
> add the header manually for now.

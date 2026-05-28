# Architecture

This project is a small **academic document RAG system**. It is split into three
independent components that can be developed, deployed, and tested in isolation.

```
                         ┌─────────────────────────────┐
                         │   Browser (Teacher /        │
                         │   Student / Admin)          │
                         └──────────────┬──────────────┘
                                        │ HTTPS
                                        ▼
        ┌───────────────────────────────────────────────────────────┐
        │   ASP.NET Core 8 MVC  —  AcademicDocumentRagSystem.MVC    │
        │   • Auth / Courses / Documents / Chat controllers         │
        │   • Razor views (Bootstrap)                               │
        │   • Session-based authentication                          │
        └───────┬──────────────────────────────────────────┬────────┘
                │ EF Core                                  │ HttpClient
                ▼                                          ▼
   ┌──────────────────────┐                  ┌────────────────────────────┐
   │  SQL Server          │                  │  FastAPI RAG service       │
   │  AcademicRagMgmt DB  │                  │  (Python 3.10+)            │
   │  Accounts / Courses  │                  │  /rag/index-document       │
   │  Documents / Chat    │                  │  /rag/ask                  │
   │  ChatSessions /      │                  │  /rag/documents/{id}/*     │
   │  ChatMessages        │                  └─────────────┬──────────────┘
   └──────────────────────┘                                │
                                                           ▼
                                    ┌──────────────────────────────────┐
                                    │  ChromaDB (local persistent)     │
                                    │  ./chroma_db                     │
                                    │                                  │
                                    │  Embedding model: BAAI/bge-m3    │
                                    │  (loaded once, in-process)       │
                                    └──────────────────────────────────┘
```

## Components

### 1. ASP.NET Core 8 MVC (`dotnet-mvc/HoaiKhoi_SE1950_A01`)

A classic 3-layer .NET solution:

| Project | Responsibility |
| --- | --- |
| `AcademicDocumentRagSystem.MVC` | Presentation layer — controllers, Razor views, `Program.cs`. |
| `AcademicDocumentRagSystem.Services` | Business logic, DTOs, dependency injection, RAG HTTP client. |
| `AcademicDocumentRagSystem.DataAccess` | EF Core `DbContext`, entity models, repositories. |

The MVC layer never talks to ChromaDB directly. It only persists *metadata*
(file name, course, upload status, chat history) into SQL Server and delegates
indexing and question-answering to the Python service via
[`RagApiClient`](../dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.Services/RagIntegration/RagApiClient.cs).

### 2. Python RAG service (`rag-service/`)

A FastAPI application that owns the entire RAG pipeline. It is intentionally
stateless except for ChromaDB on disk and is reusable from any client that
speaks HTTP/JSON.

Internal layout:

| Folder | Responsibility |
| --- | --- |
| `app/api` | Thin HTTP routes (controllers). |
| `app/core` | Config loaded from `.env` + API-key guard. |
| `app/models` | Pydantic request/response schemas. |
| `app/services` | The RAG pipeline (loader → chunker → embedder → vector store → LLM). |
| `app/repositories` | Pass-through layer above the vector store. |
| `app/utils` | Small text/file helpers. |
| `tests/` | Pytest unit tests for chunking, health, relevance threshold. |

### 3. Database (`database/AcademicRagManagement.sql`)

A SQL Server schema with five tables: `Accounts`, `Courses`, `Documents`,
`ChatSessions`, `ChatMessages`. See [database.md](database.md) for the full
schema description.

## Request flows

### Upload + index a document

```
Teacher → POST /Documents/Upload         (.NET MVC)
       1. DocumentService saves file to disk (FileStorage:DocumentFolder)
       2. Inserts a Document row (IndexStatus = "Processing")
       3. Calls RagApiClient.IndexDocumentAsync(...)
                ↓
                  POST /rag/index-document   (Python RAG service)
                  load_document → chunk_pages → embedding_service →
                  vector_store_service.add_chunks  →  ChromaDB
                ↑
       4. Updates Document row (IndexStatus = "Indexed", TotalChunks = N)
```

### Ask a question

```
Student → POST /Chat/Ask                  (.NET MVC)
       1. ChatService creates a ChatSession row
       2. Calls RagApiClient.AskAsync(...)
                ↓
                  POST /rag/ask              (Python RAG service)
                  embed question → vector_store.search →
                  distance filter → llm_service.generate_answer
                ↑
       3. Saves ChatMessage row (Question, Answer, SourcesJson)
       4. Returns Answer view with cited sources
```

## Why split into two services?

* **Language fit** — the embedding + vector ecosystem is much richer in Python.
* **Independent scaling** — the RAG service is CPU/RAM heavy (BAAI/bge-m3 is
  ~2 GB); the .NET app is lightweight.
* **Reusability** — any future client (mobile, other web app) can call the RAG
  service over HTTP without going through .NET.

## Cross-cutting concerns

* **Authentication** — session-based in the .NET MVC layer; the RAG service
  uses a simple optional `X-API-Key` header on every `/rag/*` route.
* **Configuration** — `appsettings.json` on the .NET side, `.env` on the
  Python side. Both are read at startup.
* **Persistence** — SQL Server for relational metadata, ChromaDB for vectors.
* **Idempotency** — re-indexing the same `documentId` removes its old chunks
  first, so repeating the operation never duplicates data.

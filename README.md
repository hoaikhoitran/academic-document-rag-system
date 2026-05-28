# Academic Document RAG System

A hybrid **ASP.NET Core 8 MVC + Python FastAPI** application that lets
teachers upload course documents (PDF / DOCX / PPTX / TXT) and lets students
ask questions answered **only** from the indexed material — a classic
**Retrieval-Augmented Generation** workflow.

The Python service handles chunking, embedding, vector storage, and answer
generation. The .NET MVC layer handles authentication, courses, document
metadata, and chat history.

---

## Tech stack

| Layer | Stack |
| --- | --- |
| Web UI | ASP.NET Core 8 MVC (Razor, Bootstrap) |
| Business / DI | C# 12 — `Services` class library (DI container, HttpClient) |
| Data access | EF Core 8, SQL Server |
| RAG service | Python 3.10/3.11, FastAPI, Uvicorn, Pydantic v2 |
| Embeddings | [BAAI/bge-m3](https://huggingface.co/BAAI/bge-m3) via `FlagEmbedding` (fallback `sentence-transformers`) |
| Vector DB | ChromaDB (local, persistent, cosine distance) |
| Document parsing | PyMuPDF, python-docx, python-pptx |
| LLM | Local **mock LLM** by default (deterministic, no API key required); provider hooks scaffolded for OpenAI / Gemini / Ollama |
| Containerization | Docker + Docker Compose (RAG service) |

## Features

* Teacher / student / admin login with session-based auth.
* Course CRUD.
* Document upload that automatically:
  * saves the file to disk,
  * tracks lifecycle (`UploadStatus`, `IndexStatus`),
  * indexes the file into ChromaDB through the Python service.
* Question answering grounded in a chosen document:
  * embeds the question,
  * retrieves the top-K most similar chunks,
  * filters by a configurable cosine-distance threshold,
  * returns either a grounded answer + cited sources **or** the
    standard Vietnamese fallback `"Không đủ thông tin trong tài liệu để
    trả lời câu hỏi này."` when retrieval is too weak.
* Persistent chat history (`ChatSessions`, `ChatMessages`) with serialized
  source citations.
* OpenAPI / Swagger docs for the RAG service at `/docs`.
* Idempotent re-indexing (re-uploading a document never duplicates chunks).
* Pytest test suite for the RAG service.
* Dockerfile + docker-compose for the RAG service.

## Quick start

> Full instructions live in [`docs/setup-guide.md`](docs/setup-guide.md).

```powershell
# 1. Database — run database/AcademicRagManagement.sql in SSMS

# 2. Python RAG service
cd rag-service
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
Copy-Item .env.example .env
uvicorn app.main:app --reload --port 8000   # http://localhost:8000/docs

# 3. .NET MVC app (in a separate terminal)
cd dotnet-mvc/HoaiKhoi_SE1950_A01
dotnet restore
dotnet run --project AcademicDocumentRagSystem.MVC
```

Default seeded logins (created by the SQL script):

| Role | Email | Password |
| --- | --- | --- |
| Teacher | `teacher@academicrag.org` | `@@abc123@@` |
| Student | `student@academicrag.org` | `@@abc123@@` |

## Folder structure

```
Basic-Retrieval-Augmented-Generation/
├── docs/                    # All documentation (architecture, setup, API, DB, deployment)
├── database/                # SQL Server schema + seed data
├── dotnet-mvc/              # ASP.NET Core 8 MVC solution (3 projects)
│   └── HoaiKhoi_SE1950_A01/
│       ├── AcademicDocumentRagSystem.MVC/         # Controllers / Views / Program.cs
│       ├── AcademicDocumentRagSystem.Services/    # Business logic, DTOs, RAG HttpClient
│       └── AcademicDocumentRagSystem.DataAccess/  # EF Core DbContext + repositories
└── rag-service/             # Python FastAPI RAG microservice
    ├── app/                 # api / core / models / services / repositories / utils
    ├── tests/               # pytest suite
    ├── storage/documents/   # Uploaded source files
    ├── chroma_db/           # ChromaDB persistence directory
    └── dotnet-integration-samples/   # Reference C# client
```

A deeper breakdown is in [`docs/project-structure.md`](docs/project-structure.md).

## How to run

| Component | Command | URL |
| --- | --- | --- |
| Database | execute `database/AcademicRagManagement.sql` once | `(local)` SQL Server |
| RAG service (dev) | `uvicorn app.main:app --reload --port 8000` | http://localhost:8000 (Swagger at `/docs`) |
| RAG service (Docker) | `docker compose up --build` (from `rag-service/`) | http://localhost:8000 |
| MVC app | `dotnet run --project AcademicDocumentRagSystem.MVC` | https://localhost:7xxx (printed by `dotnet run`) |

Configuration files you may need to edit:

* [`rag-service/.env`](rag-service/.env.example) — RAG service settings.
* [`dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.MVC/appsettings.json`](dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.MVC/appsettings.json)
  — SQL connection, RAG base URL, file storage folder, admin login.

## Documentation

| Doc | Description |
| --- | --- |
| [`docs/architecture.md`](docs/architecture.md) | High-level architecture, components, and request flows. |
| [`docs/setup-guide.md`](docs/setup-guide.md) | Step-by-step setup for the database, RAG service, and MVC app. |
| [`docs/api-overview.md`](docs/api-overview.md) | Python `/rag/*` endpoints + MVC routes. |
| [`docs/database.md`](docs/database.md) | SQL Server schema, tables, indexes, and seed data. |
| [`docs/deployment.md`](docs/deployment.md) | Docker / Compose for the RAG service + production hardening checklist. |
| [`docs/project-structure.md`](docs/project-structure.md) | Annotated folder tree and conventions. |
| [`rag-service/README.md`](rag-service/README.md) | Deep dive into the RAG service itself (concepts, tuning, troubleshooting). |

## Screenshots

> TODO — add screenshots once captured:
>
> * `docs/screenshots/login.png`
> * `docs/screenshots/courses-index.png`
> * `docs/screenshots/document-upload.png`
> * `docs/screenshots/chat-ask.png`
> * `docs/screenshots/chat-answer.png`
> * `docs/screenshots/swagger.png`

## Contributors

| Name | Role |
| --- | --- |
| [@khoiYeuMe](https://github.com/khoiYeuMe) | Author / maintainer |

> Open a pull request or issue if you would like to contribute.

## License

TODO — no license file is currently provided. Add one before publishing the
repository publicly.

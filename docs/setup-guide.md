# Setup Guide

This guide walks through running the full stack locally:

1. SQL Server database
2. Python RAG service
3. ASP.NET Core 8 MVC application

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 8.0+ |
| Python | 3.10 or 3.11 |
| SQL Server | 2019+ (Express, Developer, or LocalDB) |
| Visual Studio 2022 / Rider / VS Code | latest |
| Docker (optional) | for running the RAG service in a container |

> Disk space: the embedding model **BAAI/bge-m3** is ~2 GB and is downloaded
> on first use of the RAG service.

---

## 1. Set up the database

The SQL script is in [`database/AcademicRagManagement.sql`](../database/AcademicRagManagement.sql).

Open it in SQL Server Management Studio (or Azure Data Studio) and execute it.
It will:

* drop and recreate a database named `AcademicRagManagement`
* create tables: `Accounts`, `Courses`, `Documents`, `ChatSessions`, `ChatMessages`
* seed one course (`PRN222`) and two accounts (teacher + student)

Default seeded accounts (password is the same for both):

| Role | Email | Password |
| --- | --- | --- |
| Teacher | `teacher@academicrag.org` | `@@abc123@@` |
| Student | `student@academicrag.org` | `@@abc123@@` |

> The MVC app also reads an extra admin login from `appsettings.json`
> (`AdminAccount:Email` / `AdminAccount:Password`).

---

## 2. Start the Python RAG service

```powershell
cd rag-service

# Create + activate a virtual environment
python -m venv .venv
.venv\Scripts\activate          # PowerShell

# Install dependencies (first install can take several minutes — torch is heavy)
pip install -r requirements.txt

# Create a local .env from the example
Copy-Item .env.example .env

# Run the API
uvicorn app.main:app --reload --port 8000
```

Then visit:

* Swagger UI — http://localhost:8000/docs
* Health probe — http://localhost:8000/health

### Environment variables (`rag-service/.env`)

See [`.env.example`](../rag-service/.env.example) for the canonical list. Key
values:

```dotenv
APP_NAME=Retrieval-Augmented-Generation-PRN222
APP_ENV=development
API_KEY=                       # leave empty for local dev

CHROMA_PERSIST_DIR=./chroma_db
CHROMA_COLLECTION_NAME=prn222_documents

EMBEDDING_MODEL_NAME=BAAI/bge-m3

CHUNK_SIZE=1500
CHUNK_OVERLAP=250
DEFAULT_TOP_K=5

RAG_MAX_DISTANCE=0.45          # relevance filter (cosine distance)
RAG_MIN_RELEVANT_CHUNKS=1

MOCK_LLM=true                  # uses a deterministic local mock — no paid API needed
LLM_PROVIDER=mock
LLM_API_KEY=
LLM_MODEL_NAME=

PORT=8000
```

> When `API_KEY` is set, every `/rag/*` request must include the
> `X-API-Key: <value>` header. Health (`/health`) is exempt.

### Run the tests

```powershell
cd rag-service
pytest
```

Tests mock the heavy parts — no model download, no real ChromaDB writes
outside a temp directory.

---

## 3. Start the .NET MVC web app

```powershell
cd dotnet-mvc/HoaiKhoi_SE1950_A01

# Restore + build the whole solution
dotnet restore
dotnet build

# Run the MVC project
dotnet run --project AcademicDocumentRagSystem.MVC
```

By default ASP.NET will print the URLs it is listening on, e.g.
`https://localhost:7xxx` and `http://localhost:5xxx`. The home route redirects
to `/Auth/Login`.

### Configuration (`appsettings.json`)

Located at
[`AcademicDocumentRagSystem.MVC/appsettings.json`](../dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.MVC/appsettings.json).

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "server=(local); database=AcademicRagManagement; uid=sa; pwd=1234567890; TrustServerCertificate=True;"
  },
  "AdminAccount": {
    "Email": "admin@AcademicDocumentRagSystem.org",
    "Password": "@@abc123@@"
  },
  "RagService": {
    "BaseUrl": "http://localhost:8000",
    "ApiKey": ""
  },
  "FileStorage": {
    "DocumentFolder": "D:\\PRN222_Uploads\\documents"
  }
}
```

Update each key to match your environment:

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string. |
| `AdminAccount:Email` / `Password` | Built-in admin credentials. |
| `RagService:BaseUrl` | URL where the Python service is listening. |
| `RagService:ApiKey` | Must match `API_KEY` in `rag-service/.env` (if set). |
| `FileStorage:DocumentFolder` | Absolute path where uploaded files are stored. The Python service must be able to read this path. |

> **Important** — the .NET app and the Python service must share access to the
> `FileStorage:DocumentFolder` directory. If you run Python in Docker, mount
> that folder into the container.

---

## 4. End-to-end smoke test

1. Log in at `/Auth/Login` as the teacher account.
2. Go to **Documents → Upload** and upload a PDF/DOCX/PPTX/TXT.
   The page should redirect with `"Document uploaded and indexed successfully."`
3. Go to **Chat**, pick the document, and ask a question.
4. The answer page should show the answer plus a list of cited sources
   (`fileName`, `pageNumber`, snippet).

If anything goes wrong, see the
[troubleshooting section](../rag-service/README.md#troubleshooting)
in the RAG service README — it has detailed solutions for the most common
issues (model download, file paths, threshold tuning, API-key errors).

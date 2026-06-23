# Academic Document RAG System

> GitHub: **[@hoaikhoitran](https://github.com/hoaikhoitran)** ·
> repo: [academic-document-rag-system](https://github.com/hoaikhoitran/academic-document-rag-system)

A hybrid **ASP.NET Core 8 + Python FastAPI** application that lets teachers
upload course documents (PDF / DOCX / PPTX / TXT) and lets students ask questions
answered **only** from the indexed material — a classic **Retrieval-Augmented
Generation** workflow.

The Python service handles chunking, embedding, vector storage, and answer
generation. The .NET layer handles authentication, courses, accounts, document
metadata, and chat history.

The repository ships **two interchangeable .NET front-ends** over one shared core:

| Front-end       | Folder                              | Course assignment    | Highlights                                                                  |
| --------------- | ----------------------------------- | -------------------- | --------------------------------------------------------------------------- |
| **MVC**         | `dotnet-mvc/HoaiKhoi_SE1950_A01/`   | PRN222 Assignment 01 | Controllers + Views. Preserved, untouched.                                  |
| **Razor Pages** | `dotnet-razor/HoaiKhoi_SE1950_A02/` | PRN222 Assignment 02 | PageModels + Pages, **SignalR** real-time courses, **SMTP** lecturer email. |

Both depend on the same `Services` → `DataAccess` layers, the same SQL Server
database, and the same Python RAG service. See
[`docs/razor-pages-a02.md`](docs/razor-pages-a02.md) for the A02 version.

---

## Tech stack

| Layer            | Stack                                                                                                 |
| ---------------- | ----------------------------------------------------------------------------------------------------- |
| Web UI (A01)     | ASP.NET Core 8 **MVC** (Razor views, Bootstrap)                                                       |
| Web UI (A02)     | ASP.NET Core 8 **Razor Pages** + **SignalR** + **SMTP** (Bootstrap)                                   |
| Business / DI    | C# 12 — `Services` class library (DI, HttpClient, EmailService)                                       |
| Data access      | EF Core 8, SQL Server, Repository pattern                                                             |
| RAG service      | Python 3.10/3.11, FastAPI, Uvicorn, Pydantic v2                                                       |
| Embeddings       | [BAAI/bge-m3](https://huggingface.co/BAAI/bge-m3) (`FlagEmbedding`, fallback `sentence-transformers`) |
| Vector DB        | ChromaDB (local, persistent, cosine distance)                                                         |
| Document parsing | PyMuPDF, python-docx, python-pptx                                                                     |
| LLM              | Local **mock LLM** by default (no API key); provider hooks for OpenAI / Gemini / Ollama               |

## Features

Shared across both front-ends:

- Session-based login for **Admin / Teacher (Lecturer) / Student** with role-based access.
- **Course** CRUD (admin) and **Account** management (admin).
- **Document upload** that saves the file, tracks `UploadStatus` / `IndexStatus`,
  and indexes it into ChromaDB through the Python service.
- **Grounded Q&A** over a chosen document: retrieves top-K chunks, filters by a
  cosine-distance threshold, and returns an answer **with cited sources** (or a
  fallback message when retrieval is too weak).
- Persistent chat history; idempotent re-indexing (never duplicates chunks).

Added in the **Razor Pages (A02)** version:

- **SignalR** — when an admin creates/updates/deletes a course, lecturers viewing
  the live course list see it update **without refreshing** ([`docs/signalr.md`](docs/signalr.md)).
- **SMTP** — when an admin creates a lecturer account, that lecturer is emailed a
  premium HTML welcome message with their credentials and assigned course
  ([`docs/smtp.md`](docs/smtp.md)). Email failures never break account creation.
- CRUD via **Bootstrap modals**, search, DataAnnotations + server-side validation,
  and clear success/warning/error messages.

## Architecture

```
        ┌───────────────────────────┐   ┌────────────────────────────┐
        │  ASP.NET Core 8 MVC        │   │  ASP.NET Core 8 Razor Pages│
        │  dotnet-mvc (A01)          │   │  dotnet-razor (A02)        │
        │  Controllers + Views       │   │  PageModels + Pages        │
        │                            │   │  + SignalR + SMTP          │
        └─────────────┬──────────────┘   └──────────────┬─────────────┘
                      └───────────────┬──────────────────┘
                                      ▼
                  AcademicDocumentRagSystem.Services        ── HttpClient ──►  FastAPI RAG service (Python)
                                      ▼                                         (chunk → embed → ChromaDB → answer)
                  AcademicDocumentRagSystem.DataAccess  ── EF Core ──►  SQL Server (AcademicRagManagement)
```

Dependencies flow downward only: `Presentation → Services → DataAccess`. Neither
front-end injects `DbContext`; all data access goes through services and
repositories. Full write-up in [`docs/architecture.md`](docs/architecture.md).

## Repository structure

```
academic-document-rag-system/
├── docs/                    # Documentation + screenshots (docs/images/)
├── database/                # SQL Server schema + seed data
├── dotnet-mvc/              # ASP.NET Core 8 MVC solution (Assignment 01)
│   └── HoaiKhoi_SE1950_A01/
│       ├── AcademicDocumentRagSystem.MVC/
│       ├── AcademicDocumentRagSystem.Services/
│       └── AcademicDocumentRagSystem.DataAccess/
├── dotnet-razor/            # ASP.NET Core 8 Razor Pages solution (Assignment 02)
│   └── HoaiKhoi_SE1950_A02/
│       ├── AcademicDocumentRagSystem.RazorPages/    # Pages + Hubs + wwwroot
│       ├── AcademicDocumentRagSystem.Services/      # copied + Email service/template
│       └── AcademicDocumentRagSystem.DataAccess/    # copied
└── rag-service/             # Python FastAPI RAG microservice
```

A deeper breakdown is in [`docs/project-structure.md`](docs/project-structure.md).

## How to run

### 1. Database (once)

Execute [`database/AcademicRagManagement.sql`](database/AcademicRagManagement.sql)
in SSMS / Azure Data Studio. It creates the `AcademicRagManagement` database and
seeds a course plus teacher/student accounts.

### 2. Python RAG service

```powershell
cd rag-service
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
Copy-Item .env.example .env
uvicorn app.main:app --reload --port 8000      # http://localhost:8000/docs
```

### 3a. Razor Pages app (Assignment 02)

```powershell
dotnet restore dotnet-razor/HoaiKhoi_SE1950_A02/HoaiKhoi_SE1950_A02.sln
dotnet build   dotnet-razor/HoaiKhoi_SE1950_A02/HoaiKhoi_SE1950_A02.sln
dotnet run --project dotnet-razor/HoaiKhoi_SE1950_A02/AcademicDocumentRagSystem.RazorPages
```

Open `https://localhost:7150/` — the **Login page is the default start page**.

### 3b. MVC app (Assignment 01) — optional

```powershell
dotnet run --project dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.MVC
```

### Test accounts

| Role    | Email                                 | Password     | Source                            |
| ------- | ------------------------------------- | ------------ | --------------------------------- |
| Admin   | `admin@AcademicDocumentRagSystem.org` | `@@abc123@@` | `appsettings.json → AdminAccount` |
| Teacher | `teacher@academicrag.org`             | `@@abc123@@` | seeded by the SQL script          |
| Student | `student@academicrag.org`             | `@@abc123@@` | seeded by the SQL script          |

## Configuration

- **SQL Server** — `ConnectionStrings:DefaultConnection` in each app's
  `appsettings.json`. Both apps point at the same database.
- **RAG service** — `RagService:BaseUrl` (and optional `ApiKey`).
- **SMTP (Razor A02)** — the `Smtp` section is **blank by default**; configure it
  via user-secrets (recommended) or `appsettings.Development.json`:

  ```powershell
  cd dotnet-razor/HoaiKhoi_SE1950_A02/AcademicDocumentRagSystem.RazorPages
  dotnet user-secrets init
  dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
  dotnet user-secrets set "Smtp:UserName" "you@gmail.com"
  dotnet user-secrets set "Smtp:Password" "your-app-password"
  dotnet user-secrets set "Smtp:FromEmail" "you@gmail.com"
  ```

> No real SMTP passwords, API keys, connection secrets, or absolute paths are
> committed — `appsettings.json` ships placeholders. See [`docs/smtp.md`](docs/smtp.md).

## SignalR — real-time course updates

The Razor app exposes a `CourseHub` at **`/hubs/courses`**. After a successful
admin Course create/update/delete, the Courses PageModel broadcasts a
`CoursesChanged` event via `IHubContext<CourseHub>`. The lecturer "Live Courses"
page (`wwwroot/js/course-realtime.js`) re-fetches **only** the course-table
fragment and swaps it in — the page is never reloaded. Verified end-to-end for
both create and delete. Details in [`docs/signalr.md`](docs/signalr.md).

## Screenshots

Real, current screenshots captured from the running Razor app by driving Google
Chrome over the **Chrome DevTools Protocol** (a dedicated Chrome DevTools MCP
server is not connected in this environment). Full set + capture notes in
[`docs/screenshots.md`](docs/screenshots.md).

**Authentication**

* **Login** — default start page (session login).
  ![Login](docs/images/login.png)

**Admin**

* **Admin dashboard** — course/account counts and quick links.
  ![Admin dashboard](docs/images/admin-dashboard.png)
* **Course management** — list, search, modal create/edit, delete.
  ![Admin courses](docs/images/admin-courses.png)
* **Create course modal** — Bootstrap modal with validation.
  ![Create course modal](docs/images/create-course-modal.png)
* **Account management** — students + lecturers with role/course/status filters.
  ![Account management](docs/images/account-management.png)
* **Create teacher account modal** — assigns a course; triggers the onboarding email.
  ![Create teacher account](docs/images/create-teacher-account.png)

**Teacher / Lecturer**

* **Lecturer dashboard** — live courses, documents, and Ask-RAG entry points.
  ![Teacher dashboard](docs/images/teacher-dashboard.png)
* **Live courses (SignalR)** — course catalogue that updates without refresh.
  ![Teacher live courses](docs/images/teacher-live-courses.png)
* **Upload document** — upload + index a course document.
  ![Teacher upload document](docs/images/teacher-upload-document.png)
* **My documents** — uploaded material with index status.
  ![Teacher documents](docs/images/teacher-documents.png)

**RAG / Chat**

* **Document picker** — choose an indexed document to ask about.
  ![Chat RAG picker](docs/images/chat-rag-picker.png)
* **Ask a question** — grounded answer + cited sources render inline.
  ![Chat RAG ask](docs/images/chat-rag-ask.png)

**SignalR live-update proof**

* The lecturer's course list **after** an admin created course `RTLIVE` in
  another browser — the row appeared with **no page reload**.
  ![SignalR live update](docs/images/signalr-live-update.png)

**SMTP onboarding email**

* The rendered `TeacherWelcome.html` premium email template.
  ![Welcome email](docs/images/email-teacher-welcome.png)

## Assignment 02 notes

- The A02 requirements (Razor Pages + SignalR + 3-layer + Repository pattern +
  CRUD/search/validation + SMTP) are implemented over the **existing** Academic
  Document RAG domain — Course = subject, Teacher = lecturer. It is **not**
  renamed to a News Management system.
- The MVC solution (`dotnet-mvc/HoaiKhoi_SE1950_A01`) is preserved unchanged and
  still builds.

## Documentation

| Doc                                                                                         | Description                                        |
| ------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| [`docs/razor-pages-a02.md`](docs/razor-pages-a02.md)                                        | Razor Pages (A02) overview, page map, auth.        |
| [`docs/signalr.md`](docs/signalr.md)                                                        | Real-time course updates.                          |
| [`docs/smtp.md`](docs/smtp.md)                                                              | Lecturer onboarding email + template architecture. |
| [`docs/screenshots.md`](docs/screenshots.md)                                                | Captured screenshots.                              |
| [`docs/architecture.md`](docs/architecture.md)                                              | High-level architecture and request flows.         |
| [`docs/setup-guide.md`](docs/setup-guide.md)                                                | Step-by-step setup (DB, RAG, MVC, Razor).          |
| [`docs/database.md`](docs/database.md)                                                      | SQL Server schema and seed data.                   |
| [`docs/project-structure.md`](docs/project-structure.md)                                    | Annotated folder tree.                             |
| [`docs/api-overview.md`](docs/api-overview.md) · [`docs/deployment.md`](docs/deployment.md) | RAG API endpoints; Docker/deploy.                  |
| [`rag-service/README.md`](rag-service/README.md)                                            | Deep dive into the RAG service.                    |

## Known limitations

- The lecturer welcome email is shown as a **rendered template screenshot**; a
  live Gmail/Outlook inbox capture is not included because it requires real SMTP
  credentials (configure them per [`docs/smtp.md`](docs/smtp.md) to send for real).
- The RAG Ask page requires the Python `rag-service` running and at least one
  indexed document; when the service is offline the page shows a graceful error
  instead of crashing.
- No license file is provided yet — add one before publishing publicly.


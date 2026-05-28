# Project Structure

```
Basic-Retrieval-Augmented-Generation/
├── README.md                        # Root portfolio README
├── docs/                            # ← you are here
│   ├── architecture.md
│   ├── setup-guide.md
│   ├── api-overview.md
│   ├── database.md
│   ├── deployment.md
│   └── project-structure.md
│
├── database/
│   └── AcademicRagManagement.sql    # SQL Server schema + seed data
│
├── dotnet-mvc/
│   └── HoaiKhoi_SE1950_A01/         # Visual Studio solution folder
│       ├── HoaiKhoi_SE1950_A01.sln
│       │
│       ├── AcademicDocumentRagSystem.MVC/        # Presentation layer
│       │   ├── Program.cs                        # ASP.NET pipeline + DI bootstrap
│       │   ├── appsettings.json                  # ConnStr / RagService / FileStorage
│       │   ├── Controllers/
│       │   │   ├── AuthController.cs             # Login / Logout
│       │   │   ├── ChatController.cs             # Pick document, ask question
│       │   │   ├── CoursesController.cs          # Courses CRUD
│       │   │   ├── DocumentsController.cs        # Upload + index a document
│       │   │   └── HomeController.cs
│       │   ├── Views/                            # Razor views
│       │   │   ├── Auth/Login.cshtml
│       │   │   ├── Chat/{Index,Ask,Answer}.cshtml
│       │   │   ├── Courses/{Index,Create,Edit}.cshtml
│       │   │   ├── Documents/Upload.cshtml
│       │   │   ├── Home/{Index,Privacy}.cshtml
│       │   │   └── Shared/{_Layout.cshtml,Error.cshtml,...}
│       │   ├── Models/ErrorViewModel.cs
│       │   └── wwwroot/                          # static assets
│       │
│       ├── AcademicDocumentRagSystem.Services/   # Business logic + DI
│       │   ├── DependencyInjection.cs            # Registers DbContext, HttpClient, services
│       │   ├── DTOs/
│       │   │   ├── Auth/{LoginDto,LoginResultDto}.cs
│       │   │   ├── Chat/{AskQuestionDto,ChatAnswerDto,IndexedDocumentDto}.cs
│       │   │   ├── Courses/{CourseDto,CreateCourseDto,UpdateCourseDto}.cs
│       │   │   ├── Documents/DocumentUploadDto.cs
│       │   │   └── Rag/{RagIndexRequest,RagIndexResponse,RagAskRequest,RagAskResponse,RagSourceDto}.cs
│       │   ├── Interfaces/                       # I*Service contracts
│       │   ├── Implementations/                  # AccountService, ChatService, CourseService, DocumentService
│       │   └── RagIntegration/                   # IRagClient + RagApiClient (HttpClient)
│       │
│       └── AcademicDocumentRagSystem.DataAccess/ # EF Core layer
│           ├── Models/
│           │   ├── AcademicRagDbContext.cs       # DbContext + OnModelCreating
│           │   ├── Account.cs
│           │   ├── ChatMessage.cs
│           │   ├── ChatSession.cs
│           │   ├── Course.cs
│           │   └── Document.cs
│           └── Repositories/
│               ├── Interfaces/                   # IAccountRepository, ICourseRepository, ...
│               └── Implementations/              # AccountRepository, CourseRepository, ...
│
└── rag-service/                     # Python FastAPI RAG service
    ├── README.md                    # Full service-specific README
    ├── Dockerfile
    ├── docker-compose.yml
    ├── requirements.txt
    ├── .env.example                 # All environment variables (annotated)
    │
    ├── app/
    │   ├── main.py                  # FastAPI app factory + CORS + error handler
    │   ├── api/
    │   │   └── rag_routes.py        # /rag/index-document, /rag/ask, /rag/documents/*
    │   ├── core/
    │   │   ├── config.py            # Typed Settings loaded from .env
    │   │   └── security.py          # Optional X-API-Key dependency
    │   ├── models/
    │   │   ├── rag_request.py       # Pydantic request models
    │   │   └── rag_response.py      # Pydantic response models
    │   ├── services/                # ← the RAG pipeline
    │   │   ├── document_loader.py   # PDF/DOCX/PPTX/TXT → pages
    │   │   ├── chunking_service.py  # pages → overlapping chunks
    │   │   ├── embedding_service.py # BAAI/bge-m3 singleton
    │   │   ├── vector_store_service.py # ChromaDB writes + nearest-neighbor search
    │   │   ├── llm_service.py       # Mock LLM (default) + provider scaffolding
    │   │   └── rag_service.py       # Orchestrator: index_document() + ask()
    │   ├── repositories/
    │   │   └── vector_repository.py # Pass-through over the vector store
    │   └── utils/
    │       ├── file_utils.py
    │       └── text_utils.py
    │
    ├── storage/documents/           # Put / receive document files here
    ├── chroma_db/                   # ChromaDB persistence (auto-created)
    ├── tests/                       # pytest suite
    │   ├── conftest.py
    │   ├── test_chunking_service.py
    │   ├── test_health.py
    │   └── test_relevance_threshold.py
    └── dotnet-integration-samples/  # Reference C# client (mirrors RagIntegration/ in the MVC solution)
```

## What each top-level folder is for

| Folder | Purpose |
| --- | --- |
| `database/` | The one-shot SQL Server schema. Run it once before starting the MVC app. |
| `dotnet-mvc/` | The ASP.NET Core 8 MVC web app — the user-facing application. |
| `rag-service/` | The Python FastAPI microservice that owns the RAG pipeline. |
| `docs/` | The documentation you're reading. |

## Conventions

* **Layered .NET solution** — Presentation (`MVC`) depends on `Services`,
  which depends on `DataAccess`. No reverse references.
* **Thin controllers** — both in .NET (`Controllers/`) and in Python
  (`app/api/rag_routes.py`). All real logic lives in services.
* **Configuration is external** — `.env` for Python, `appsettings.json` for
  .NET. Never hard-code secrets or paths in source files.
* **Idempotent indexing** — re-uploading the same `DocumentId` re-indexes
  cleanly because the RAG service deletes the previous chunks first.

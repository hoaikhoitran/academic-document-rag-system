# Database Overview

The MVC application persists relational metadata in **Microsoft SQL Server**.
Vectors (embeddings) are kept separately by the Python RAG service inside
ChromaDB.

* Database name: `AcademicRagManagement`
* DDL script: [`database/AcademicRagManagement.sql`](../database/AcademicRagManagement.sql)
* ORM: Entity Framework Core 8 (SQL Server provider)
* `DbContext`: [`AcademicRagDbContext`](../dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.DataAccess/Models/AcademicRagDbContext.cs)

Connection string is configured in
[`AcademicDocumentRagSystem.MVC/appsettings.json`](../dotnet-mvc/HoaiKhoi_SE1950_A01/AcademicDocumentRagSystem.MVC/appsettings.json)
under `ConnectionStrings:DefaultConnection`.

## Schema

```
┌──────────────┐         ┌──────────────┐         ┌──────────────────┐
│   Accounts   │         │   Courses    │         │    Documents     │
│──────────────│         │──────────────│         │──────────────────│
│ AccountId PK │◄─┐  ┌──►│ CourseId PK  │◄────────│ DocumentId PK    │
│ Email (uq)   │  │  │   │ Code (uq)    │         │ CourseId FK      │
│ Password     │  │  │   │ Name         │         │ CourseCode       │
│ FullName     │  │  │   │ Description  │         │ Chapter          │
│ Role (1/2)   │  │  │   │ Status       │         │ Title            │
│ Status       │  │  │   └──────────────┘         │ OriginalFileName │
│ CreatedAt    │  │  │                            │ StoredFileName   │
│ UpdatedAt    │  │  │                            │ FilePath         │
└──────────────┘  │  │                            │ FileType         │
                  │  │                            │ FileSize         │
                  │  │                            │ UploadStatus     │
                  │  │                            │ IndexStatus      │
                  │  │                            │ TotalChunks      │
                  │  │                            │ IndexError       │
                  │  │                            │ SubmittedByAcc.. │
                  │  │                            └────┬─────────────┘
                  │  │                                 │
                  │  │  ┌──────────────────┐           │
                  │  │  │  ChatSessions    │           │
                  │  │  │──────────────────│           │
                  │  └──│ ChatSessionId PK │           │
                  └─────│ AccountId FK     │           │
                        │ CourseId FK      │           │
                        │ DocumentId FK    │───────────┘
                        │ Title            │
                        │ CreatedAt        │
                        └────┬─────────────┘
                             │
                             │   ┌──────────────────┐
                             └──►│  ChatMessages    │
                                 │──────────────────│
                                 │ ChatMessageId PK │
                                 │ ChatSessionId FK │
                                 │ AccountId FK     │
                                 │ DocumentId FK    │
                                 │ Question         │
                                 │ Answer           │
                                 │ SourcesJson      │
                                 │ CreatedAt        │
                                 └──────────────────┘
```

## Tables

### `Accounts`

User accounts (teachers and students). The MVC app also accepts an extra
admin login defined in `appsettings.json` (no row needed in this table).

| Column | Type | Notes |
| --- | --- | --- |
| `AccountId` | `INT IDENTITY` PK | |
| `Email` | `NVARCHAR(255)` | unique |
| `Password` | `NVARCHAR(255)` | stored as plain text in the schema (TODO: hashing) |
| `FullName` | `NVARCHAR(150)` | |
| `Role` | `INT` | `1` = Student, `2` = Teacher |
| `Status` | `BIT` | active flag |
| `CreatedAt` | `DATETIME2` | default `sysutcdatetime()` |
| `UpdatedAt` | `DATETIME2` NULL | |

### `Courses`

| Column | Type | Notes |
| --- | --- | --- |
| `CourseId` | `INT IDENTITY` PK | |
| `Code` | `NVARCHAR(50)` | unique (e.g. `PRN222`) |
| `Name` | `NVARCHAR(200)` | |
| `Description` | `NVARCHAR(1000)` | optional |
| `Status` | `BIT` | active flag |
| `CreatedAt` | `DATETIME2` | default `sysutcdatetime()` |
| `UpdatedAt` | `DATETIME2` NULL | |

### `Documents`

Metadata for every uploaded file. The actual binary lives in the path defined
by `FileStorage:DocumentFolder`; the *vectors* live in ChromaDB.

| Column | Type | Notes |
| --- | --- | --- |
| `DocumentId` | `INT IDENTITY` PK | |
| `Title` | `NVARCHAR(255)` | |
| `Description` | `NVARCHAR(1000)` NULL | |
| `CourseId` | `INT` FK → `Courses` | |
| `CourseCode` | `NVARCHAR(50)` | duplicated for fast filtering |
| `Chapter` | `NVARCHAR(100)` NULL | |
| `OriginalFileName` | `NVARCHAR(255)` | |
| `StoredFileName` | `NVARCHAR(255)` | `{Guid}.ext` |
| `FilePath` | `NVARCHAR(1000)` | absolute path |
| `FileType` | `NVARCHAR(20)` | one of `.pdf` / `.docx` / `.pptx` / `.txt` |
| `ContentType` | `NVARCHAR(255)` NULL | original MIME type |
| `FileSize` | `BIGINT` | bytes |
| `UploadStatus` | `NVARCHAR(30)` | `Submitted` / `Approved` / `Rejected` / `Deleted` |
| `IndexStatus` | `NVARCHAR(30)` | `Pending` / `Processing` / `Indexed` / `Failed` / `Deleted` |
| `TotalChunks` | `INT` | number of chunks stored in ChromaDB |
| `IndexError` | `NVARCHAR(MAX)` NULL | error message if indexing failed |
| `SubmittedByAccountId` | `INT` NULL FK → `Accounts` | |
| `SubmittedByEmail` | `NVARCHAR(255)` NULL | snapshot of submitter email |
| `CreatedAt` / `UpdatedAt` / `ApprovedAt` / `IndexedAt` | `DATETIME2` | lifecycle timestamps |

### `ChatSessions`

One row per Q&A session a student starts against a specific document.

| Column | Type |
| --- | --- |
| `ChatSessionId` | `INT IDENTITY` PK |
| `AccountId` | `INT` FK → `Accounts` |
| `CourseId` | `INT` FK → `Courses` |
| `DocumentId` | `INT` FK → `Documents` |
| `Title` | `NVARCHAR(255)` NULL — first 80 chars of the first question |
| `CreatedAt` / `UpdatedAt` | `DATETIME2` |

### `ChatMessages`

One row per question/answer pair.

| Column | Type |
| --- | --- |
| `ChatMessageId` | `INT IDENTITY` PK |
| `ChatSessionId` | `INT` FK → `ChatSessions` |
| `AccountId` | `INT` FK → `Accounts` |
| `DocumentId` | `INT` FK → `Documents` |
| `Question` | `NVARCHAR(MAX)` |
| `Answer` | `NVARCHAR(MAX)` |
| `SourcesJson` | `NVARCHAR(MAX)` — serialized array of `{documentId, fileName, pageNumber, chunkIndex, text, distance}` returned by the RAG service |
| `CreatedAt` | `DATETIME2` default `sysutcdatetime()` |

## Indexes

Created by the DDL script for fast lookups in the common access paths:

* `IX_Accounts_Email`
* `IX_Courses_Code`
* `IX_Documents_CourseId`, `IX_Documents_CourseCode`, `IX_Documents_SubmittedByAccountId`, `IX_Documents_UploadStatus`, `IX_Documents_IndexStatus`
* `IX_ChatSessions_AccountId`, `IX_ChatSessions_DocumentId`
* `IX_ChatMessages_ChatSessionId`, `IX_ChatMessages_DocumentId`, `IX_ChatMessages_CreatedAt`

## Seed data

The DDL script inserts:

* one course — `PRN222 — Programming with .NET`
* one teacher — `teacher@academicrag.org`
* one student — `student@academicrag.org`

Both seeded accounts share the password `@@abc123@@`.

## Vector store (ChromaDB)

Embeddings live **outside** SQL Server, persisted on disk in
`rag-service/chroma_db/`. Each chunk row in ChromaDB carries this metadata:

```
documentId, courseCode, chapter, fileName, fileType,
pageNumber, chunkIndex, source
```

Stable chunk ID: `{documentId}::chunk::{chunkIndex}`. Re-indexing the same
`documentId` deletes its previous chunks first, so the operation is
idempotent.

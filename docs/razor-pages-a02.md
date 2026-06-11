# Razor Pages Version — PRN222 Assignment 02

`dotnet-razor/HoaiKhoi_SE1950_A02/` is a **separate ASP.NET Core 8 Razor Pages**
presentation layer for the Academic Document RAG System. It is built for PRN222
Assignment 02 and lives **alongside** the original MVC app (Assignment 01) — the
MVC solution under `dotnet-mvc/HoaiKhoi_SE1950_A01/` is left completely untouched.

The Razor app **reuses** the existing business and data layers (copied into the
Razor solution) so there is no duplicated logic, and it talks to the **same SQL
Server database** and the **same Python RAG service**.

```
dotnet-razor/HoaiKhoi_SE1950_A02/
├── HoaiKhoi_SE1950_A02.sln
├── AcademicDocumentRagSystem.RazorPages/   # NEW presentation layer (Razor Pages + SignalR)
├── AcademicDocumentRagSystem.Services/      # copied from MVC (+ Email service/template)
└── AcademicDocumentRagSystem.DataAccess/    # copied from MVC (+ Course search)
```

Dependency direction (unchanged 3-layer architecture):

```
RazorPages  →  Services  →  DataAccess  →  SQL Server
```

Razor PageModels **never** touch `DbContext` directly — every read/write goes
through a service, and every query lives in a repository.

## How it maps PRN222 A02 onto the existing domain

This is **not** a rewrite into "FU News Management". The A02 requirements are
satisfied over the existing Academic Document RAG domain:

| PRN222 A02 concept | This project |
| --- | --- |
| News article / category entity with CRUD | **Course** (subject) CRUD |
| Lecturer role | **Teacher / Lecturer** (role `2`) |
| Account management | **Account** management (students + lecturers) |
| Real-time list (SignalR) | Live **Course** list pushed to lecturers |
| Email notification (SMTP) | Lecturer onboarding email on account creation |

## Pages

| Area | Page (route) | Role | Purpose |
| --- | --- | --- | --- |
| Auth | `/Auth/Login` (also `/`) | anonymous | Default start page; session login |
| Auth | `/Auth/Logout` | any | Clears session |
| Admin | `/Admin/Index` | Admin | Dashboard with course/account counts |
| Admin | `/Courses/Index` | Admin | Course CRUD (modal create/edit, search, delete) |
| Admin | `/Accounts/Index` | Admin | Account CRUD (modal, search, role/course, SMTP email) |
| Admin | `/Documents/All` | Admin | All documents across courses (filter by course/status) |
| Teacher | `/Teacher/Index` | Teacher | Lecturer dashboard |
| Teacher | `/Teacher/Courses` | Teacher | **Live** course catalogue (SignalR) |
| Teacher | `/Documents/Upload` | Teacher | Upload + index a course document |
| Teacher | `/Documents/Index` | Teacher | "My Documents" with index status |
| Teacher/Admin | `/Documents/Details` | Teacher, Admin | Chunk preview + re-index + index history |
| Teacher/Student | `/Chat/Index` | Teacher, Student | Pick an indexed document |
| Teacher/Student | `/Chat/Ask` | Teacher, Student | Ask a question; inline grounded answer + sources |
| Teacher/Student | `/Chat/Sessions`, `/Chat/Session` | Teacher, Student | Chat history |
| Student | `/Student/Index` | Student | Student dashboard |
| any | `/AccessDenied`, `/Error` | — | Friendly error pages |

## Feature walkthrough (screenshots)

All images are real captures from the running app (Chrome via the Chrome
DevTools Protocol). Full set + capture method in [`screenshots.md`](screenshots.md).

### Auth
![Login](images/login.png)

### Admin
Dashboard, then course management and the create-course modal:

![Admin dashboard](images/admin-dashboard.png)

### Courses
![Course management](images/admin-courses.png)
![Create course modal](images/create-course-modal.png)

### Accounts
Account list (with role/course/status filters) and the create-lecturer modal:

![Account management](images/account-management.png)
![Create teacher account](images/create-teacher-account.png)

### Teacher
Lecturer dashboard and the live course catalogue:

![Teacher dashboard](images/teacher-dashboard.png)
![Teacher live courses](images/teacher-live-courses.png)

### Documents
Upload form and "My Documents":

![Teacher upload document](images/teacher-upload-document.png)
![Teacher documents](images/teacher-documents.png)

### Chat / RAG
Document picker and the ask-a-question form (answer renders inline):

![Chat RAG picker](images/chat-rag-picker.png)
![Chat RAG ask](images/chat-rag-ask.png)

### SignalR (live courses)
Captured on the lecturer page **after** an admin created course `RTLIVE` in
another browser — the row appeared with no reload (see [`signalr.md`](signalr.md)):

![SignalR live update](images/signalr-live-update.png)

### SMTP email
The rendered lecturer onboarding email template (see [`smtp.md`](smtp.md)):

![Teacher welcome email](images/email-teacher-welcome.png)

## Authentication & role-based access

Session-based auth, identical semantics to the MVC app. After login the same
session keys are written (`Email`, `FullName`, `RoleName`, `AccountId`,
`CourseId`, `CourseCode`).

Role checks are enforced by a Razor Pages **page filter**,
`Infrastructure/SessionAuthorizeAttribute.cs`, applied to PageModels:

```csharp
[SessionAuthorize("Admin")]              // admins only
[SessionAuthorize("Teacher", "Admin")]   // teachers or admins
```

Unauthenticated users are redirected to `/Auth/Login`; authenticated users
without the required role are redirected to `/AccessDenied`.

Roles: `Admin` (configured in `appsettings.json → AdminAccount`, not stored in
the DB), `Teacher` (role `2`), `Student` (role `1`).

## Course create/edit — the dual-model validation note

Both the Create and Edit course/account modals live on the same page, so the
PageModel has **two** bound models (`CreateInput` + `EditInput`). Posting one
form leaves the other empty, whose `[Required]`/non-nullable fields would
otherwise fail validation and silently block the submit. Each handler therefore
clears model state and validates **only its own** model:

```csharp
ModelState.Clear();
if (!TryValidateModel(CreateInput, nameof(CreateInput))) { /* reopen modal */ }
```

This is the fix for the "Create modal reopens and nothing is saved" bug.

## What was added vs. the copied layers

* **Services** (copied, then extended — MVC copy untouched):
  * `Email/` — `IEmailService` + `EmailService` (SMTP), `IEmailTemplateRenderer`
    + `EmailTemplateRenderer`, `Email/Models/TeacherWelcomeEmailModel.cs`,
    `Email/Templates/TeacherWelcome.html` (embedded resource).
  * `AccountService.CreateAsync` now returns `CreateAccountResult` and sends the
    lecturer onboarding email after a successful insert.
  * `CourseService.SearchAsync` / `CourseRepository.SearchAsync` for course search.
* **RazorPages** (new): all pages above, plus `Hubs/CourseHub.cs`,
  `wwwroot/js/course-realtime.js`, `Infrastructure/SessionAuthorizeAttribute.cs`,
  and a vendored SignalR JS client under `wwwroot/lib/signalr/`.

See [`signalr.md`](signalr.md) and [`smtp.md`](smtp.md) for those two features in
depth, and [`setup-guide.md`](setup-guide.md) for how to run everything.

## Known scope notes

* The Python `rag-service/` is unchanged and unmoved; the Razor app calls it
  through the existing `Services` layer (`IRagClient`/`RagApiClient`).
* The MVC app (`HoaiKhoi_SE1950_A01`) remains the canonical Assignment 01 build
  and is fully preserved.

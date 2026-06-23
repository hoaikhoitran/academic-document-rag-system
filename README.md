# Academic Document RAG System
Dự án hỗ trợ quản lý môn học, tài khoản, tài liệu học tập và cho phép sinh viên đặt câu hỏi dựa trên nội dung tài liệu đã được upload. Câu trả lời được tạo dựa trên dữ liệu truy xuất từ tài liệu, giúp hạn chế trả lời sai ngữ cảnh và hỗ trợ truy vết nguồn.

# Diagram
<img width="6520" height="9459" alt="MVC" src="https://github.com/user-attachments/assets/23a77d41-12fd-4c60-8dfc-dc40234a97ee" />

## Features
Đăng nhập và phân quyền người dùng theo vai trò Admin, Teacher, Student.
Admin quản lý tài khoản và môn học.
Teacher upload tài liệu học tập theo môn học.
Hỗ trợ tài liệu PDF, DOCX, PPTX và TXT.
Student đặt câu hỏi dựa trên nội dung tài liệu đã upload.
Trả lời câu hỏi kèm nguồn tham chiếu từ tài liệu.
Lưu lịch sử phiên hỏi đáp.
Quản lý trạng thái upload và index tài liệu.
## Hướng dẫn sử dụng ngắn

### 1. Chạy RAG service

```bash
cd rag-service
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
uvicorn app.main:app --reload --port 8000
```

Kiểm tra RAG service:

```text
http://localhost:8000/health
```

### 2. Tạo database

Mở SQL Server Management Studio và chạy file:

```text
database/AcademicRagManagement.sql
```

### 3. Chạy ứng dụng MVC

Mở solution:

```text
dotnet-mvc/HoaiKhoi_SE1950_A01/HoaiKhoi_SE1950_A01.sln
```

Chọn project `AcademicDocumentRagSystem.MVC` làm startup project và chạy ứng dụng.

### 4. Sử dụng hệ thống

* Đăng nhập bằng tài khoản có sẵn trong database.
* Admin quản lý tài khoản và môn học.
* Teacher upload tài liệu cho môn học.
* Hệ thống tự động gửi tài liệu sang RAG service để index.
* Student chọn tài liệu đã index và đặt câu hỏi.
* Hệ thống trả lời dựa trên nội dung tài liệu và hiển thị nguồn tham chiếu.

## Công nghệ sử dụng

### Backend MVC

* ASP.NET Core MVC
* .NET 8
* SQL Server

### RAG Service

* Python 3.10+
* FastAPI
* ChromaDB
* BAAI/bge-m3 embedding model

### Database

* SQL Server
* Các bảng chính:

  * Accounts
  * Courses
  * Documents
  * ChatSessions
  * ChatMessages
  * DocumentChunks
  * DocumentIndexLogs

---

## Kiến trúc tổng quan

```text
Người dùng
   |
   v
ASP.NET Core MVC
   |
   |-- Quản lý tài khoản
   |-- Quản lý môn học
   |-- Upload tài liệu
   |-- Lưu lịch sử chat
   |
   v
SQL Server
   |
   v
Python FastAPI RAG Service
   |
   |-- Document Loader
   |-- Chunking Service
   |-- Embedding Service
   |-- Vector Store Service
   |-- LLM Service
   |
   v
ChromaDB
```
```text
## Repo structure

academic-document-rag-system/
├── database/
├── dotnet-mvc/
│   └── HoaiKhoi_SE1950_A01/
│       ├── HoaiKhoi_SE1950_A01.sln
│       ├── AcademicDocumentRagSystem.MVC/
│       │   ├── AcademicDocumentRagSystem.MVC.csproj
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   ├── Controllers/
│       │   │   ├── AccountsController.cs
│       │   │   ├── AuthController.cs
│       │   │   ├── ChatController.cs
│       │   │   ├── CoursesController.cs
│       │   │   ├── DocumentsController.cs
│       │   │   └── HomeController.cs
│       │   ├── Filters/
│       │   │   └── SessionAuthorizeAttribute.cs
│       │   ├── Models/
│       │   │   └── ErrorViewModel.cs
│       │   ├── Properties/
│       │   │   └── launchSettings.json
│       │   ├── Views/
│       │   │   ├── _ViewImports.cshtml
│       │   │   ├── _ViewStart.cshtml
│       │   │   ├── Accounts/
│       │   │   │   ├── Create.cshtml
│       │   │   │   ├── Edit.cshtml
│       │   │   │   └── Index.cshtml
│       │   │   ├── Auth/
│       │   │   │   ├── AccessDenied.cshtml
│       │   │   │   └── Login.cshtml
│       │   │   ├── Chat/
│       │   │   │   ├── Answer.cshtml
│       │   │   │   ├── Ask.cshtml
│       │   │   │   ├── Index.cshtml
│       │   │   │   ├── Session.cshtml
│       │   │   │   └── Sessions.cshtml
│       │   │   ├── Courses/
│       │   │   │   ├── Create.cshtml
│       │   │   │   ├── Edit.cshtml
│       │   │   │   └── Index.cshtml
│       │   │   ├── Documents/
│       │   │   │   ├── All.cshtml
│       │   │   │   ├── Details.cshtml
│       │   │   │   ├── Index.cshtml
│       │   │   │   └── Upload.cshtml
│       │   │   ├── Home/
│       │   │   │   ├── Index.cshtml
│       │   │   │   └── Privacy.cshtml
│       │   │   └── Shared/
│       │   │       ├── _Layout.cshtml
│       │   │       ├── _Layout.cshtml.css
│       │   │       ├── _ValidationScriptsPartial.cshtml
│       │   │       └── Error.cshtml
│       │   └── wwwroot/
│       ├── AcademicDocumentRagSystem.Services/
│       │   ├── AcademicDocumentRagSystem.Services.csproj
│       │   ├── DependencyInjection.cs
│       │   ├── Chunking/
│       │   │   ├── ChunkPreviewGenerator.cs
│       │   │   ├── ChunkPreviewItem.cs
│       │   │   ├── ChunkPreviewResult.cs
│       │   │   └── IChunkPreviewGenerator.cs
│       │   ├── DTOs/
│       │   │   ├── Accounts/
│       │   │   │   ├── AccountListItemDto.cs
│       │   │   │   ├── CreateAccountDto.cs
│       │   │   │   └── UpdateAccountDto.cs
│       │   │   ├── Auth/
│       │   │   │   ├── LoginDto.cs
│       │   │   │   └── LoginResultDto.cs
│       │   │   ├── Chat/
│       │   │   │   ├── AskQuestionDto.cs
│       │   │   │   ├── ChatAnswerDto.cs
│       │   │   │   ├── ChatMessageDto.cs
│       │   │   │   ├── ChatSessionDetailsDto.cs
│       │   │   │   ├── ChatSessionDto.cs
│       │   │   │   └── IndexedDocumentDto.cs
│       │   │   ├── Courses/
│       │   │   │   ├── CourseDto.cs
│       │   │   │   ├── CreateCourseDto.cs
│       │   │   │   └── UpdateCourseDto.cs
│       │   │   ├── Documents/
│       │   │   │   ├── DocumentChunkDto.cs
│       │   │   │   ├── DocumentDetailsDto.cs
│       │   │   │   ├── DocumentFilterDto.cs
│       │   │   │   ├── DocumentIndexLogDto.cs
│       │   │   │   ├── DocumentListItemDto.cs
│       │   │   │   ├── DocumentUploadDto.cs
│       │   │   │   └── UploaderDisplay.cs
│       │   │   └── Rag/
│       │   │       ├── RagAskRequest.cs
│       │   │       ├── RagAskResponse.cs
│       │   │       ├── RagConversationTurnDto.cs
│       │   │       ├── RagIndexRequest.cs
│       │   │       ├── RagIndexResponse.cs
│       │   │       └── RagSourceDto.cs
│       │   ├── Implementations/
│       │   │   ├── AccountService.cs
│       │   │   ├── ChatService.cs
│       │   │   ├── CourseService.cs
│       │   │   └── DocumentService.cs
│       │   ├── Interfaces/
│       │   │   ├── IAccountService.cs
│       │   │   ├── IChatService.cs
│       │   │   ├── ICourseService.cs
│       │   │   └── IDocumentService.cs
│       │   ├── Maintenance/
│       │   │   └── DocumentFileHashBackfiller.cs
│       │   └── RagIntegration/
│       │       ├── IRagClient.cs
│       │       └── RagApiClient.cs
│       └── AcademicDocumentRagSystem.DataAccess/
│           ├── AcademicDocumentRagSystem.DataAccess.csproj
│           ├── Models/
│           │   ├── AcademicRagDbContext.cs
│           │   ├── Account.cs
│           │   ├── ChatMessage.cs
│           │   ├── ChatSession.cs
│           │   ├── Course.cs
│           │   ├── Document.cs
│           │   ├── DocumentChunk.cs
│           │   └── DocumentIndexLog.cs
│           └── Repositories/
│               ├── Implementations/
│               │   ├── AccountRepository.cs
│               │   ├── ChatRepository.cs
│               │   ├── CourseRepository.cs
│               │   ├── DocumentChunkRepository.cs
│               │   ├── DocumentIndexLogRepository.cs
│               │   └── DocumentRepository.cs
│               └── Interfaces/
│                   ├── IAccountRepository.cs
│                   ├── IChatRepository.cs
│                   ├── ICourseRepository.cs
│                   ├── IDocumentChunkRepository.cs
│                   ├── IDocumentIndexLogRepository.cs
│                   └── IDocumentRepository.cs

```
---

## Luồng xử lý tài liệu

```text
Teacher upload tài liệu
        |
        v
ASP.NET Core MVC lưu vào SQL Server
        |
        v
Gửi file path sang RAG service
        |
        v
RAG service đọc nội dung tài liệu
        |
        v
Chia nội dung thành chunks
        |
        v
Tạo embedding cho từng chunk
        |
        v
Lưu vector + metadata vào ChromaDB
        |
        v
Cập nhật trạng thái index về hệ thống MVC
```

## Yêu cầu môi trường

Trước khi chạy dự án, cần cài đặt:

* Visual Studio 2022 hoặc Rider
* .NET SDK 8
* SQL Server
* SQL Server Management Studio
* Python 3.10 hoặc 3.11
* Git
## Contributors
* Trần Hoài Khôi
* Chu Vương Mạnh
* Huỳnh Trần Thế Thuật
* Lâm Hoàng Nhân


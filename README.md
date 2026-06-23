# Academic Document RAG System
Dự án hỗ trợ quản lý môn học, tài khoản, tài liệu học tập và cho phép sinh viên đặt câu hỏi dựa trên nội dung tài liệu đã được upload. Câu trả lời được tạo dựa trên dữ liệu truy xuất từ tài liệu, giúp hạn chế trả lời sai ngữ cảnh và hỗ trợ truy vết nguồn.

# Diagram
<img width="6520" height="9459" alt="MVC" src="https://github.com/user-attachments/assets/23a77d41-12fd-4c60-8dfc-dc40234a97ee" />


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

---

## Luồng xử lý tài liệu

```text
Teacher upload tài liệu
        |
        v
ASP.NET Core MVC lưu metadata vào SQL Server
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


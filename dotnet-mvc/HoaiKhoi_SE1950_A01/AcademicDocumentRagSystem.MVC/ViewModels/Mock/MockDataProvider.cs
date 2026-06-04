namespace AcademicDocumentRagSystem.MVC.ViewModels.Mock;

public static class MockDataProvider
{
    public static IReadOnlyList<CourseMock> Courses { get; } = new List<CourseMock>
    {
        new() { Id = "ai101", Code = "AI101", Name = "Trí tuệ nhân tạo", Chapters = 8, Docs = 24, Students = 142 },
        new() { Id = "ml202", Code = "ML202", Name = "Machine Learning cơ bản", Chapters = 10, Docs = 31, Students = 98 },
        new() { Id = "nlp303", Code = "NLP303", Name = "Xử lý ngôn ngữ tự nhiên", Chapters = 6, Docs = 18, Students = 67 },
    };

    public static IReadOnlyList<DocumentMock> Documents { get; } = new List<DocumentMock>
    {
        new() { Id = "d1", Name = "Chương 1 - Giới thiệu AI.pdf", Type = "pdf", Course = "AI101", Chapter = "Chương 1", Size = "2.4 MB", Chunks = 48, Status = "indexed", Uploaded = "2 ngày trước" },
        new() { Id = "d2", Name = "Slide bài giảng tuần 2.pptx", Type = "pptx", Course = "AI101", Chapter = "Chương 2", Size = "5.1 MB", Chunks = 72, Status = "indexed", Uploaded = "5 ngày trước" },
        new() { Id = "d3", Name = "Tài liệu tham khảo Russell.pdf", Type = "pdf", Course = "AI101", Chapter = "Chương 1", Size = "12.7 MB", Chunks = 312, Status = "indexed", Uploaded = "1 tuần trước" },
        new() { Id = "d4", Name = "Bài tập chương 3.docx", Type = "docx", Course = "AI101", Chapter = "Chương 3", Size = "0.8 MB", Chunks = 15, Status = "processing", Uploaded = "vừa xong" },
        new() { Id = "d5", Name = "Đề cương cuối kỳ.pdf", Type = "pdf", Course = "AI101", Chapter = "Tổng hợp", Size = "1.2 MB", Chunks = 22, Status = "indexed", Uploaded = "3 ngày trước" },
        new() { Id = "d6", Name = "Bài giảng search algorithms.pdf", Type = "pdf", Course = "AI101", Chapter = "Chương 4", Size = "3.6 MB", Chunks = 84, Status = "indexed", Uploaded = "1 ngày trước" },
    };

    public static IReadOnlyList<ChatHistoryMock> ChatHistory { get; } = new List<ChatHistoryMock>
    {
        new() { Id = "c1", Title = "Sự khác nhau giữa AI và ML", Time = "Hôm nay", Course = "AI101" },
        new() { Id = "c2", Title = "Thuật toán A* hoạt động ra sao?", Time = "Hôm nay", Course = "AI101" },
        new() { Id = "c3", Title = "Tóm tắt chương 2 giúp em", Time = "Hôm qua", Course = "AI101" },
        new() { Id = "c4", Title = "Ví dụ về propositional logic", Time = "3 ngày trước", Course = "AI101" },
    };

    public static IReadOnlyList<ChatMessageMock> SampleMessages { get; } = new List<ChatMessageMock>
    {
        new() { Role = "user", Content = "Cho em hỏi sự khác nhau cơ bản giữa AI và Machine Learning là gì ạ?" },
        new()
        {
            Role = "assistant",
            Content = "Theo tài liệu môn học, **AI (Trí tuệ nhân tạo)** là một lĩnh vực rộng nghiên cứu cách tạo ra các hệ thống thông minh có thể thực hiện các tác vụ cần đến trí tuệ con người. **Machine Learning** là một nhánh con của AI, tập trung vào việc xây dựng các thuật toán cho phép máy tính học từ dữ liệu mà không cần được lập trình tường minh.\n\nMột số điểm khác biệt chính:\n\n- **Phạm vi**: AI bao gồm cả ML, xử lý ngôn ngữ, robotics, hệ chuyên gia...\n- **Cách tiếp cận**: AI có thể dùng quy tắc (rule-based), còn ML học từ dữ liệu.\n- **Ví dụ**: Một chatbot dùng if-else là AI nhưng không phải ML. Một mô hình dự đoán giá nhà từ dữ liệu lịch sử là ML.",
            Citations = new List<CitationMock>
            {
                new() { Id = "d1", Name = "Chương 1 - Giới thiệu AI.pdf", Page = 12 },
                new() { Id = "d3", Name = "Tài liệu tham khảo Russell.pdf", Page = 4 },
            }
        },
    };

    public static IReadOnlyList<BenchmarkMock> BenchmarkData { get; } = new List<BenchmarkMock>
    {
        new() { Model = "multilingual-e5-base", Faithfulness = 0.82, AnswerRelevancy = 0.79, ContextPrecision = 0.74, Latency = 142 },
        new() { Model = "text-embedding-3-small", Faithfulness = 0.88, AnswerRelevancy = 0.85, ContextPrecision = 0.81, Latency = 198 },
        new() { Model = "PhoBERT-base", Faithfulness = 0.86, AnswerRelevancy = 0.84, ContextPrecision = 0.79, Latency = 121 },
        new() { Model = "bge-m3", Faithfulness = 0.91, AnswerRelevancy = 0.88, ContextPrecision = 0.85, Latency = 167 },
    };

    public static IReadOnlyList<ChunkingMock> ChunkingData { get; } = new List<ChunkingMock>
    {
        new() { Strategy = "Fixed 256", Score = 0.71 },
        new() { Strategy = "Fixed 512", Score = 0.78 },
        new() { Strategy = "Recursive", Score = 0.84 },
        new() { Strategy = "Semantic", Score = 0.89 },
        new() { Strategy = "Sentence-window", Score = 0.82 },
    };

    public static IReadOnlyList<UsageMock> UsageData { get; } = new List<UsageMock>
    {
        new() { Day = "T2", Queries = 120, Users = 42 },
        new() { Day = "T3", Queries = 180, Users = 58 },
        new() { Day = "T4", Queries = 240, Users = 71 },
        new() { Day = "T5", Queries = 210, Users = 65 },
        new() { Day = "T6", Queries = 320, Users = 89 },
        new() { Day = "T7", Queries = 150, Users = 38 },
        new() { Day = "CN", Queries = 90, Users = 22 },
    };

    public static IReadOnlyList<RagVsFinetuneMock> RagVsFinetune { get; } = new List<RagVsFinetuneMock>
    {
        new() { Metric = "Faithfulness", Rag = 0.89, FineTuned = 0.74 },
        new() { Metric = "Relevancy", Rag = 0.86, FineTuned = 0.78 },
        new() { Metric = "Precision", Rag = 0.83, FineTuned = 0.71 },
        new() { Metric = "Recall", Rag = 0.80, FineTuned = 0.76 },
        new() { Metric = "Cost ↓", Rag = 0.90, FineTuned = 0.45 },
    };

    public static IReadOnlyList<UserMock> Users { get; } = new List<UserMock>
    {
        new() { Name = "Nguyễn Văn A", Email = "vana@uni.edu.vn", Role = "student", Course = "AI101", Active = "2 phút trước" },
        new() { Name = "Trần Thị B", Email = "tranb@uni.edu.vn", Role = "lecturer", Course = "AI101, ML202", Active = "5 phút trước" },
        new() { Name = "Lê Văn C", Email = "levanc@uni.edu.vn", Role = "student", Course = "ML202", Active = "1 giờ trước" },
        new() { Name = "Phạm Thị D", Email = "phamd@uni.edu.vn", Role = "lecturer", Course = "NLP303", Active = "3 giờ trước" },
        new() { Name = "Hoàng Quản E", Email = "admin@uni.edu.vn", Role = "admin", Course = "—", Active = "vừa xong" },
        new() { Name = "Đỗ Văn F", Email = "dof@uni.edu.vn", Role = "student", Course = "AI101", Active = "hôm qua" },
    };

    public static StudentChatViewModel GetStudentChatViewModel() => new()
    {
        ChatHistory = ChatHistory.ToList(),
        Messages = SampleMessages.ToList(),
        Documents = Documents.ToList(),
    };

    public static StudentLibraryViewModel GetStudentLibraryViewModel() => new()
    {
        Documents = Documents.ToList(),
        Courses = Courses.ToList(),
    };

    public static TeacherIndexViewModel GetTeacherIndexViewModel()
    {
        var docs = Documents.ToList();
        return new TeacherIndexViewModel
        {
            Documents = docs,
            IndexedCount = docs.Count(d => d.Status == "indexed"),
            ProcessingCount = docs.Count(d => d.Status == "processing"),
            TotalChunks = docs.Sum(d => d.Chunks),
        };
    }

    public static AdminDashboardViewModel GetAdminDashboardViewModel()
    {
        var usage = UsageData.ToList();
        return new AdminDashboardViewModel
        {
            UsageData = usage,
            Courses = Courses.ToList(),
            MaxQueries = usage.Max(u => u.Queries),
            MaxUsers = usage.Max(u => u.Users),
        };
    }

    public static AdminResearchViewModel GetAdminResearchViewModel() => new()
    {
        RagVsFinetune = RagVsFinetune.ToList(),
        ChunkingData = ChunkingData.ToList(),
    };

    public static AdminBenchmarkViewModel GetAdminBenchmarkViewModel()
    {
        var data = BenchmarkData.ToList();
        return new AdminBenchmarkViewModel
        {
            BenchmarkData = data,
            Winner = data.OrderByDescending(b => b.Faithfulness).First(),
        };
    }

    public static string FormatBoldMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var parts = text.Split("**");
        var html = new System.Text.StringBuilder();
        for (var i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 1)
                html.Append("<strong>").Append(System.Net.WebUtility.HtmlEncode(parts[i])).Append("</strong>");
            else
                html.Append(System.Net.WebUtility.HtmlEncode(parts[i]).Replace("\n", "<br/>"));
        }
        return html.ToString();
    }
}

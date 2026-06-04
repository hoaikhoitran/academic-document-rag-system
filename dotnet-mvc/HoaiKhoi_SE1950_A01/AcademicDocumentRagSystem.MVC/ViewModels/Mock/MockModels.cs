namespace AcademicDocumentRagSystem.MVC.ViewModels.Mock;

public class CourseMock
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Chapters { get; set; }
    public int Docs { get; set; }
    public int Students { get; set; }
}

public class DocumentMock
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Chapter { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int Chunks { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Uploaded { get; set; } = string.Empty;
}

public class ChatHistoryMock
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
}

public class CitationMock
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Page { get; set; }
}

public class ChatMessageMock
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<CitationMock>? Citations { get; set; }
}

public class BenchmarkMock
{
    public string Model { get; set; } = string.Empty;
    public double Faithfulness { get; set; }
    public double AnswerRelevancy { get; set; }
    public double ContextPrecision { get; set; }
    public int Latency { get; set; }
}

public class ChunkingMock
{
    public string Strategy { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class UsageMock
{
    public string Day { get; set; } = string.Empty;
    public int Queries { get; set; }
    public int Users { get; set; }
}

public class RagVsFinetuneMock
{
    public string Metric { get; set; } = string.Empty;
    public double Rag { get; set; }
    public double FineTuned { get; set; }
}

public class UserMock
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Active { get; set; } = string.Empty;
}

public class StudentChatViewModel
{
    public List<ChatHistoryMock> ChatHistory { get; set; } = new();
    public List<ChatMessageMock> Messages { get; set; } = new();
    public List<DocumentMock> Documents { get; set; } = new();
}

public class StudentLibraryViewModel
{
    public List<DocumentMock> Documents { get; set; } = new();
    public List<CourseMock> Courses { get; set; } = new();
}

public class TeacherIndexViewModel
{
    public List<DocumentMock> Documents { get; set; } = new();
    public int IndexedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int TotalChunks { get; set; }
}

public class AdminDashboardViewModel
{
    public List<UsageMock> UsageData { get; set; } = new();
    public List<CourseMock> Courses { get; set; } = new();
    public int MaxQueries { get; set; }
    public int MaxUsers { get; set; }
}

public class AdminResearchViewModel
{
    public List<RagVsFinetuneMock> RagVsFinetune { get; set; } = new();
    public List<ChunkingMock> ChunkingData { get; set; } = new();
}

public class AdminBenchmarkViewModel
{
    public List<BenchmarkMock> BenchmarkData { get; set; } = new();
    public BenchmarkMock? Winner { get; set; }
}

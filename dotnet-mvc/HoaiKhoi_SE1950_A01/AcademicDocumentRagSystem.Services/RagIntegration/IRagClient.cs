using AcademicDocumentRagSystem.Services.DTOs.Rag;

namespace AcademicDocumentRagSystem.Services.RagIntegration
{
    public interface IRagClient
    {
        Task<RagIndexResponse> IndexDocumentAsync(RagIndexRequest request);

        Task<RagAskResponse> AskAsync(RagAskRequest request);
    }
}
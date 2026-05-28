// =======================================================================
// IRagClient.cs
// -----------------------------------------------------------------------
// Contract for the PRN222 .NET API to talk to the Python RAG service.
// Inject this in the Application layer; the implementation (RagClient)
// lives in the Infrastructure layer.
// =======================================================================
using System.Threading;
using System.Threading.Tasks;

namespace PRN222.Integration.Rag;

public interface IRagClient
{
    /// <summary>GET /health — returns true when the RAG service is reachable.</summary>
    Task<bool> HealthAsync(CancellationToken ct = default);

    /// <summary>POST /rag/index-document — index a course document.</summary>
    Task<RagIndexResponse> IndexDocumentAsync(RagIndexRequest request, CancellationToken ct = default);

    /// <summary>POST /rag/ask — answer a question grounded in the indexed docs.</summary>
    Task<RagAskResponse> AskAsync(RagAskRequest request, CancellationToken ct = default);

    /// <summary>DELETE /rag/documents/{documentId} — remove a document from the index.</summary>
    Task DeleteDocumentAsync(string documentId, CancellationToken ct = default);
}

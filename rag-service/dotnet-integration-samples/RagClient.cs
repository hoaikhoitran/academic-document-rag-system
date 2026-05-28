// =======================================================================
// RagClient.cs
// -----------------------------------------------------------------------
// HttpClient-based implementation of IRagClient. Drop this into your
// PRN222 Infrastructure project.
//
// Register in Program.cs (Minimal Hosting):
//
//   builder.Services.AddHttpClient<IRagClient, RagClient>(client =>
//   {
//       client.BaseAddress = new Uri(builder.Configuration["Rag:BaseUrl"]!);
//       var apiKey = builder.Configuration["Rag:ApiKey"];
//       if (!string.IsNullOrWhiteSpace(apiKey))
//           client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
//   });
//
// appsettings.json:
//   "Rag": {
//     "BaseUrl": "http://localhost:8000",
//     "ApiKey": ""
//   }
// =======================================================================
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PRN222.Integration.Rag;

public class RagClient : IRagClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RagClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public async Task<bool> HealthAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync("/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<RagIndexResponse> IndexDocumentAsync(
        RagIndexRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/rag/index-document", request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RagIndexResponse>(JsonOptions, ct);
        return result ?? throw new InvalidOperationException("Empty response from RAG service.");
    }

    public async Task<RagAskResponse> AskAsync(
        RagAskRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/rag/ask", request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RagAskResponse>(JsonOptions, ct);
        return result ?? throw new InvalidOperationException("Empty response from RAG service.");
    }

    public async Task DeleteDocumentAsync(string documentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("documentId is required.", nameof(documentId));

        using var response = await _http.DeleteAsync($"/rag/documents/{Uri.EscapeDataString(documentId)}", ct);
        response.EnsureSuccessStatusCode();
    }
}

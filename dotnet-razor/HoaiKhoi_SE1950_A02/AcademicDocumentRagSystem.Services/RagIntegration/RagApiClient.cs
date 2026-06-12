using System.Net.Http.Json;
using AcademicDocumentRagSystem.Services.DTOs.Rag;

namespace AcademicDocumentRagSystem.Services.RagIntegration
{
    public class RagApiClient : IRagClient
    {
        private readonly HttpClient _httpClient;

        public RagApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RagIndexResponse> IndexDocumentAsync(RagIndexRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/rag/index-document", request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"RAG index failed. Status: {(int)response.StatusCode}. Body: {responseBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<RagIndexResponse>();

            if (result == null)
            {
                throw new Exception("RAG service returned empty response.");
            }

            return result;
        }
        public async Task<RagAskResponse> AskAsync(RagAskRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/rag/ask", request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"RAG ask failed. Status: {(int)response.StatusCode}. Body: {responseBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<RagAskResponse>();

            if (result == null)
            {
                throw new Exception("RAG service returned empty response.");
            }

            return result;
        }
    }
}
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using Microsoft.Extensions.Configuration;

namespace FiveamTechCv.Core.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string EmbeddingModel = "text-embedding-004";
    private const string GenerationModel = "gemini-1.5-flash";

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing in configuration");
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{EmbeddingModel}:embedContent?key={_apiKey}";
        
        var payload = new
        {
            model = $"models/{EmbeddingModel}",
            content = new { parts = new[] { new { text = text } } }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Embedding?.Values ?? new List<float>();
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GenerationModel}:generateContent?key={_apiKey}";
        
        var payload = new
        {
            contents = new[] 
            { 
                new 
                { 
                    role = "user",
                    parts = new[] { new { text = prompt } } 
                } 
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GenerationResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "I'm sorry, I couldn't generate a response.";
    }

    // DTOs
    private class EmbeddingResponse
    {
        public EmbeddingData? Embedding { get; set; }
    }

    private class EmbeddingData
    {
        public List<float>? Values { get; set; }
    }

    private class GenerationResponse
    {
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        public Content? Content { get; set; }
    }

    private class Content
    {
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }
}

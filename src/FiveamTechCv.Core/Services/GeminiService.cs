using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using Microsoft.Extensions.Configuration;

namespace FiveamTechCv.Core.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    // Current Stable Models
    private const string EmbeddingModel = "text-embedding-004"; 
    private const string GenerationModel = "gemini-2.5-flash";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing");
        
        // Updated to use the v1beta base address for newer models
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        }
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        var requestUrl = $"v1beta/models/{EmbeddingModel}:embedContent?key={_apiKey}";
        
        var payload = new
        {
            model = $"models/{EmbeddingModel}",
            content = new { parts = new[] { new { text } } }
        };

        using var response = await _httpClient.PostAsJsonAsync(requestUrl, payload, JsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Gemini API Error (Embedding): {response.StatusCode} - {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(JsonOptions);
        return result?.Embedding?.Values ?? [];
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        var requestUrl = $"v1/models/{GenerationModel}:generateContent?key={_apiKey}";
        
        var payload = new
        {
            contents = new[] 
            { 
                new { parts = new[] { new { text = prompt } } } 
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(requestUrl, payload, JsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Try to parse the error to see if it's RPM or RPD
                // The error format from Gemini usually contains details in JSON
                // Example: { "error": { "code": 429, "message": "Resource has been exhausted (e.g. check quota).", "status": "RESOURCE_EXHAUSTED" } }
                // Sometimes the message contains "RPM" or "RPD" or "TPM"
                
                string userMessage = "I'm currently receiving too many requests. Please try again tomorrow.";
                
                if (errorContent.Contains("RPM", StringComparison.OrdinalIgnoreCase) || 
                    errorContent.Contains("requests per minute", StringComparison.OrdinalIgnoreCase))
                {
                    userMessage = "I'm a bit overwhelmed right now (RPM limit). Please give me a minute to catch my breath.";
                }
                else if (errorContent.Contains("RPD", StringComparison.OrdinalIgnoreCase) || 
                         errorContent.Contains("requests per day", StringComparison.OrdinalIgnoreCase))
                {
                    userMessage = "I've reached my daily limit of thoughts (RPD limit). Please come back tomorrow!";
                }
                else if (errorContent.Contains("TPM", StringComparison.OrdinalIgnoreCase) || 
                         errorContent.Contains("tokens per minute", StringComparison.OrdinalIgnoreCase))
                {
                     userMessage = "That was a lot to process at once (TPM limit). Please try a shorter question or wait a moment.";
                }

                // Return the friendly message instead of throwing, so the UI can display it
                return userMessage;
            }

            throw new HttpRequestException($"Gemini API Error (Generation): {response.StatusCode} - {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<GenerationResponse>(JsonOptions);
        
        return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text 
               ?? throw new InvalidOperationException("API returned an empty response.");
    }

    #region DTOs
    private record EmbeddingResponse(EmbeddingData? Embedding);
    private record EmbeddingData(List<float>? Values);
    private record GenerationResponse(List<Candidate>? Candidates);
    private record Candidate(Content? Content);
    private record Content(List<Part>? Parts);
    private record Part(string? Text);
    #endregion
}

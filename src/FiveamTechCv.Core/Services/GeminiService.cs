using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using Microsoft.Extensions.Configuration;
using Google.GenAI;
using Google.GenAI.Types;

namespace FiveamTechCv.Core.Services;

public class GeminiService : IGeminiService
{
    private readonly Client _geminiClient;
    private readonly string _apiKey;
    
    // Current Stable Models
    private const string EmbeddingModel = "text-embedding-004"; 
    private const string GenerationModel = "gemini-2.5-flash";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiService(IConfiguration configuration)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing");
        
        _geminiClient = new Client(apiKey:_apiKey);
    }

    public async Task<List<double>> GenerateEmbeddingAsync(string text)
    {
        var response = await _geminiClient.Models.EmbedContentAsync(
            EmbeddingModel,
            text
        );
        
        return (
            response.Embeddings == null
                ? throw new InvalidOperationException("API returned an empty response.") 
                : response.Embeddings[0].Values) ?? throw new InvalidOperationException("API returned an empty response."
        );
    }

    public async Task<string> GenerateResponseAsync(string systemPrompt, List<ChatMessage> history, string prompt)
    {
        // Add history
        // var historyContent = new List<Content>();
        //  var userHistory = history.Where(x => x.Role == "user").Select(x => new Part { Text = x.Text}).ToList();
        //  if (userHistory.Count > 0)
        //  {
        //      historyContent.Add(new Content
        //      {
        //          Role = "user",
        //          Parts = userHistory
        //      });
        //  }
        //
        //  var assistantHistory = history.Where(x => x.Role == "system").Select(x => new Part { Text = x.Text}).ToList();
        //  if (assistantHistory.Count > 0)
        //  {
        //      historyContent.Add(new Content
        //      {
        //          Role = "assistant",
        //          Parts = assistantHistory
        //      });
        //  }
        var historyContent = history.Select(x => new Content
        {
            Role = x.Role == "system" ? "model" : "user",
            Parts = [new Part { Text = x.Text }]
        }).ToList();

        // Add current prompt
        historyContent.Add(new Content
        { 
            Role = "user",
            Parts = new List<Part> { new() { Text = prompt } } 
        });

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts = new List<Part> { new() { Text = systemPrompt } }
            }
        };
        
        
        var response = await _geminiClient.Models.GenerateContentAsync(GenerationModel, historyContent, config);
        return response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text 
               ?? throw new InvalidOperationException("API returned an empty response.");
    }
}

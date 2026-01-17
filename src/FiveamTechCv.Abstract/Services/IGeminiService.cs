using FiveamTechCv.Entities;

namespace FiveamTechCv.Abstract.Services;

public interface IGeminiService
{
    Task<List<double>> GenerateEmbeddingAsync(string text);
    Task<string> GenerateResponseAsync(string systemPrompt, List<ChatMessage> history, string prompt);
}

public class ChatMessage
{
    public string Role { get; set; }
    public string Text { get; set; }
}

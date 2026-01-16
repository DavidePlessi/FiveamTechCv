using FiveamTechCv.Entities;

namespace FiveamTechCv.Abstract.Services;

public interface IGeminiService
{
    Task<List<float>> GenerateEmbeddingAsync(string text);
    Task<string> GenerateResponseAsync(string prompt);
}

using System.Text;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Core.Services;

public class AiCVService : IAiCVService
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IGeminiService _geminiService;

    public AiCVService(IVectorSearchService vectorSearchService, IGeminiService geminiService)
    {
        _vectorSearchService = vectorSearchService;
        _geminiService = geminiService;
    }

    public async Task<string> AskAsync(string question)
    {
        // 1. Retrieve relevant nodes
        var nodes = await _vectorSearchService.SearchAsync(question);

        if (!nodes.Any())
        {
            return "I couldn't find any relevant information to answer your question.";
        }

        // 2. Build Context
        var contextBuilder = new StringBuilder();
        foreach (var node in nodes)
        {
            if (node is IVectorizable vectorizable)
            {
                contextBuilder.AppendLine(vectorizable.GetContentToEmbed());
            }
            else
            {
                 // Fallback if not IVectorizable, though VectorSearchService filters for specific types currently
                 contextBuilder.AppendLine($"Node Id: {node.Id}");
            }
        }

        var context = contextBuilder.ToString();

        // 3. Construct Prompt
        var systemPrompt = $@"You are an AI assistant for a professional CV. 
Your goal is to answer questions about the candidate's experience, projects, and skills based STRICTLY on the provided context.
Do not invent information. If the answer is not in the context, state clearly that you do not have that information.

Context:
{context}

Question: {question}

Answer:";

        // 4. Generate Response
        return await _geminiService.GenerateResponseAsync(systemPrompt);
    }
}

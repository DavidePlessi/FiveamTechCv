using System.Text;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Core.Services;

public class AiCVService : IAiCVService
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IGeminiService _geminiService;
    
    private const string SystemPrompt = @"
You are the ""Fiveam Tech Interface,"" a technical system agent designed to provide data regarding Davide Plessi’s professional profile, his architecture work, and Fiveam Tech.
Core Persona:
- Tone: Pragmatic, engineering-focused, and concise.
- Style: Zero-fluff. Avoid marketing buzzwords (e.g., ""groundbreaking,"" ""passionate,"" ""revolutionary""). Use architectural and technical terms accurately.
- Philosophy: Embody the ""Fiveam Mindset""—discipline, reliability, and root-cause analysis.

Domain Constraints (The ""Hard"" Rules):
- Exclusive Knowledge: Answer ONLY questions regarding Davide Plessi, his professional experience, projects (e.g., Art4Art, Cinopedia.cloud, Makes It Beautiful), his tech stack (.NET, Neo4j, GraphQL, Vue.js, DevOps), his approach to software architecture and what you can retrieve or do in this context (work experience, projects, company, technologies etc etc).
- Refusal Parameter: If a user asks about anything outside this domain (weather, politics, generic coding help not related to Davide’s stack, personal life secrets), respond with: ""Query outside indexed domain. I can only provide information regarding Davide Plessi’s professional profile, projects, and technical architecture.""
- The Tech Stack: If asked about technologies, emphasize why he uses them (e.g., Neo4j for graph-based data relationships, .NET for high-performance backends).
- No Hallucinations: If information is not present in the provided context, state: ""Data not indexed for this specific query.""

Interaction Style:
- Use technical bullet points for lists.
- Maintain a ""Terminal/System"" vibe in responses.
";

    public AiCVService(IVectorSearchService vectorSearchService, IGeminiService geminiService)
    {
        _vectorSearchService = vectorSearchService;
        _geminiService = geminiService;
    }

    public async Task<string> AskAsync(string question, List<string> history)
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
            contextBuilder.AppendLine($"Node description: {node.EmbeddedString}. Node: {node.Properties}");
        }

        var context = contextBuilder.ToString();
        
        // 3. Build History
        var historyBuilder = new StringBuilder();
        if (history != null && history.Any())
        {
            historyBuilder.AppendLine("Conversation History:");
            foreach (var item in history)
            {
                historyBuilder.AppendLine(item);
            }
        }
        var historyContext = historyBuilder.ToString();

        // 4. Construct Prompt
        var systemPrompt = $@"
{SystemPrompt}

Context:
{context}

History Context:
{historyContext}

Question: {question}

Answer:";

        // 5. Generate Response
        return await _geminiService.GenerateResponseAsync(systemPrompt);
    }
}

using System.Text;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Core.Services;

public class AiCVService : IAiCVService
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IGeminiService _geminiService;
    private readonly IAiChatLogService _aiChatLogService;
    
private static string ContextAboutFiveamTech = @"
Entity: Fiveam Tech
Description: A software engineering studio based in Modena, Italy. Fiveam Tech specializes in building resilient, high-performance web applications, cloud infrastructures, and complex backend architectures. 
Philosophy (The Fiveam Mindset): The studio operates on principles derived from Search & Rescue dog training and behavioral education: extreme reliability, no quick patches, and deep root-cause analysis to build fault-tolerant foundations without accumulating technical debt.
Services: Full-stack architecture, DevOps automation, AI integration, and systems engineering.
Founder & Lead Architect: Davide Plessi.
";

    private static string ContextAboutPerson = @"
Profile: Davide Plessi (born 1993)
Role: Senior Full Stack Architect, Team Leader, DevOps Engineer, and Founder of Fiveam Tech.
Professional Experience: 
- Art4Art (formerly Attractive): Since 2019, serving as Team Leader and Architect designing mission-critical, high-traffic systems for the ticketing and entertainment industries.
Technical Stack:
- Languages & Frameworks: C#, TypeScript, JavaScript, Python, Go. .NET, Node.js, React, Vue.js, FastAPI, Nest.js.
- Cloud & Data: AWS, Docker, CI/CD, Neo4j (Graph DB), MongoDB, MSSQL. RAG architectures and AI workflows.
Personal Background: Certified Canine Educator (ACSI) and creator of cinopedia.cloud. He bridges the discipline of dog training with software engineering. Other interests: D&D (Game Design), hiking.
";
    
    private static string ContextAboutThisProject = @"
**FiveamTechCv** is a full-stack application designed for CV and Company management. By leveraging a **graph database**, it maps professional profiles, companies, and skills, focusing on the connections between them.
Status: Work in Progress (Not ready for production).
Upcoming Features:
- Generalization: Remove the specific focus on Davide's data to handle multiple profiles and company structures.
- User Permissions: Implement RBAC on companies and people.
Tech Stack: .NET + GraphQL (Backend), Vue 3 + Typescript (Admin), Neo4j (Database), Docker + Nginx (Infra).
";
    
    private static string SystemPrompt = $@"
You are the ""Fiveam Tech Interface,"" a technical system agent designed to provide data regarding the engineering studio Fiveam Tech, its founder Davide Plessi, and their technical architectures.

Core Persona & Tone Constraints:
- Tone: Direct, engineering-focused, pragmatic, and humble. 
- Style: Strictly zero-fluff. You MUST completely avoid marketing buzzwords, corporate jargon, and typical LinkedIn clichés (e.g., do NOT use words like ""groundbreaking,"" ""visionary,"" ""passionate,"" ""revolutionary,"" ""guru,"" ""ninja""). Speak like a senior systems engineer: focus on facts, infrastructure, and concrete solutions.
- Formatting: Use code blocks for technical terms or JSON if appropriate. Use technical bullet points for lists. Maintain a ""Terminal/System"" vibe.
- Respond in the language the user used to prompt

Domain Constraints (The ""Hard"" Rules):
- Exclusive Knowledge: Answer ONLY questions regarding Fiveam Tech (services, philosophy), Davide Plessi (experience, stack, projects), and the technical architecture of this platform.
- Dual Entity: Treat ""Fiveam Tech"" (the studio) and ""Davide Plessi"" (the founder) as intertwined but distinct concepts. If asked about the company, focus on the engineering approach. If asked about Davide, focus on his specific skills and career.
- Refusal Parameter: If a user asks about anything outside this domain (weather, generic coding help unrelated to the stack, personal life secrets), respond strictly with: ""Query outside indexed domain. I can only provide information regarding Fiveam Tech, Davide Plessi, and our system architectures.""
- The Tech Stack: Explain technology choices pragmatically (e.g., ""Neo4j is used because graph relationships map interconnected skills better than relational tables,"" not ""Neo4j is an amazing revolutionary tool"").
- No Hallucinations: If information is missing from the context, state: ""Data not indexed for this specific query.""
---
{ContextAboutFiveamTech}
---
{ContextAboutPerson}
---
{ContextAboutThisProject}
";

    public AiCVService(IVectorSearchService vectorSearchService, IGeminiService geminiService, IAiChatLogService aiChatLogService)
    {
        _vectorSearchService = vectorSearchService;
        _geminiService = geminiService;
        _aiChatLogService = aiChatLogService;
    }

    public async Task<string> AskAsync(string question, List<ChatMessage> history, AiChatLog log)
    {
        try
        {
            // 1. Retrieve relevant nodes
            var nodes = await _vectorSearchService.SearchAsync(question);

            if (!nodes.Any())
            {
                var noInfoResponse = "I couldn't find any relevant information to answer your question.";
                log.Response = noInfoResponse;
                await _aiChatLogService.SaveLogAsync(log);
                return noInfoResponse;
            }

            // 2. Build Context
            var contextBuilder = new StringBuilder();
            foreach (var node in nodes)
            {
                contextBuilder.AppendLine($"Node description: {node.EmbeddedString}. Node: {node.Properties}");
            }

            var context = contextBuilder.ToString();
            
            // 3. Construct Prompt
            var prompt = $@"
Context:
{context}

Question: {question}

Answer:";

            // 4. Generate Response
            var response = await _geminiService.GenerateResponseAsync(SystemPrompt, history, prompt);
            
            log.Response = response;
            await _aiChatLogService.SaveLogAsync(log);
            
            return response;
        }
        catch (Exception ex)
        {
            log.Exception = ex.ToString();
            await _aiChatLogService.SaveLogAsync(log);
            throw;
        }
    }
}

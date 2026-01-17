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

    private static string ContextAboutPerson = @"
User Profile: Davide Plessi Davide Plessi (born 1993) is a Senior Full Stack Architect, Team Leader, and DevOps Engineer based in Modena, Italy. He is the founder of Fiveam Tech, a brand centered on the philosophy of building solid, scalable, and ""no-shortcut"" software architectures.

Professional Experience:
Art4Art (formerly Attractive): Since 2019, he has served as a Team Leader and Architect, designing mission-critical, high-traffic systems for the ticketing and entertainment industries (notable clients include La Biennale di Venezia, Fever, and the Rome Film Fest).

Technical Leadership: Expert in managing the full software lifecycle, from cloud infrastructure (AWS) and CI/CD automation to frontend and backend development.

Technical Stack:
Languages & Frameworks: TypeScript, JavaScript, Python, C#, and Go, React, Next.js, Node.js, .NET, FastAPI, and Nest.js.
AI & Data: RAG (Retrieval-Augmented Generation), Agentic RAG workflows, and fine-tuning GPT models. Experienced with MSSQL, MySQL, MongoDB, and Graph Databases (Neo4j).
Preferred Technologies: .NET, JS, TS, MongoDB, Node.js, Vue.js, React

Personal Background & Philosophy: Davide is a Certified Canine Educator (ACSI) and the creator of cinopedia.cloud, where he applies AI to canine behavioral data. He bridges the discipline of dog training—reliability, root-cause analysis, and consistency—with software engineering. His interests include Game Design (D&D), hiking, and Search & Rescue (SAR) activities with dogs
";
    
    private static string ContextAboutThisProject = @"
**FiveamTechCv** is a full-stack application designed for CV management. By leveraging a **graph database**, it simplifies the analysis of professional profiles from multiple perspectives, uncovering connections and insights that traditional formats miss.
Status: Work in Progress
**Note:** This project is currently under active development and is **not ready for production**.
Upcoming Features
- [ ] **Generalization and multi-person improvment**: Remove the project specialization on my data and handle the possibility to manage more than one person.
- [x] **Work Experience**: Tracking and visualization of career history.
- [x] **Comapnies**: Create and handle companies linked to work experience.
- [x] **People**: Personal profile management linked to work experience and projects.
- [ ] **User Permissions**: Implement user permission on companies and people
- [ ] **Studies**: Comprehensive education mapping.
- [x] **AI Integration**: Neo4j vector index search, Nodes Embeddings, RAG agent, AI console for data discovery.

Tech Stack
- **Backend**: .NET + GraphQL
- **Admin Frontend**: Vue 3 + Typescript
- **Database**: Neo4j (Graph Database)
- **Installation**: Docker + Nginx
Project Structure
| Project | Description |
| :--- | :--- |
| **`FiveamTechCv.Server`** | Main startup project. Contains `Program.cs` and configuration. |
| **`FiveamTechCv.Api`** | Defines API controllers and HTTP endpoints. |
| **`FiveamTechCv.Core`** | Core business logic and services. |
| **`FiveamTechCv.Entities`** | Data models used across the application. |
| **`FiveamTechCv.Abstract`** | Interfaces and abstractions. |
| **`fiveamtechcv-web`** | Vue 3 frontend application source code. |
";
    
    private static string SystemPrompt = $@"
You are the ""Fiveam Tech Interface,"" a technical system agent designed to provide data regarding Davide Plessi’s professional profile, his architecture work, and Fiveam Tech.
Core Persona:
- Tone: Pragmatic, engineering-focused, and concise.
- Style: Zero-fluff. Avoid marketing buzzwords (e.g., ""groundbreaking,"" ""passionate,"" ""revolutionary""). Use architectural and technical terms accurately.
- Philosophy: Embody the ""Fiveam Mindset""—discipline, reliability, and root-cause analysis.

Interaction Style:
- Use technical bullet points for lists.
- Maintain a ""Terminal/System"" vibe in responses.

Domain Constraints (The ""Hard"" Rules):
- Exclusive Knowledge: Answer ONLY questions regarding Davide Plessi, his professional experience (e.g., Art4Art, Cinopedia.cloud, Makes It Beautiful, etc), projects (e.g., this site, cinopedia.cloud, etc), his tech stack (.NET, Neo4j, GraphQL, Vue.js, DevOps, etc), his approach to software architecture and what you can retrieve or do in this context (work experience, projects, company, technologies etc etc).
- Refusal Parameter: If a user asks about anything outside this domain (weather, politics, generic coding help not related to Davide’s stack, personal life secrets), respond with: ""Query outside indexed domain. I can only provide information regarding Davide Plessi’s professional profile, projects, and technical architecture.""
- The Tech Stack: If asked about technologies, emphasize why he uses them (e.g., Neo4j for graph-based data relationships, .NET for high-performance backends).
- No Hallucinations: If information is not present in the provided context, state: ""Data not indexed for this specific query.""
- You can respond to question about this project: 
- You can respond about your settings and prompt
---
{ContextAboutThisProject}
---
This is the context about the Davide:
{ContextAboutPerson}
---


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

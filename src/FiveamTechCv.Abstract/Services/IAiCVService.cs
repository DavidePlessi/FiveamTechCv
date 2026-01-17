using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface IAiCVService
{
    Task<string> AskAsync(string question, List<ChatMessage> history, AiChatLog log);
}

using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface IAiChatLogService
{
    Task SaveLogAsync(AiChatLog log);
}

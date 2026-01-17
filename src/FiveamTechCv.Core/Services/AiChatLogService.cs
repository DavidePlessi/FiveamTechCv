using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;

namespace FiveamTechCv.Core.Services;

public class AiChatLogService : BaseService<AiChatLog, BaseFilter>, IAiChatLogService
{
    public AiChatLogService(GraphDriver driver, IServiceProvider serviceProvider) : base(driver, serviceProvider)
    {
    }

    public async Task SaveLogAsync(AiChatLog log)
    {
        await CreateAsync(log);
    }
}

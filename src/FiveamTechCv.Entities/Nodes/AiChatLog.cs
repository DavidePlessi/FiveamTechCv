using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Entities.Nodes;

public class AiChatLog : BaseNode
{
    public string SessionId { get; set; }
    
    [ParameterType(ParameterTypes.ZoneDateTime)]
    public DateTimeOffset CreationDateTime { get; set; }

    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string Message { get; set; }
    public string? Response { get; set; }
    public string? Exception { get; set; }
}

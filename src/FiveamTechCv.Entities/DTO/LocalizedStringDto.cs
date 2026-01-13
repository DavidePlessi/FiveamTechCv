using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class LocalizedStringDto : BaseDto<LocalizedString>
{
    public string? Language { get; set; }
    public string? Value { get; set; }
}
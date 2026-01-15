using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class CompanyDto : BaseDto<Company>
{
    public string? Name { get; set; }
    public string? Website { get; set; }
    
    public List<LocalizedStringDto>? Description { get; set; }
}

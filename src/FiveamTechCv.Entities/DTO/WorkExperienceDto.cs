using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class WorkExperienceDto : BaseDto<WorkExperience>
{
    public string? Company { get; set; }
    public string? CompanyUrl { get; set; }
    public string? Position { get; set; }
    
    public List<LocalizedStringDto>? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int? Order { get; set; }
    
    [EntityConversionInfo(false, "Projects")]
    public List<string>? ProjectIdsToLink { get; set; }
    
    [EntityConversionInfo(false, "Tags")]
    public List<string>? TagIdsToLink { get; set; }
}
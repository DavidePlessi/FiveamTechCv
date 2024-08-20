using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class WorkExperienceDto : BaseDto<WorkExperience>
{
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? Description { get; set; }
    public long? StartDate { get; set; }
    public long? EndDate { get; set; }
    
    [EntityConversionInfo(true)]
    public List<string>? ProjectIdsToLink { get; set; }
}
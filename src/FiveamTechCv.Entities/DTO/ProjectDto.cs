using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class ProjectDto : BaseDto<Project>
{
    public string Name { get; set; }
    public List<LocalizedStringDto>? Description { get; set; }
    
    [EntityConversionInfo(true)]
    public List<string>? TagIdsToLink { get; set; }
}
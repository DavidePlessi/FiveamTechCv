using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class ProjectDto : BaseDto<Project>
{
    public string Name { get; set; }
    public int? Order { get; set; }
    
    public List<LocalizedStringDto>? Description { get; set; }
    
    [EntityConversionInfo(false, "Tags")]
    public List<string>? TagIdsToLink { get; set; }
    
    [EntityConversionInfo(false, "People")]
    public List<string>? PersonIdsToLink { get; set; }
}
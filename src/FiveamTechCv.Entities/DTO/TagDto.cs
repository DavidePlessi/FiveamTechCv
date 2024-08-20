using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class TagDto : BaseDto<Tag>
{
    public string? Name { get; set; }
    public TagType? Type { get; set; }
    public string? DocumentationLink { get; set; }
    
    [EntityConversionInfo(true)]
    public List<string>? ProjectIdsToLink { get; set; }
}
using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.DTO;

public class PersonDto : BaseDto<Person>
{
    public string Name { get; set; }
    public string LastName { get; set; }
    
    [EntityConversionInfo(false, "BornDate")]
    public DateTimeOffset? BornDate { get; set; }

    public List<LocalizedStringDto>? Info { get; set; }
    public List<LocalizedStringDto>? Summary { get; set; }
    public List<LocalizedStringDto>? Mindset { get; set; }
    public List<LocalizedStringDto>? Slogan { get; set; }
    
    [EntityConversionInfo(false, "Projects")]
    public List<string>? ProjectIdsToLink { get; set; }
    
    [EntityConversionInfo(false, "WorkExperiences")]
    public List<string>? WorkExperienceIdsToLink { get; set; }
    
    [EntityConversionInfo(false, "Tags")]
    public List<string>? TagIdsToLink { get; set; }
}

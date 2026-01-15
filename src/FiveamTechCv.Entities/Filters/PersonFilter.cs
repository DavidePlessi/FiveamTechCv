using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Entities.Filters;

public class PersonFilter : BaseNodeFilter
{
    [FilterType(FilterType.Contains)]
    public string? Name { get; set; }
    
    [FilterType(FilterType.Contains)]
    public string? LastName { get; set; }
}

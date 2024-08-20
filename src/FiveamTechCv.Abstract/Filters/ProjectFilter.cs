using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Abstract.Filters;

public class ProjectFilter : BaseNodeFilter
{
    [FilterType(FilterType.Contains)]
    public string? Name { get; set; }
    
    [FilterType(FilterType.Contains)]
    public string? Description { get; set; }
}
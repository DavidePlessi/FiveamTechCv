using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Entities.Filters;

public class CompanyFilter : BaseNodeFilter
{
    [FilterType(FilterType.Contains)]
    public string? Name { get; set; }
}

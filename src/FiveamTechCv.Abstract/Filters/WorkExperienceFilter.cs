using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Abstract.Filters;

public class WorkExperienceFilter : BaseNodeFilter
{
    [FilterType(FilterType.Contains)]
    public string? Company { get; set; }
    
    [FilterType(FilterType.Contains)]
    public string? Position { get; set; }
    
    [FilterType(FilterType.Contains)]
    public string? Description { get; set; }
    
    [FilterType(FilterType.GreaterThanOrEqual)]
    [ParameterType(ParameterTypes.Long, "StartDate")]
    public long? FromStartDate { get; set; }
    
    [FilterType(FilterType.LessThanOrEqual)]
    [ParameterType(ParameterTypes.Long, "StartDate")]
    public long? ToStartDate { get; set; }
    
    [FilterType(FilterType.GreaterThanOrEqual)]
    [ParameterType(ParameterTypes.Long, "EndDate")]
    public long? FromEndDate { get; set; }
    
    [FilterType(FilterType.LessThanOrEqual)]
    [ParameterType(ParameterTypes.Long, "EndDate")]
    public long? ToEndDate { get; set; }
}
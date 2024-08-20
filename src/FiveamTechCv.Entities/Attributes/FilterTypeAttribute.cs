namespace FiveamTechCv.Entities.Attributes;

public enum FilterType
{
    Equal,
    Different,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith
}

public class FilterTypeAttribute: Attribute
{
    public FilterType FilterType { get; set; }
    
    
    public FilterTypeAttribute(
        FilterType filterType
    )
    {
        FilterType = filterType;
    }
}
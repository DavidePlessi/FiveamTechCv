using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.Filters;

public class TagFilter : BaseNodeFilter
{
    [FilterType(FilterType.Contains)]
    public string? Name { get; set; }
    
    public TagType? Type { get; set; }
    
    // public string ToCypherCondition()
    // {
    //     var conditions = new List<string>();
    //     
    //     if (Name != null)
    //     {
    //         conditions.Add("Username: $username");
    //     }
    //     
    //     if (Type != null)
    //     {
    //         conditions.Add("Type: $type");
    //     }
    //     
    //     if (ProjectId != null)
    //     {
    //         conditions.Add("ProjectId: $projectId");
    //     }
    //     
    //     return conditions.Count > 0 ? "{" + string.Join(", ", conditions) + "}" : "";
    // }
    //
    // public object ToCypherParams()
    // {
    //     var result = new ExpandoObject();
    //     var dictionary = result as IDictionary<string, object>;
    //     
    //     if (Name != null)
    //     {
    //         dictionary.Add("name", Name);
    //     }
    //
    //     if (Type != null)
    //     {
    //         dictionary.Add("type", Type);
    //     }
    //
    //     if (ProjectId != null)
    //     {
    //         dictionary.Add("projectId", ProjectId);
    //     }
    //     
    //     return result;
    // }
}
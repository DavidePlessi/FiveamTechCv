using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class Company : BaseNode
{
    public string? Name { get; set; }
    public string? Website { get; set; }
    
    [Neo4JRelationship("HAS_DESCRIPTION")]
    [NodeRelationship("HAS_DESCRIPTION", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Description { get; set; }
    
     // To Description
    public const string HAS_DESCRIPTION = "HAS_DESCRIPTION";
}

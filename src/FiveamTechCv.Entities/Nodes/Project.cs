using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class Project : BaseNode
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    [Neo4JRelationship("HAS_TAG")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Tag>? Tags { get; set; }
    
}
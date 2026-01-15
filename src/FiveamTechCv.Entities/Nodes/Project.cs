using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class Project : BaseNode
{
    public string Name { get; set; }
    public int? Order { get; set; }
    
    [Neo4JRelationship("HAS_DESCRIPTION")]
    [NodeRelationship("HAS_DESCRIPTION", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Description { get; set; }
    
    [Neo4JRelationship("HAS_TAG")]
    [NodeRelationship("HAS_TAG", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Tag>? Tags { get; set; }
    
    // To Tag
    public const string HAS_TAG = "HAS_TAG";
    
    //To Description
    public const string HAS_DESCRIPTION = "HAS_DESCRIPTION";
    
    [Neo4JRelationship(Person.HAS_PROJECT, RelationshipDirection.Incoming)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Person>? People { get; set; }
}
using System.Text.Json.Serialization;
using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public enum TagType
{
    Framework,
    Language,
    Technology,
    Library,
    Database,
    Platform,
    Area,
    Category,
    Role
}

public class Tag : BaseVectorizableNode
{
    public string? Name { get; set; }
    public TagType? Type { get; set; }
    public string? DocumentationLink { get; set; }
    public int? Order { get; set; }
    
    [Neo4JRelationship("HAS_TAG", RelationshipDirection.Incoming)]
    [NodeRelationship("HAS_TAG", NodeRelationType.Link, true)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Project>? Projects { get; set; }

    public override string? GetContentToEmbed()
    {
        return $"Tag: {Name} (Type: {Type})";
    }
}
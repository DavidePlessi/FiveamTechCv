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
    Area
}

public class Tag : BaseNode
{
    public string? Name { get; set; }
    public TagType? Type { get; set; }
    public string? DocumentationLink { get; set; }
    
    [Neo4JRelationship("HAS_TAG", RelationshipDirection.Incoming)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Project>? Projects { get; set; }
}
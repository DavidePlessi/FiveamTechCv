using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class WorkExperience : BaseNode
{
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? Description { get; set; }
    public long StartDate { get; set; }
    public long EndDate { get; set; }
    
    [Neo4JRelationship("HAS_PROJECT")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Project>? Projects { get; set; }
    
    // To Project
    public const string HAS_PROJECT = "HAS_PROJECT";
}
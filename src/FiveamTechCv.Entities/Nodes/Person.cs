using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;
using HotChocolate;

namespace FiveamTechCv.Entities.Nodes;

public class Person : BaseNode
{
    public string? Name { get; set; }
    public string? LastName { get; set; }
    
    [ParameterType(ParameterTypes.ZoneDateTime)]
    [GraphQLType("DateTime")]
    public DateTimeOffset? BornDate { get; set; }
    
    [Neo4JRelationship("HAS_INFO")]
    [NodeRelationship("HAS_INFO", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Info { get; set; }
    
    [Neo4JRelationship("HAS_SUMMARY")]
    [NodeRelationship("HAS_SUMMARY", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Summary { get; set; }
    
    [Neo4JRelationship("HAS_MINDSET")]
    [NodeRelationship("HAS_MINDSET", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Mindset { get; set; }
    
    [Neo4JRelationship("HAS_SLOGAN")]
    [NodeRelationship("HAS_SLOGAN", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Slogan { get; set; }
    
    [Neo4JRelationship("HAS_PROJECT")]
    [NodeRelationship("HAS_PROJECT", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Project>? Projects { get; set; }
    
    [Neo4JRelationship("HAS_WORK_EXPERIENCE")]
    [NodeRelationship("HAS_WORK_EXPERIENCE", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<WorkExperience>? WorkExperiences { get; set; }
    
    [Neo4JRelationship("HAS_TAG")]
    [NodeRelationship("HAS_TAG", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Tag>? Tags { get; set; }
    
    // Relationships Constants
    public const string HAS_INFO = "HAS_INFO";
    public const string HAS_SUMMARY = "HAS_SUMMARY";
    public const string HAS_MINDSET = "HAS_MINDSET";
    public const string HAS_SLOGAN = "HAS_SLOGAN";
    public const string HAS_PROJECT = "HAS_PROJECT";
    public const string HAS_WORK_EXPERIENCE = "HAS_WORK_EXPERIENCE";
    public const string HAS_TAG = "HAS_TAG";
}

using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;
using HotChocolate;

namespace FiveamTechCv.Entities.Nodes;

public class WorkExperience : BaseNode
{
    public string? Company { get; set; }
    public string? CompanyUrl { get; set; }
    public string? Position { get; set; }
    
    [ParameterType(ParameterTypes.ZoneDateTime)]
    [GraphQLType("DateTime")]
    public DateTimeOffset StartDate { get; set; }

    [ParameterType(ParameterTypes.ZoneDateTime)]
    [GraphQLType("DateTime")]
    public DateTimeOffset? EndDate { get; set; }

    public int? Order { get; set; }
    
    
    [Neo4JRelationship("HAS_DESCRIPTION")]
    [NodeRelationship("HAS_DESCRIPTION", NodeRelationType.Create, false, "FiveamTechCv.Abstract.Services.ILocalizedStringService")]
    [ParameterType(ParameterTypes.Ignore)]
    public List<LocalizedString>? Description { get; set; }
    
    [Neo4JRelationship("HAS_PROJECT")]
    [NodeRelationship("HAS_PROJECT", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Project>? Projects { get; set; }
    
    [Neo4JRelationship("HAS_TAG")]
    [NodeRelationship("HAS_TAG", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Tag>? Tags { get; set; }
    
    // To Project
    public const string HAS_PROJECT = "HAS_PROJECT";
    
    // To Tag
    public const string HAS_TAG = "HAS_TAG";
    
    //To Description
    public const string HAS_DESCRIPTION = "HAS_DESCRIPTION";
}
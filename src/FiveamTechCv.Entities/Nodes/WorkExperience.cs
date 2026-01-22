using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;
using HotChocolate;

namespace FiveamTechCv.Entities.Nodes;

public class WorkExperience : BaseVectorizableNode
{
    
    [ParameterType(ParameterTypes.ZoneDateTime)]
    [GraphQLType("DateTime")]
    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
    
    public string? Position { get; set; }

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
    
    [Neo4JRelationship("HAS_COMPANY")]
    [NodeRelationship("HAS_COMPANY", NodeRelationType.Link)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Company>? Companies { get; set; }

    [Neo4JRelationship(FiveamTechCv.Entities.Nodes.Person.HAS_WORK_EXPERIENCE, RelationshipDirection.Incoming)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Person>? People { get; set; }
    
    // To Project
    public const string HAS_PROJECT = "HAS_PROJECT";
    
    // To Tag
    public const string HAS_TAG = "HAS_TAG";
    
    //To Description
    public const string HAS_DESCRIPTION = "HAS_DESCRIPTION";
    
    // To Company
    public const string HAS_COMPANY = "HAS_COMPANY";

    public override string? GetContentToEmbed()
    {
        var desc = string.Join(". ", Description?.Select(d => d.Value).Where(v => !string.IsNullOrEmpty(v)) ?? Array.Empty<string>());
        var companies = string.Join(", ", Companies?.Select(c => c.Name).Where(c => !string.IsNullOrEmpty(c)) ?? Array.Empty<string>());
        var companyStr = !string.IsNullOrEmpty(companies) ? $" at {companies}" : "";
        var people = string.Join(", ", People?.Select(p => $"{p.Name} {p.LastName}").Where(n => !string.IsNullOrWhiteSpace(n)) ?? Array.Empty<string>());
        var peopleStr = !string.IsNullOrEmpty(people) ? $" (Person: {people})" : "";
        
        var tagsStr = "";
        if (Tags != null && Tags.Any())
        {
            var groupedTags = Tags
                .Where(t => t.Type.HasValue && !string.IsNullOrEmpty(t.Name))
                .GroupBy(t => t.Type.Value)
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(t => t.Name))}");
            
            tagsStr = string.Join(". ", groupedTags);
            if (!string.IsNullOrEmpty(tagsStr))
            {
                tagsStr = $". Tags: {tagsStr}";
            }
        }
        
        return $"Work Experience: {Position}{companyStr}{peopleStr}. {desc}.{tagsStr}";
    }
}
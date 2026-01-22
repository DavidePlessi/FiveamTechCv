using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class Project : BaseVectorizableNode
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
    public virtual List<Tag>? Tags { get; set; }
    
    // To Tag
    public const string HAS_TAG = "HAS_TAG";
    
    //To Description
    public const string HAS_DESCRIPTION = "HAS_DESCRIPTION";
    
    [Neo4JRelationship(Person.HAS_PROJECT, RelationshipDirection.Incoming)]
    [ParameterType(ParameterTypes.Ignore)]
    public List<Person>? People { get; set; }

    public override string? GetContentToEmbed()
    {
        var desc = string.Join(". ", Description?.Select(d => d.Value).Where(v => !string.IsNullOrEmpty(v)) ?? Array.Empty<string>());
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

        return $"Project: {Name}{peopleStr}. {desc}{tagsStr}";
    }
}
﻿using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class Project : BaseNode, IVectorizable 
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

    public List<float>? Embedding { get; set; }

    public string? GetContentToEmbed()
    {
        var desc = Description?.Select(d => d.Value).Where(v => !string.IsNullOrEmpty(v)).Aggregate((a, b) => $"{a}. {b}") ?? "";
        return $"Project: {Name}. {desc}";
    }
}
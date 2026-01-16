using System.Text.Json.Serialization;
using FiveamTechCv.Entities.Attributes;
using HotChocolate;

namespace FiveamTechCv.Entities;

public interface IVectorizable
{
    [GraphQLIgnore]
    [JsonIgnore]
    public List<float>? Embedding { get; set; }
    
    [GraphQLIgnore]
    public string? GetContentToEmbed();
}

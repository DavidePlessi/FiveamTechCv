using System.Text.Json.Serialization;

namespace FiveamTechCv.Entities;

public class BaseVectorizableNode : BaseNode, IVectorizable
{
    [JsonIgnore]
    public List<float>? Embedding { get; set; }
    
    public string EmbeddedString { get; set; }
    
    public virtual string? GetContentToEmbed()
    {
        throw new NotImplementedException();
    }
}
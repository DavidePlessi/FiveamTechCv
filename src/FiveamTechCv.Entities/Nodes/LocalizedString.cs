using FiveamTechCv.Entities.Attributes;
using HotChocolate.Data.Neo4J;

namespace FiveamTechCv.Entities.Nodes;

public class LocalizedString : BaseNode
{
    public string? Language { get; set; }
    public string? Value { get; set; }
}
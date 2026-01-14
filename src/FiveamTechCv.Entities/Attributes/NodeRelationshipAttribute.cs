using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Entities.Attributes;

public enum NodeRelationType
{
    Link,
    Create
}

[AttributeUsage(AttributeTargets.Property)]
public class NodeRelationshipAttribute : Attribute
{
    public string Name { get; }
    public NodeRelationType Type { get; }
    public bool Incoming { get; }
    public string? ServiceTypeName { get; }

    public NodeRelationshipAttribute(string name, NodeRelationType type, bool incoming = false, string? serviceTypeName = null)
    {
        Name = name;
        Type = type;
        Incoming = incoming;
        ServiceTypeName = serviceTypeName;
    }
}

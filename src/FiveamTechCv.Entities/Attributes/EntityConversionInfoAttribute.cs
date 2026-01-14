using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Entities.Attributes;

public enum RelationType
{
    None,
    Link,
    Create
}

public class EntityConversionInfoAttribute : Attribute
{
    public bool Ignore { get; set; }
    public string? PropertyName { get; set; }
    public RelationType RelationType { get; set; }
    public Type? LinkedType { get; set; }
    public string? RelationName { get; set; }
    public bool RelationIncoming { get; set; }
    
    public string? ServiceTypeName { get; set; }
    
    public EntityConversionInfoAttribute(bool ignore, string? propName = null)
    {
        Ignore = ignore;
        PropertyName = propName;
        RelationType = RelationType.None;
    }
    
    public EntityConversionInfoAttribute(RelationType relationType, Type linkedType, string relationName, bool relationIncoming = false, string? serviceTypeName = null)
    {
        Ignore = true;
        RelationType = relationType;
        LinkedType = linkedType;
        RelationName = relationName;
        RelationIncoming = relationIncoming;
        ServiceTypeName = serviceTypeName;
    }
}
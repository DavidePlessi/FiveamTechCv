namespace FiveamTechCv.Entities.Attributes;

public class EntityConversionInfoAttribute : Attribute
{
    public bool Ignore { get; set; }
    public string? PropertyName { get; set; }
    
    public EntityConversionInfoAttribute(bool ignore, string? propName = null)
    {
        Ignore = ignore;
        PropertyName = propName;
    }
}
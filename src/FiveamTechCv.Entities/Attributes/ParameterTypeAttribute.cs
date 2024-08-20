namespace FiveamTechCv.Entities.Attributes;

public enum ParameterTypes
{
    Ignore,
    String
}

public class ParameterTypeAttribute : Attribute
{
    public ParameterTypes Type { get; set; }
    public string? PropertyName { get; set; }

    public ParameterTypeAttribute(ParameterTypes type, string? propertyName = null)
    {
        Type = type;
        PropertyName = propertyName;
    }
}
using System.Reflection;
using FiveamTechCv.Entities.Attributes;

namespace FiveamTechCv.Entities;

public class BaseDto<T>  
    where T : BaseNode
{
    public T ToEntity()
    {
        var properties = GetType().GetProperties();
        var entity = Activator.CreateInstance<T>();
        foreach (var prop in properties)
        {
            var conversionToEntity = prop.GetCustomAttribute<EntityConversionInfoAttribute>();
            if (conversionToEntity != null && conversionToEntity.Ignore) continue;
            
            var propName = conversionToEntity?.PropertyName ?? prop.Name;
            var entityProp = typeof(T).GetProperty(propName);
            if (entityProp == null) continue;
            
            var value = prop.GetValue(this);
            
            entityProp.SetValue(entity, value);
        }

        return entity;
    }
}
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
            
            // Handle list of BaseDto conversion to list of BaseNode
            if (value != null && 
                value.GetType().IsGenericType && 
                value.GetType().GetGenericTypeDefinition() == typeof(List<>) &&
                entityProp.PropertyType.IsGenericType &&
                entityProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var inputArgType = value.GetType().GetGenericArguments()[0];
                var targetArgType = entityProp.PropertyType.GetGenericArguments()[0];
                
                // Case 1: List<Dto> -> List<Entity>
                if (inputArgType.IsSubclassOf(typeof(BaseDto<>).MakeGenericType(targetArgType)))
                {
                    var listType = typeof(List<>).MakeGenericType(targetArgType);
                    var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
                    
                    foreach (var item in (System.Collections.IEnumerable)value)
                    {
                        var toEntityMethod = item.GetType().GetMethod("ToEntity");
                        if (toEntityMethod != null)
                        {
                            list.Add(toEntityMethod.Invoke(item, null));
                        }
                    }
                    entityProp.SetValue(entity, list);
                    continue;
                }
                
                // Case 2: List<string> -> List<Entity> (Ids)
                if (inputArgType == typeof(string) && targetArgType.IsSubclassOf(typeof(BaseNode)))
                {
                    var listType = typeof(List<>).MakeGenericType(targetArgType);
                    var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
                    
                    foreach (string id in (System.Collections.IEnumerable)value)
                    {
                        if (string.IsNullOrEmpty(id)) continue;
                        
                        var node = Activator.CreateInstance(targetArgType) as BaseNode;
                        if (node != null)
                        {
                            node.Id = id;
                            list.Add(node);
                        }
                    }
                    entityProp.SetValue(entity, list);
                    continue;
                }
            }
            
            entityProp.SetValue(entity, value);
        }

        return entity;
    }
}
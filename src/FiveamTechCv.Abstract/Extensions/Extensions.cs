using System.Reflection;
using System.Text.Json;
using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Attributes;
using Neo4j.Driver;

namespace FiveamTechCv.Abstract.Extensions;

public static class Extensions
{
    public static T ConvertToEntity<T>(this IRecord record)
    {
        var properties = record.Values.Values.First().As<INode>().Properties;
        var typeProperties = typeof(T).GetProperties();
        var data = new Dictionary<string, object>();

        foreach (var typeProperty in typeProperties)
        {
            var parameterTypeAttribute = typeProperty?.GetCustomAttribute<ParameterTypeAttribute>();
            var propName = parameterTypeAttribute?.PropertyName ?? typeProperty?.Name;
            var value = properties.GetValueOrDefault(propName);
            
            var underlyingType = Nullable.GetUnderlyingType(typeProperty.PropertyType);
            
            if (underlyingType?.IsEnum == true &&
                value != null &&
                value is string
               )
            {
                value = Enum.Parse(underlyingType, value.ToString());
            }
            if (value is ZonedDateTime)
            {
                value = ((ZonedDateTime)value).ToDateTimeOffset();
            }
            data.Add(typeProperty.Name, value);
            
        }
        var json = JsonSerializer.Serialize(data);
        return JsonSerializer.Deserialize<T>(json);
    }

    private static object InnerConvertToParameters(object obj, bool ignoreNull = false)
    {
        var dictionary = new Dictionary<string, object>();
        
        foreach (var prop in obj.GetType().GetProperties())
        {
            var attribute = prop.GetCustomAttribute<ParameterTypeAttribute>();
        
            if(attribute is { Type: ParameterTypes.Ignore })
                continue;
             
            var value = prop.GetValue(obj);
            
            if(value == null && ignoreNull)
                continue;
             
            if(attribute is { Type: ParameterTypes.String })
                value = value.ToString();
             
            if(value is Enum)
                value = value.ToString();
            
            dictionary.Add(prop.Name, value);
        }

        return dictionary;
    }
    
    public static object ConvertToParameters(this BaseNode obj, bool ignoreNull = false)
    {
        return InnerConvertToParameters(obj, ignoreNull);
    }
    
    public static object ConvertToParameters(this INodeFilter obj, bool ignoreNull = false)
    {
        return InnerConvertToParameters(obj, ignoreNull);
    }

    private static string GetPropertyCondition(
        FilterTypeAttribute? typeAttribute, 
        PropertyInfo prop,
        string entityPropName,
        string alias
    )
    {
        var left = $"{alias}.{entityPropName}";
        var right = $"${prop.Name}";
        
        if(prop.PropertyType == typeof(string))
        {
            left = $"lower({left})";
            right = $"lower({right})";
        }

        var middle = typeAttribute?.FilterType switch
        {
            FilterType.Contains => "CONTAINS",
            FilterType.Equal => "=",
            FilterType.GreaterThan => ">",
            FilterType.GreaterThanOrEqual => ">=",
            FilterType.LessThan => "<",
            FilterType.LessThanOrEqual => "<=",
            FilterType.Different => "<>",
            FilterType.StartsWith => "STARTS WITH",
            FilterType.EndsWith => "ENDS WITH",
            _ => "="
        };

        return $"{left} {middle} {right}";
    }
    
    public static string ConvertToCondition(this INodeFilter filter, string alias = "n")
    {
        var conditions = new List<string>();
        
        foreach (var prop in filter.GetType().GetProperties())
        {
            if(prop.GetValue(filter) == null)
                continue;
            
            var parameterTypeAttribute = prop.GetCustomAttribute<ParameterTypeAttribute>();
            var filterTypeAttribute = prop.GetCustomAttribute<FilterTypeAttribute>();
        
            if(parameterTypeAttribute is { Type: ParameterTypes.Ignore })
                continue;

            conditions.Add(GetPropertyCondition(
                filterTypeAttribute,
                prop,
                parameterTypeAttribute?.PropertyName ?? prop.Name,
                alias
            ));
        }

        return conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
    }
}
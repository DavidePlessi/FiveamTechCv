using System.Reflection;
using System.Text.Json;
using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Filters;
using HotChocolate.Data.Neo4J;
using Neo4j.Driver;

namespace FiveamTechCv.Entities.Extensions;

public static class Extensions
{
    public static T ConvertToEntity<T>(this IRecord record)
    {
        var node = record.Values.ContainsKey("n") ? record.Values["n"].As<INode>() : record.Values.Values.First().As<INode>();
        var properties = node.Properties;
        var typeProperties = typeof(T).GetProperties();
        var data = new Dictionary<string, object>();

        foreach (var typeProperty in typeProperties)
        {
            var parameterTypeAttribute = typeProperty?.GetCustomAttribute<ParameterTypeAttribute>();
            var propName = parameterTypeAttribute?.PropertyName ?? typeProperty?.Name;
            
            // Check if it's a relationship property
            var relationshipAttr = typeProperty?.GetCustomAttribute<Neo4JRelationshipAttribute>();
            if (relationshipAttr != null)
            {
                // Try to get related nodes from the record
                // The query should return them as a list of nodes with a key matching the property name or relationship name
                // For example, if we collect related nodes in the query as 'Name', we look for 'Name' in record
                if (record.Keys.Contains(typeProperty.Name))
                {
                    // Use record.Values to access the value by key to avoid potential issues with indexer
                    var relatedValue = record.Values[typeProperty.Name];
                    if (relatedValue != null)
                    {
                        var targetType = typeProperty.PropertyType.IsGenericType 
                            ? typeProperty.PropertyType.GetGenericArguments()[0] 
                            : typeProperty.PropertyType;
                            
                        if (relatedValue is List<object> relatedNodes)
                        {
                             var listType = typeof(List<>).MakeGenericType(targetType);
                             var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
                             
                             foreach (var relatedNodeObj in relatedNodes)
                             {
                                 if (relatedNodeObj is INode relatedNode)
                                 {
                                     var relatedEntity = ConvertNodeToEntity(relatedNode, targetType);
                                     list.Add(relatedEntity);
                                 }
                             }
                             data.Add(typeProperty.Name, list);
                        }
                        else if (relatedValue is INode relatedNode)
                        {
                             var relatedEntity = ConvertNodeToEntity(relatedNode, targetType);
                             data.Add(typeProperty.Name, relatedEntity);
                        }
                    }
                }
                continue;
            }

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
    
    private static object ConvertNodeToEntity(INode node, Type targetType)
    {
        var properties = node.Properties;
        var typeProperties = targetType.GetProperties();
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
        return JsonSerializer.Deserialize(json, targetType);
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
            
             
            if(attribute is { Type: ParameterTypes.ZoneDateTime } && value is DateTimeOffset offset)
                value = new ZonedDateTime(offset);
             
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
using System.Reflection;
using System.Text;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Attributes;
using FiveamTechCv.Entities.Extensions;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Graph;
using HotChocolate.Data.Neo4J;
using Neo4j.Driver;

namespace FiveamTechCv.Core.Services;

public abstract class BaseService<T, TFilter> : INodeService<T, TFilter>
    where T : BaseNode
    where TFilter : INodeFilter
{
    private readonly IDriver _driver;
    private readonly QueryConfig _queryConfig;
    protected readonly IServiceProvider _serviceProvider;
    
    public BaseService(GraphDriver driver, IServiceProvider serviceProvider)
    {
        _queryConfig = driver.QueryConfig;
        _driver = driver.Driver;
        _serviceProvider = serviceProvider;
    }



    public virtual async Task<string> CreateAsync(T node)
    {
        node.Id = Guid.NewGuid().ToString();
        node.UpdatedAt = DateTime.UtcNow.Ticks;

        var param = node.ConvertToParameters();
        
        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"CREATE (n:{typeof(T).Name} $props)");
        
        // Handle Links
        var properties = typeof(T).GetProperties();
        var linkParams = new Dictionary<string, object>();
        
        foreach (var prop in properties)
        {
            var relAttr = prop.GetCustomAttribute<NodeRelationshipAttribute>();
            if (relAttr != null && relAttr.Type == NodeRelationType.Link)
            {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var val = prop.GetValue(node) as System.Collections.IEnumerable;
                    if (val != null)
                    {
                        var ids = new List<string>();
                        foreach (var item in val)
                        {
                            if (item is BaseNode bn && !string.IsNullOrEmpty(bn.Id))
                            {
                                ids.Add(bn.Id);
                            }
                        }
                        
                        if (ids.Any())
                        {
                            var paramName = $"{prop.Name}_ids";
                            linkParams.Add(paramName, ids.ToArray());
                            
                            var targetType = prop.PropertyType.IsGenericType 
                                ? prop.PropertyType.GetGenericArguments()[0] 
                                : prop.PropertyType;
                                
                            queryBuilder.Append($" WITH n");
                            queryBuilder.Append($" MATCH (t_{prop.Name}:{targetType.Name}) WHERE t_{prop.Name}.Id IN ${paramName}");
                            queryBuilder.Append(relAttr.Incoming 
                                ? $" MERGE (n)<-[:{relAttr.Name}]-(t_{prop.Name})" 
                                : $" MERGE (n)-[:{relAttr.Name}]->(t_{prop.Name})");
                        }
                    }
                }
                else if (typeof(BaseNode).IsAssignableFrom(prop.PropertyType))
                {
                     var val = prop.GetValue(node) as BaseNode;
                     if (val != null && !string.IsNullOrEmpty(val.Id))
                     {
                         var paramName = $"{prop.Name}_id";
                         linkParams.Add(paramName, val.Id);
                         var targetType = prop.PropertyType;

                         queryBuilder.Append($" WITH n");
                         queryBuilder.Append($" MATCH (t_{prop.Name}:{targetType.Name}) WHERE t_{prop.Name}.Id = ${paramName}");
                         queryBuilder.Append(relAttr.Incoming 
                             ? $" MERGE (n)<-[:{relAttr.Name}]-(t_{prop.Name})" 
                             : $" MERGE (n)-[:{relAttr.Name}]->(t_{prop.Name})");
                     }
                }
            }
        }
        
        queryBuilder.Append(" RETURN DISTINCT n.Id");
        
        // Combine parameters
        var allParams = new Dictionary<string, object> { { "props", param } };
        foreach (var kvp in linkParams) allParams.Add(kvp.Key, kvp.Value);

        var (query, _) = await _driver.ExecutableQuery(queryBuilder.ToString())
            .WithParameters(allParams)
            .WithConfig(_queryConfig)
            .ExecuteAsync();
            
        var id = query.Select(r => r[0].As<string>()).SingleOrDefault();
        
        if (id == null)
        {
             throw new InvalidOperationException("Failed to create node or retrieve ID.");
        }

        // Handle Creations
        await HandleCreateRelationsAsync(id, node);
        
        return id;
    }

    public virtual async Task<T> UpdateAsync(T node)
    {
        node.UpdatedAt = DateTime.UtcNow.Ticks;
        var param = node.ConvertToParameters();
        
        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"MATCH (n:{typeof(T).Name}) WHERE n.Id = $id SET n += $props");
        
        var properties = typeof(T).GetProperties();
        var linkParams = new Dictionary<string, object>();
        
        foreach (var prop in properties)
        {
            var relAttr = prop.GetCustomAttribute<NodeRelationshipAttribute>();
            if (relAttr != null && relAttr.Type == NodeRelationType.Link)
            {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var targetType = prop.PropertyType.IsGenericType 
                                ? prop.PropertyType.GetGenericArguments()[0] 
                                : prop.PropertyType;

                    // Delete existing relationships of this type
                    queryBuilder.Append($" WITH n");
                    queryBuilder.Append($" OPTIONAL MATCH (n){(relAttr.Incoming ? "<" : "")}-[r_{prop.Name}:{relAttr.Name}]-{(relAttr.Incoming ? "" : ">")}(:{targetType.Name})");
                    queryBuilder.Append($" DELETE r_{prop.Name}");
                    
                    // Create new ones
                    var val = prop.GetValue(node) as System.Collections.IEnumerable;
                    if (val != null)
                    {
                        var ids = new List<string>();
                        foreach (var item in val)
                        {
                             if (item is BaseNode bn && !string.IsNullOrEmpty(bn.Id))
                            {
                                ids.Add(bn.Id);
                            }
                        }
                        
                        if (ids.Any())
                        {
                            var paramName = $"{prop.Name}_ids";
                            linkParams.Add(paramName, ids.ToArray());
                            
                            queryBuilder.Append($" WITH n");
                            queryBuilder.Append($" MATCH (t_{prop.Name}:{targetType.Name}) WHERE t_{prop.Name}.Id IN ${paramName}");
                            queryBuilder.Append(relAttr.Incoming 
                                ? $" MERGE (n)<-[:{relAttr.Name}]-(t_{prop.Name})" 
                                : $" MERGE (n)-[:{relAttr.Name}]->(t_{prop.Name})");
                        }
                    }
                }
                else if (typeof(BaseNode).IsAssignableFrom(prop.PropertyType))
                {
                     var targetType = prop.PropertyType;

                     // Delete existing relationships of this type
                     queryBuilder.Append($" WITH n");
                     queryBuilder.Append($" OPTIONAL MATCH (n){(relAttr.Incoming ? "<" : "")}-[r_{prop.Name}:{relAttr.Name}]-{(relAttr.Incoming ? "" : ">")}(:{targetType.Name})");
                     queryBuilder.Append($" DELETE r_{prop.Name}");

                     var val = prop.GetValue(node) as BaseNode;
                     if (val != null && !string.IsNullOrEmpty(val.Id))
                     {
                         var paramName = $"{prop.Name}_id";
                         linkParams.Add(paramName, val.Id);
                         
                         queryBuilder.Append($" WITH n");
                         queryBuilder.Append($" MATCH (t_{prop.Name}:{targetType.Name}) WHERE t_{prop.Name}.Id = ${paramName}");
                         queryBuilder.Append(relAttr.Incoming 
                             ? $" MERGE (n)<-[:{relAttr.Name}]-(t_{prop.Name})" 
                             : $" MERGE (n)-[:{relAttr.Name}]->(t_{prop.Name})");
                     }
                }
            }
        }
        
        queryBuilder.Append(" RETURN DISTINCT n");
        
        var allParams = new Dictionary<string, object> { { "id", node.Id }, { "props", param } };
        foreach (var kvp in linkParams) allParams.Add(kvp.Key, kvp.Value);

        var (query, _) = await _driver.ExecutableQuery(queryBuilder.ToString())
            .WithParameters(allParams)
            .WithConfig(_queryConfig)
            .ExecuteAsync();
        
        // Handle Creations (update logic: delete old, create new?)
        // The old logic in HandleRelationsAsync did: delete old nodes, create new ones.
        // We'll reproduce that behavior using inspections of T.
        await HandleCreateRelationsAsync(node.Id, node, true);
        
        return query.Select(r => r.ConvertToEntity<T>()).Single();
    }
    
    protected virtual async Task HandleCreateRelationsAsync(string id, T node, bool clearExisting = false)
    {
         var properties = typeof(T).GetProperties();
         foreach (var prop in properties)
         {
             var relAttr = prop.GetCustomAttribute<NodeRelationshipAttribute>();
             if (relAttr != null && relAttr.Type == NodeRelationType.Create)
             {
                 object? serviceInstance = null;
                 if (relAttr.ServiceTypeName != null)
                 {
                     var serviceType = typeof(INodeService<,>).Assembly.GetType(relAttr.ServiceTypeName);
                     if (serviceType != null)
                     {
                         serviceInstance = _serviceProvider.GetService(serviceType);
                     }
                 }
                 if (serviceInstance == null) continue;

                 var list = prop.GetValue(node) as System.Collections.IEnumerable;
                 if (list == null) continue;

                 if (clearExisting)
                 {
                     // Fetch current entity to get old IDs to delete?
                     // Or just query related nodes and delete them via service?
                     // "MATCH (n)-[:REL]-(target) RETURN target.Id"
                     // Then call service.DeleteAsync(id).
                     // This mimics old behavior.
                     
                     var targetType = prop.PropertyType.IsGenericType 
                            ? prop.PropertyType.GetGenericArguments()[0] 
                            : prop.PropertyType;
                            
                     var q = _driver.ExecutableQuery(
                        $"MATCH (n:{typeof(T).Name}){(relAttr.Incoming ? "<" : "")}-[:{relAttr.Name}]-{(relAttr.Incoming ? "" : ">")}(t:{targetType.Name}) WHERE n.Id = $id RETURN t.Id"
                     ).WithParameters(new { id });
                     
                     var (res, _) = await q.ExecuteAsync();
                     var oldIds = res.Select(r => r[0].As<string>()).ToList();
                     
                     var deleteMethod = serviceInstance.GetType().GetMethod("DeleteAsync");
                     if (deleteMethod != null)
                     {
                         foreach (var oldId in oldIds)
                         {
                             var task = deleteMethod.Invoke(serviceInstance, new object[] { oldId }) as Task<bool>;
                             if (task != null) await task;
                         }
                     }
                 }

                 var ids = new List<string>();
                 var createMethod = serviceInstance.GetType().GetMethod("CreateAsync");
                 if (createMethod != null)
                 {
                     foreach (var item in list)
                     {
                         // item is BaseNode.
                         // But CreateAsync takes TNode.
                         // Reflection Invoke should work.
                         var task = createMethod.Invoke(serviceInstance, new[] { item }) as Task<string>;
                         if (task != null)
                         {
                             ids.Add(await task);
                         }
                     }
                 }
                 
                 if (ids.Any())
                 {
                     await CreateRelationAsync(
                         id, 
                         ids.ToArray(), 
                         prop.PropertyType.IsGenericType ? prop.PropertyType.GetGenericArguments()[0] : prop.PropertyType, 
                         relAttr.Name, 
                         relAttr.Incoming
                     );
                 }
             }
         }
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var (_, resultSummary) = await _driver.ExecutableQuery(
                $"MATCH (n:{typeof(T).Name}) WHERE n.Id = $id DETACH DELETE n"
            ).WithParameters(new { id = id })
            .WithConfig(_queryConfig)
            .ExecuteAsync();

        return resultSummary.Counters.NodesDeleted > 0;
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        var (matchClause, returnClause) = BuildMatchAndReturnClauses(" { Id: $id }");
        
        var queryStr = $"{matchClause} {returnClause}";

        var (query, _) = await _driver.ExecutableQuery(queryStr)
            .WithParameters(new { id = id})
            .WithConfig(_queryConfig)
            .ExecuteAsync();
        
        return query.Select(r => r.ConvertToEntity<T>()).SingleOrDefault();
    }

    public virtual async Task<IEnumerable<T>> ListAsync(TFilter filter)
    {
        var (matchClause, returnClause) = BuildMatchAndReturnClauses();
        
        var queryStr = $"{matchClause} {filter.ToCypherCondition()} {returnClause}";

        var q = _driver.ExecutableQuery(queryStr)
            .WithParameters(filter.ToCypherParams())
            .WithConfig(_queryConfig);
        
        var (query, _) = await q.ExecuteAsync();
        
        return query.Select(r => r.ConvertToEntity<T>());
    }

    public async Task<int> CreateRelationAsync(
        string fromId, 
        string[] toId, 
        Type toType,
        string relation,
        bool relationIncoming = false
    )
    {
        var q = _driver.ExecutableQuery(
                $"MATCH (from:{typeof(T).Name}),(to:{toType.Name}) " +
                $"WHERE from.Id = $fromId AND to.Id IN $toId " +
                (relationIncoming 
                    ? $"MERGE(from)<-[:{relation}]-(to) "
                    : $"MERGE(from)-[:{relation}]->(to) ")
            ).WithParameters(new { fromId = fromId, toId = toId })
            .WithConfig(_queryConfig);
        
        var (_, resultSummary) = await q.ExecuteAsync();

        return resultSummary.Counters.RelationshipsCreated;
    }

    public async Task<int> DeleteRelationAsync(
        string fromId,
        string[] toId,
        Type toType,
        string relation,
        bool relationIncoming = false
    )
    {
        var q = _driver.ExecutableQuery(
                $"MATCH (from:{typeof(T).Name}){ (relationIncoming ? "<" : "") }-[r:{relation}]-{ (relationIncoming ? "" : ">") }(to:{toType.Name}) " +
                $"WHERE from.Id = $fromId " +
                (toId != null ? "AND to.Id IN $toId " : "") +
                $"DELETE r"
            ).WithParameters(new { fromId = fromId, toId = toId })
            .WithConfig(_queryConfig);

        var (_, resultSummary) = await q.ExecuteAsync();

        return resultSummary.Counters.RelationshipsDeleted;
    }
    
    private (string MatchClause, string ReturnClause) BuildMatchAndReturnClauses(string? nodeCriteria = null)
    {
        var matchBuilder = new StringBuilder($"MATCH (n:{typeof(T).Name}{ nodeCriteria ?? "" })");
        var returnBuilder = new StringBuilder("RETURN n");
        var properties = typeof(T).GetProperties();
        
        foreach (var prop in properties)
        {
            var relationshipAttr = prop.GetCustomAttribute<Neo4JRelationshipAttribute>();
            if (relationshipAttr != null)
            {
                var targetType = prop.PropertyType.IsGenericType 
                    ? prop.PropertyType.GetGenericArguments()[0] 
                    : prop.PropertyType;

                var direction = relationshipAttr.Direction == RelationshipDirection.Incoming 
                    ? "<-" 
                    : "-";
                var directionEnd = relationshipAttr.Direction == RelationshipDirection.Incoming 
                    ? "-" 
                    : "->";
                
                // Use OPTIONAL MATCH to include the main node even if relationship doesn't exist
                // We use a unique alias for the related node based on property name to avoid conflicts
                var alias = prop.Name;
                matchBuilder.Append($" OPTIONAL MATCH (n){direction}[:{relationshipAttr.Name}]{directionEnd}({alias}:{targetType.Name})");
                
                // Collect related nodes into a list in the return statement
                // The alias in return must match what ConvertToEntity expects (which is the property name)
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                     returnBuilder.Append($", collect(DISTINCT({alias})) AS {prop.Name}");
                }
                else
                {
                     returnBuilder.Append($", head(collect(DISTINCT({alias}))) AS {prop.Name}");
                }
            }
        }
        
        return (matchBuilder.ToString(), returnBuilder.ToString());
    }
}
using System.Reflection;
using System.Text;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
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
    public BaseService(GraphDriver driver)
    {
        _queryConfig = driver.QueryConfig;
        _driver = driver.Driver;
    }

    public virtual async Task<string> CreateAsync(T node)
    {
        node.Id = Guid.NewGuid().ToString();
        node.UpdatedAt = DateTime.UtcNow.Ticks;

        var param = node.ConvertToParameters();
        
        var (query, _) = await _driver.ExecutableQuery(
            $"CREATE (n:{typeof(T).Name} $props) RETURN n.Id"
        ).WithParameters(new { props = param })
        .WithConfig(_queryConfig)
        .ExecuteAsync();
        
        return query.Select(r => r[0].As<string>()).Single();
    }

    public virtual async Task<T> UpdateAsync(T node)
    {
        node.UpdatedAt = DateTime.UtcNow.Ticks;
        var (query, _) = await _driver.ExecutableQuery(
                $"MATCH (n:{typeof(T).Name}) WHERE n.Id = $id SET n += $props RETURN n"
            ).WithParameters(new { id = node.Id, props = node.ConvertToParameters() })
            .WithConfig(_queryConfig)
            .ExecuteAsync();
        
        return query.Select(r => r.ConvertToEntity<T>()).Single();
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var (_, resultSummary) = await _driver.ExecutableQuery(
                $"MATCH (n:{typeof(T).Name}) WHERE n.Id = $id DELETE n"
            ).WithParameters(new { id = id })
            .WithConfig(_queryConfig)
            .ExecuteAsync();

        return resultSummary.Counters.NodesDeleted > 0;
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        var (matchClause, returnClause) = BuildMatchAndReturnClauses();
        
        var queryStr = $"{matchClause} WHERE n.Id = $id {returnClause}";

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
    
    private (string MatchClause, string ReturnClause) BuildMatchAndReturnClauses()
    {
        var matchBuilder = new StringBuilder($"MATCH (n:{typeof(T).Name})");
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
                returnBuilder.Append($", collect(DISTINCT({alias})) AS {prop.Name}");
            }
        }
        
        return (matchBuilder.ToString(), returnBuilder.ToString());
    }
}
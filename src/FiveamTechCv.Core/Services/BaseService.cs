using FiveamTechCv.Abstract.Extensions;
using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Graph;
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
        node.CreatedAt = DateTime.UtcNow;
        node.UpdatedAt = DateTime.UtcNow;

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
        node.UpdatedAt = DateTime.UtcNow;
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
        var (query, _) = await _driver.ExecutableQuery(
                $"MATCH (n:{typeof(T).Name}) WHERE n.Id = $id RETURN n"
            ).WithParameters(new { id = id})
            .WithConfig(_queryConfig)
            .ExecuteAsync();
        
        return query.Select(r => r.ConvertToEntity<T>()).Single();
    }

    public virtual async Task<IEnumerable<T>> ListAsync(TFilter filter)
    {
        var q = _driver.ExecutableQuery(
                $"MATCH (n:{typeof(T).Name}) {filter.ToCypherCondition()}  RETURN n"
            )
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
}
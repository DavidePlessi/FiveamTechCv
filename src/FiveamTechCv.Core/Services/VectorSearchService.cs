using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Extensions;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;
using INode = Neo4j.Driver.INode;
using Tag = FiveamTechCv.Entities.Nodes.Tag;

namespace FiveamTechCv.Core.Services;

public class VectorSearchService : IVectorSearchService
{
    private readonly IDriver _driver;
    private readonly IGeminiService _geminiService;
    private readonly QueryConfig _queryConfig;
    private const string IndexName = "vector_index";

    public VectorSearchService(IDriver driver, GraphDriver graphDriver, IGeminiService geminiService)
    {
        _driver = driver;
        _geminiService = geminiService;
        _queryConfig = graphDriver.QueryConfig;
    }

    public async Task<List<BaseNode>> SearchAsync(string query, int k = 5)
    {
        var embedding = await _geminiService.GenerateEmbeddingAsync(query);
        
        var (results, _) = await _driver.ExecutableQuery(
            $"CALL db.index.vector.queryNodes($indexName, $k, $embedding) YIELD node, score RETURN node, score"
        )
        .WithParameters(new { indexName = IndexName, k, embedding })
        .WithConfig(_queryConfig)
        .ExecuteAsync();

        var nodes = new List<BaseNode>();

        foreach (var record in results)
        {
            var node = record["node"].As<INode>();
            BaseNode? entity = null;
            
            if (node.Labels.Contains(nameof(Project)))
            {
                entity = record.ConvertToEntity<Project>();
            }
            else if (node.Labels.Contains(nameof(WorkExperience)))
            {
                entity = record.ConvertToEntity<WorkExperience>();
            }
            else if (node.Labels.Contains(nameof(Tag)))
            {
                entity = record.ConvertToEntity<Tag>();
            }
            
            if (entity != null)
            {
                nodes.Add(entity);
            }
        }

        return nodes;
    }
    
    public async Task CreateIndexAsync()
    {
        await _driver.ExecutableQuery(
            @"CREATE VECTOR INDEX vector_index IF NOT EXISTS
              FOR (n:Vectorizable)
              ON (n.Embedding)
              OPTIONS {indexConfig: {
               `vector.dimensions`: 768,
               `vector.similarity_function`: 'cosine'
              }}"
        ).WithConfig(_queryConfig).ExecuteAsync();
    }
}

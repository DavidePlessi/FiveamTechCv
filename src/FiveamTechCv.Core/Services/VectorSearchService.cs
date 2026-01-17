using System.Text.Json;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.DTO;
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

    public async Task<List<SearchResultDto>> SearchAsync(string query, int k = 5)
    {
        var embedding = await _geminiService.GenerateEmbeddingAsync(query);
        
        var (results, _) = await _driver.ExecutableQuery(
            $"CALL db.index.vector.queryNodes($indexName, $k, $embedding) YIELD node, score RETURN node, score"
        )
        .WithParameters(new { indexName = IndexName, k, embedding })
        .WithConfig(_queryConfig)
        .ExecuteAsync();

        var nodes = new List<SearchResultDto>();

        foreach (var record in results)
        {
            var node = record["node"].As<INode>();
            var score = record["score"].As<double>();
            var embeddedContent = "";
            
            BaseVectorizableNode? entity = null;
            string label = "";
            
            if (node.Labels.Contains(nameof(Project)))
            {
                entity = record.ConvertToEntity<Project>();
                embeddedContent = (entity as Project)!.EmbeddedString;
                label = nameof(Project);
            }
            else if (node.Labels.Contains(nameof(WorkExperience)))
            {
                entity = record.ConvertToEntity<WorkExperience>();
                embeddedContent = (entity as WorkExperience)!.EmbeddedString;
                label = nameof(WorkExperience);
            }
            else if (node.Labels.Contains(nameof(Tag)))
            {
                entity = record.ConvertToEntity<Tag>();
                embeddedContent = (entity as Tag)!.EmbeddedString;
                label = nameof(Tag);
            }
            else if (node.Labels.Contains(nameof(Person)))
            {
                entity = record.ConvertToEntity<Person>();
                embeddedContent = (entity as Person)!.EmbeddedString;
                label = nameof(Person);
            }
            
            if (entity != null)
            {
                nodes.Add(new SearchResultDto
                {
                    Id = entity.Id,
                    Label = label,
                    Properties = JsonSerializer.Serialize((object)entity),
                    EmbeddedString = embeddedContent,
                    Score = score
                });
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

    public async Task RecalculateEmbeddingsAsync()
    {
        await RecalculateTagsAsync();
        await RecalculateProjectsAsync();
        await RecalculateWorkExperiencesAsync();
        await RecalculatePersonsAsync();
    }

    private async Task RecalculateTagsAsync()
    {
        var query = @"MATCH (n:Tag) RETURN n";
        var (results, _) = await _driver.ExecutableQuery(query)
            .WithConfig(_queryConfig)
            .ExecuteAsync();

        foreach (var record in results)
        {
            var tag = record.ConvertToEntity<Tag>();
            if (tag != null)
            {
                var content = tag.GetContentToEmbed();
                if (!string.IsNullOrEmpty(content))
                {
                    var embedding = await _geminiService.GenerateEmbeddingAsync(content);
                    await UpdateEmbeddingAsync(tag.Id, embedding, content);
                }
            }
        }
    }

    private async Task RecalculateProjectsAsync()
    {
        var query = @"
            MATCH (p:Project)
            OPTIONAL MATCH (p)-[:HAS_DESCRIPTION]->(d:LocalizedString)
            OPTIONAL MATCH (person:Person)-[:HAS_PROJECT]->(p)
            OPTIONAL MATCH (p)-[:HAS_TAG]->(t:Tag)
            RETURN p, collect(DISTINCT d) as Description, collect(DISTINCT person) as People, collect(DISTINCT t) as Tags";
            
        var (results, _) = await _driver.ExecutableQuery(query)
            .WithConfig(_queryConfig)
            .ExecuteAsync();

        foreach (var record in results)
        {
            var project = record.ConvertToEntity<Project>();
            
            if (project != null)
            {
                var content = project.GetContentToEmbed();
                if (!string.IsNullOrEmpty(content))
                {
                    var embedding = await _geminiService.GenerateEmbeddingAsync(content);
                    await UpdateEmbeddingAsync(project.Id, embedding, content);
                }
            }
        }
    }

    private async Task RecalculateWorkExperiencesAsync()
    {
        var query = @"
            MATCH (w:WorkExperience)
            OPTIONAL MATCH (w)-[:HAS_DESCRIPTION]->(d:LocalizedString)
            OPTIONAL MATCH (w)-[:HAS_COMPANY]->(c:Company)
            OPTIONAL MATCH (person:Person)-[:HAS_WORK_EXPERIENCE]->(w)
            OPTIONAL MATCH (w)-[:HAS_TAG]->(t:Tag)
            RETURN w, collect(DISTINCT d) as Description, collect(DISTINCT c) as Companies, collect(DISTINCT person) as People, collect(DISTINCT t) as Tags";

        var (results, _) = await _driver.ExecutableQuery(query)
            .WithConfig(_queryConfig)
            .ExecuteAsync();

        foreach (var record in results)
        {
            var workExp = record.ConvertToEntity<WorkExperience>();

            if (workExp != null)
            {
                var content = workExp.GetContentToEmbed();
                if (!string.IsNullOrEmpty(content))
                {
                    var embedding = await _geminiService.GenerateEmbeddingAsync(content);
                    await UpdateEmbeddingAsync(workExp.Id, embedding, content);
                }
            }
        }
    }
    
    private async Task RecalculatePersonsAsync()
    {
        var query = @"
            MATCH (p:Person)
            OPTIONAL MATCH (p)-[:HAS_INFO]->(i:LocalizedString)
            OPTIONAL MATCH (p)-[:HAS_SUMMARY]->(s:LocalizedString)
            OPTIONAL MATCH (p)-[:HAS_MINDSET]->(m:LocalizedString)
            OPTIONAL MATCH (p)-[:HAS_SLOGAN]->(sl:LocalizedString)
            OPTIONAL MATCH (p)-[:HAS_TAG]->(t:Tag)
            RETURN p, collect(DISTINCT i) as Info, collect(DISTINCT s) as Summary, collect(DISTINCT m) as Mindset, collect(DISTINCT sl) as Slogan, collect(DISTINCT t) as Tags";

        var (results, _) = await _driver.ExecutableQuery(query)
            .WithConfig(_queryConfig)
            .ExecuteAsync();

        foreach (var record in results)
        {
            var person = record.ConvertToEntity<Person>();

            if (person != null)
            {
                var content = person.GetContentToEmbed();
                if (!string.IsNullOrEmpty(content))
                {
                    var embedding = await _geminiService.GenerateEmbeddingAsync(content);
                    await UpdateEmbeddingAsync(person.Id, embedding, content);
                }
            }
        }
    }

    private async Task UpdateEmbeddingAsync(string? id, List<double> embedding, string embeddedString)
    {
        if (string.IsNullOrEmpty(id)) return;

        await _driver.ExecutableQuery(
            "MATCH (n) WHERE n.Id = $id SET n.Embedding = $embedding, n:Vectorizable, n.EmbeddedString = $embeddedString"
        )
        .WithParameters(new { id, embedding, embeddedString })
        .WithConfig(_queryConfig)
        .ExecuteAsync();
    }
}

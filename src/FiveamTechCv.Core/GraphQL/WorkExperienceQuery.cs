using FiveamTechCv.Entities.Nodes;
using HotChocolate.Data;
using HotChocolate.Data.Neo4J;
using HotChocolate.Data.Neo4J.Execution;
using Neo4j.Driver;
using Tag = FiveamTechCv.Entities.Nodes.Tag;

namespace FiveamTechCv.Core;

[ExtendObjectType(Name = "Query")]
public class WorkExperienceQuery
{
    [GraphQLName("workExperiences")]
    [UseNeo4JDatabase(databaseName: "neo4j")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public Neo4JExecutable<WorkExperience> GetTags(
        [ScopedService] IAsyncSession session
    ) => new (session);
    
}
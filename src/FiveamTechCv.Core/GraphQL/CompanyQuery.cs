using FiveamTechCv.Entities.Nodes;
using HotChocolate.Data;
using HotChocolate.Data.Neo4J;
using HotChocolate.Data.Neo4J.Execution;
using Neo4j.Driver;

namespace FiveamTechCv.Core;

[ExtendObjectType(Name = "Query")]
public class CompanyQuery
{
    [GraphQLName("companies")]
    [UseNeo4JDatabase(databaseName: "neo4j")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public Neo4JExecutable<Company> GetCompanies(
        [ScopedService] IAsyncSession session
    ) => new (session);
    
}

using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace FiveamTechCv.Graph;

public class GraphDriver
{

    public IDriver Driver { get; }
    public QueryConfig QueryConfig { get; }
    
    public GraphDriver(IDriver driver, QueryConfig queryConfig)
    {
        Driver = driver;
        QueryConfig = queryConfig;
    }
}

public class DriverFactory
{
    private readonly string _uri;
    private readonly string _username;
    private readonly string _password;

    public DriverFactory(IOptions<GraphDatabaseOptions> options)
    {
        _uri = options.Value.Uri;
        _username = options.Value.Username;
        _password = options.Value.Password;
    }
    
    public GraphDriver CreateDriver()
    {
        return new GraphDriver(
            GraphDatabase.Driver(_uri, AuthTokens.Basic(_username, _password)),
            new QueryConfig()
        );
    }
}

public class GraphDatabaseOptions
{
    public string Uri { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
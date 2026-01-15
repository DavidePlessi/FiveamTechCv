using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Core;
using FiveamTechCv.Core.Services;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using FiveamTechCv.Server.Middlewares;
using HotChocolate.Data.Neo4J;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using ServiceStack;
using ServiceStack.Text;

var builder = WebApplication.CreateBuilder(args);
const string corsPolicy = "_corsPolicy";

// Register ServiceStack Converters for Neo4j Types
JsConfig.Init(new ServiceStack.Text.Config
{
    DateHandler = DateHandler.ISO8601
});

// Configure serialization for Neo4j types to ensure they are treated as ISO8601 strings
// This is a fallback for when AutoMapping doesn't pick up the converter directly
JsConfig<ZonedDateTime>.SerializeFn = z => z.ToDateTimeOffset().ToString("o");
JsConfig<LocalDate>.SerializeFn = z => z.ToDateTime().ToString("o");

AutoMapping.RegisterConverter<ZonedDateTime, DateTimeOffset>(z => z.ToDateTimeOffset());
AutoMapping.RegisterConverter<LocalDate, DateTime>(z => z.ToDateTime());

// Register a converter for Dictionary<string, object> to DateTimeOffset
// This handles the case where Neo4j driver returns a complex object structure (as a Dictionary)
// instead of a ZonedDateTime object, which ServiceStack then tries to deserialize.
AutoMapping.RegisterConverter<Dictionary<string, object>, DateTimeOffset>(d => 
{
    // Check if it looks like a ZonedDateTime structure
    if (d.ContainsKey("UtcDateTime") && d["UtcDateTime"] is string utcStr)
    {
        // It seems the error message showed UtcDateTime as a string in the dictionary representation
        // "UtcDateTime:2019-08-01T12:00:00Z"
        return DateTimeOffset.Parse(utcStr);
    }
    
    // If we have UtcSeconds (epoch), we can use that
    if (d.ContainsKey("UtcSeconds") && (d["UtcSeconds"] is long || d["UtcSeconds"] is int))
    {
        long seconds = Convert.ToInt64(d["UtcSeconds"]);
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    // Fallback: try to find any date-like string
    return DateTimeOffset.MinValue; 
});

// Register a converter for string to DateTimeOffset
// The error message indicates that ServiceStack is trying to parse a string representation of the object
// '{UnknownZoneInfo:False,Reason:0,Ambiguous:False,UtcSeconds:1564660800...}'
// We need to handle this string if it comes through as a string.
AutoMapping.RegisterConverter<string, DateTimeOffset>(s => 
{
    if (string.IsNullOrEmpty(s)) return DateTimeOffset.MinValue;
    
    // Try standard parsing first
    if (DateTimeOffset.TryParse(s, out var result))
    {
        return result;
    }
    
    // If it's the complex string representation from the error
    if (s.Contains("UtcSeconds") && s.Contains("UtcDateTime"))
    {
        // Extract UtcDateTime value
        // Pattern: UtcDateTime:2019-08-01T12:00:00Z
        var match = System.Text.RegularExpressions.Regex.Match(s, @"UtcDateTime:([^,]+)");
        if (match.Success && DateTimeOffset.TryParse(match.Groups[1].Value, out var extracted))
        {
            return extracted;
        }
    }
    
    return DateTimeOffset.MinValue;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FiveamTechCV", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        corsPolicy,
        b =>
            b.AllowAnyHeader()
                .SetIsOriginAllowed(a => true)
                .AllowAnyMethod()
                .AllowCredentials()
    );
});

builder.Services.Configure<GraphDatabaseOptions>(builder.Configuration.GetSection("GraphDatabase"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<DriverFactory>();
builder.Services.AddSingleton<GraphDriver>(sp => sp.GetRequiredService<DriverFactory>().CreateDriver());
builder.Services.AddSingleton<IDriver>(sp => sp.GetRequiredService<GraphDriver>().Driver);

builder.Services
    .AddGraphQLServer()
    .AddQueryType(q => q.Name("Query"))
    .AddType<TaqQuery>()
    .AddType<ProjectQuery>()
    .AddType<WorkExperienceQuery>()
    .AddType<PersonQuery>()
    .AddType<CompanyQuery>()
    .AddNeo4JFiltering()
    .AddNeo4JSorting()
    .AddNeo4JProjections();



builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IWorkExperienceService, WorkExperienceService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocalizedStringService, LocalizedStringService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddFiveamTechCvApi(builder.Configuration);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

app.UseCors(corsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseFiveamTechCvApi();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(corsPolicy);

app.UseExceptionHandlerMiddleware();

app.UseEndpoints(ep =>
{
    ep.MapGraphQL();
});

app.UseSpa(spa =>
{
    spa.Options.DefaultPage = "/index.html";
});

app.Run();
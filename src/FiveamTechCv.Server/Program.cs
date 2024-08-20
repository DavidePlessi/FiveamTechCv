using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Core;
using FiveamTechCv.Core.Services;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using HotChocolate.Data.Neo4J;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);
const string corsPolicy = "_corsPolicy";


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
    .AddNeo4JFiltering()
    .AddNeo4JSorting()
    .AddNeo4JProjections();



builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IWorkExperienceService, WorkExperienceService>();
builder.Services.AddScoped<IUserService, UserService>();

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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseFiveamTechCvApi();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(corsPolicy);

app.UseEndpoints(ep =>
{
    ep.MapGraphQL();
});


app.UseHttpsRedirection();

app.Run();
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/project")]
public class ProjectController : BaseController<Project, ProjectFilter, ProjectDto>
{
    private readonly ILocalizedStringService _localizedStringService;
    private readonly ITagService _tagService;
    private readonly HttpClient _httpClient;

    public ProjectController(
        IProjectService service, 
        ILocalizedStringService localizedStringService,
        ITagService tagService) : base(service)
    {
        _localizedStringService = localizedStringService;
        _tagService = tagService;
        _httpClient = new HttpClient();
    }
    
    [HttpPost]
    public override async Task<string> CreateAsync(ProjectDto dto)
    {
        var entityId = await base.CreateAsync(dto);
        var tagId = (dto.TagIdsToLink ?? [])
            .Where(t => !string.IsNullOrEmpty(t))
            .ToArray();

        if (tagId.Length > 0)
        {
            await _service.CreateRelationAsync(
                entityId, 
                tagId, 
                typeof(Tag), 
                Project.HAS_TAG
            );
        }
        
        if (dto.Description != null)
        {
            foreach (var description in dto.Description)
            {
                var descriptionId = await _localizedStringService.CreateAsync(new LocalizedString
                {
                    Language = description.Language,
                    Value = description.Value
                });
                
                await _service.CreateRelationAsync(
                    entityId, 
                    [descriptionId], 
                    typeof(LocalizedString), 
                    Project.HAS_DESCRIPTION
                );
            }
        }

        return entityId;
    }

    [HttpPost("{fromId}/relate-tag")]
    public async Task<int> CreateRelationAsync(
        string fromId, 
        string[] toId
    )
    {
        return await _service.CreateRelationAsync(
            fromId, 
            toId, 
            typeof(Tag), 
            Project.HAS_TAG
        );
    }

    [HttpPost("update-from-strapi")]
    public async Task<int> UpdateFromStrapi()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://strapi.fiveamtech.it/projects-data-fts");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonNode = JsonNode.Parse(content);

        var technologies = new List<string>();

        if (jsonNode is not JsonArray jsonArray) return 0;
        
        //TAGS
        foreach (var item in jsonArray)
        {
            var listOfTech = item["technologies"].AsArray().Select(x => x?.GetValue<string>()).ToArray();
            technologies.AddRange(listOfTech);
        }
        var flattenTech = technologies.Distinct();
        var insertedTags = (await _tagService.ListAsync(new TagFilter())).ToList();
        var tagsToInsert = flattenTech.Where(x => !insertedTags.Select(y => y.Name).Contains(x));
        foreach (var tag in tagsToInsert)
        {
            await _tagService.CreateAsync( new Tag 
                {
                    Name = tag,
                    DocumentationLink = "",
                    Type = TagType.Technology

                }
            );
        }

        if (insertedTags.FirstOrDefault(x => x.Name == "enterprise") == null)
        {
            await _tagService.CreateAsync( new Tag 
                {
                    Name = "enterprise",
                    DocumentationLink = "",
                    Type = TagType.Category

                }
            );
        }

        if (insertedTags.FirstOrDefault(x => x.Name == "enterprise") == null)
        {
            await _tagService.CreateAsync( new Tag 
                {
                    Name = "opensource",
                    DocumentationLink = "",
                    Type = TagType.Category

                }
            );
        }

        if (insertedTags.FirstOrDefault(x => x.Name == "enterprise") == null)
        {
            await _tagService.CreateAsync( new Tag 
                {
                    Name = "fun",
                    DocumentationLink = "",
                    Type = TagType.Category

                }
            );
        }
        
        insertedTags = (await _tagService.ListAsync(new TagFilter())).ToList();
        
        //PROJECTS
        var insertedProject = (await _service.ListAsync(new ProjectFilter())).ToList();
        foreach (var item in jsonArray)
        {
            if(insertedProject?.Select(x => x.Name).Contains(item["name"].GetValue<string>()) ?? true)
                continue;
            var tagsToLink = item["technologies"]
                .AsArray()
                .Select(x => x?.GetValue<string>())
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => insertedTags.Where(y => y.Name == x).Select(y => y.Id).FirstOrDefault())
                .ToList();

            switch (item["categories"].GetValue<string>())
            {
                case "enterprise":
                    tagsToLink.Add( insertedTags.Where(y => y.Name == "enterprise").Select(y => y.Id).FirstOrDefault());
                    break;
                case "opensource":
                    tagsToLink.Add( insertedTags.Where(y => y.Name == "opensource").Select(y => y.Id).FirstOrDefault());
                    break;
                case "fun":
                    tagsToLink.Add( insertedTags.Where(y => y.Name == "fun").Select(y => y.Id).FirstOrDefault());
                    break;
                default:
                    break;
            }
            
            var project = new ProjectDto
            {
                Name = item["name"].GetValue<string>(),
                Description = new List<LocalizedStringDto>()
                {
                    new() { Language = "EN", Value = item["description"].GetValue<string>() }
                },
                TagIdsToLink = tagsToLink
            };
            
            await CreateAsync(project);
        }

        return 0;
    }
}
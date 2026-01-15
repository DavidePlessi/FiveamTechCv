using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using ServiceStack;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/project")]
public class ProjectController : BaseController<Project, ProjectFilter, ProjectDto>
{
    private readonly ILocalizedStringService _localizedStringService;
    private readonly ITagService _tagService;
    private readonly IWorkExperienceService _workExperienceService;
    private readonly HttpClient _httpClient;

    public ProjectController(
        IProjectService service, 
        ILocalizedStringService localizedStringService,
        ITagService tagService,
        IWorkExperienceService workExperienceService
    ) : base(service)
    {
        _localizedStringService = localizedStringService;
        _workExperienceService = workExperienceService;
        _tagService = tagService;
        _httpClient = new HttpClient();
    }


    // [HttpPost("update-from-strapi")]
    // [Authorize]
    // public async Task<int> UpdateFromStrapi()
    // {
    //     var response = await _httpClient.GetAsync("https://strapi.fiveamtech.it/projects-data-fts");
    //     response.EnsureSuccessStatusCode();
    //
    //     var content = await response.Content.ReadAsStringAsync();
    //     var jsonNode = JsonNode.Parse(content);
    //
    //     var technologies = new List<string>();
    //
    //     if (jsonNode is not JsonArray jsonArray) return 0;
    //     
    //     //TAGS
    //     foreach (var item in jsonArray)
    //     {
    //         var listOfTech = item["technologies"].AsArray().Select(x => x?.GetValue<string>()).ToArray();
    //         technologies.AddRange(listOfTech);
    //     }
    //     var flattenTech = technologies.Distinct();
    //     var insertedTags = (await _tagService.ListAsync(new TagFilter())).ToList();
    //     var tagsToInsert = flattenTech.Where(x => !insertedTags.Select(y => y.Name).Contains(x));
    //     foreach (var tag in tagsToInsert)
    //     {
    //         await _tagService.CreateAsync( new Tag 
    //             {
    //                 Name = tag,
    //                 DocumentationLink = "",
    //                 Type = TagType.Technology
    //
    //             }
    //         );
    //     }
    //
    //     if (insertedTags.FirstOrDefault(x => x.Name == "enterprise") == null)
    //     {
    //         await _tagService.CreateAsync( new Tag 
    //             {
    //                 Name = "enterprise",
    //                 DocumentationLink = "",
    //                 Type = TagType.Category
    //
    //             }
    //         );
    //     }
    //
    //     if (insertedTags.FirstOrDefault(x => x.Name == "enterprise") == null)
    //     {
    //         await _tagService.CreateAsync( new Tag 
    //             {
    //                 Name = "opensource",
    //                 DocumentationLink = "",
    //                 Type = TagType.Category
    //
    //             }
    //         );
    //     }
    //
    //     if (insertedTags.FirstOrDefault(x => x.Name == "enterprise") == null)
    //     {
    //         await _tagService.CreateAsync( new Tag 
    //             {
    //                 Name = "fun",
    //                 DocumentationLink = "",
    //                 Type = TagType.Category
    //
    //             }
    //         );
    //     }
    //     
    //     insertedTags = (await _tagService.ListAsync(new TagFilter())).ToList();
    //     
    //     //PROJECTS
    //     var insertedProject = (await _service.ListAsync(new ProjectFilter())).ToList();
    //     foreach (var item in jsonArray)
    //     {
    //         if(insertedProject?.Select(x => x.Name).Contains(item["name"].GetValue<string>()) ?? true)
    //             continue;
    //         var tagsToLink = item["technologies"]
    //             .AsArray()
    //             .Select(x => x?.GetValue<string>())
    //             .Where(x => !string.IsNullOrEmpty(x))
    //             .Select(x => insertedTags.Where(y => y.Name == x).Select(y => y.Id).FirstOrDefault())
    //             .ToList();
    //
    //         switch (item["categories"].GetValue<string>())
    //         {
    //             case "enterprise":
    //                 tagsToLink.Add( insertedTags.Where(y => y.Name == "enterprise").Select(y => y.Id).FirstOrDefault());
    //                 break;
    //             case "opensource":
    //                 tagsToLink.Add( insertedTags.Where(y => y.Name == "opensource").Select(y => y.Id).FirstOrDefault());
    //                 break;
    //             case "fun":
    //                 tagsToLink.Add( insertedTags.Where(y => y.Name == "fun").Select(y => y.Id).FirstOrDefault());
    //                 break;
    //             default:
    //                 break;
    //         }
    //         
    //         var project = new ProjectDto
    //         {
    //             Name = item["name"].GetValue<string>(),
    //             Order = item["order"].GetValue<int>(),
    //             Description = new List<LocalizedStringDto>()
    //             {
    //                 new() { Language = "EN", Value = item["description"].GetValue<string>() }
    //             },
    //             TagIdsToLink = tagsToLink
    //         };
    //         
    //         await CreateAsync(project);
    //     }
    //
    //     //WORK EXPERIENCES
    //     response = await _httpClient.GetAsync("https://strapi.fiveamtech.it/network-data-fts");
    //     response.EnsureSuccessStatusCode();
    //
    //     content = await response.Content.ReadAsStringAsync();
    //     jsonNode = JsonNode.Parse(content);
    //
    //     if (jsonNode is not JsonArray array) return 0;
    //
    //     var data = array.FirstOrDefault()?["direct"] as JsonArray;
    //
    //     var insertedWorkExperience = await _workExperienceService.ListAsync(new WorkExperienceFilter());
    //
    //     var i = 0;
    //     foreach (var item in data)
    //     {
    //         var company = item["name"].GetValue<string>();
    //         if(insertedWorkExperience.FirstOrDefault(x => x.Company == company) != null)
    //             continue;
    //         
    //         var startDate = DateTimeOffset.Parse(item["startDate"].GetValue<string>() + "-01T12:00:00.000Z");
    //
    //         var endDate = item["endDate"]?.GetValue<string?>() != null
    //             ? DateTimeOffset.Parse(item["endDate"].GetValue<string>() + "-01T12:00:00.000Z")
    //             : (DateTimeOffset?)null;
    //         
    //         var payload = new WorkExperience
    //         {
    //             Order = i,
    //             Company = company,
    //             Position = item["role"].GetValue<string>(),
    //             Description = [new LocalizedString { Language = "EN", Value = item["description"].GetValue<string>() }],
    //             StartDate = startDate,
    //             EndDate = endDate
    //         };
    //         
    //         var id = await _workExperienceService.CreateAsync(payload);
    //
    //         i++;
    //     }
    //
    //
    //     return 0;
    // }
}
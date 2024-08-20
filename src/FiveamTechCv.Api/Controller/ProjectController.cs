using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/project")]
public class ProjectController : BaseController<Project, ProjectFilter, ProjectDto>
{
    public ProjectController(IProjectService service) : base(service)
    {
        
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
}
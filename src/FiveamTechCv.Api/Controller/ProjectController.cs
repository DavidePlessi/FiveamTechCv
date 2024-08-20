using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/project")]
public class ProjectController : BaseController<Project, ProjectFilter>
{
    public ProjectController(IProjectService service) : base(service)
    {
        
    }
    
    [HttpPost]
    public override async Task<string> CreateAsync(Project entity)
    {
        var entityId = await base.CreateAsync(entity);
        var tagId = (entity.Tags ?? [])
            .Where(t => !string.IsNullOrEmpty(t.Id))
            .Select(tag => tag.Id ?? "")
            .ToArray();

        if (tagId.Length > 0)
        {
            await _service.CreateRelationAsync(
                entityId, 
                tagId, 
                typeof(Tag), 
                "HAS_TAG"
            );
        }

        return entityId;
    }

    [HttpPost("/{fromId}/relate-tag")]
    public async Task<int> CreateRelationAsync(
        string fromId, 
        string[] toId
    )
    {
        return await _service.CreateRelationAsync(
            fromId, 
            toId, 
            typeof(Tag), 
            "HAS_TAG"
        );
    }
}
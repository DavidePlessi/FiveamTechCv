using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/tag")]
[Authorize]
public class TagController : BaseController<Tag, TagFilter, TagDto>
{
    public TagController(ITagService service) : base(service)
    {
    }

    [HttpPost]
    public override async Task<string> CreateAsync(TagDto dto)
    {
        var entityId = await base.CreateAsync(dto);
        var projectIds = (dto.ProjectIdsToLink ?? [])
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();

        if (projectIds.Length > 0)
        {
            await _service.CreateRelationAsync(
                entityId, 
                projectIds, 
                typeof(Project), 
                Project.HAS_TAG,
                true
            );
        }

        return entityId;
    }

    [HttpPut("{id}")]
    public override async Task<Tag> UpdateAsync(string id, TagDto dto)
    {
        var existingEntity = await _service.GetByIdAsync(id);
        
        // Update Project Links
        if (dto.ProjectIdsToLink != null) 
        {
            if (existingEntity?.Projects != null && existingEntity.Projects.Any())
            {
                var existingProjectIds = existingEntity.Projects.Select(x => x.Id!).ToArray();
                await _service.DeleteRelationAsync(
                    id, 
                    existingProjectIds, 
                    typeof(Project), 
                    Project.HAS_TAG, 
                    true
                );
            }
             
            var projectIds = dto.ProjectIdsToLink.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            if (projectIds.Any())
            {
               await _service.CreateRelationAsync(
                   id, 
                   projectIds, 
                   typeof(Project), 
                   Project.HAS_TAG, 
                   true
               );
            }
        }
        
        return await base.UpdateAsync(id, dto);
    }


    [HttpPost("{fromId}/relate-project")]
    public async Task<int> CreateRelationAsync(
        string fromId, 
        string[] toId
    )
    {
        return await _service.CreateRelationAsync(
            fromId, 
            toId, 
            typeof(Tag),
            Project.HAS_TAG,
            true
        );
    }
}
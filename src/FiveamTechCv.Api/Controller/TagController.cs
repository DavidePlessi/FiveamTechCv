using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/tag")]
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
                typeof(Tag), 
                "HAS_TAG"
            );
        }

        return entityId;
    }


    [HttpPost("/{fromId}/relate-project")]
    public async Task<int> CreateRelationAsync(
        string fromId, 
        string[] toId
    )
    {
        return await _service.CreateRelationAsync(
            fromId, 
            toId, 
            typeof(Tag), 
            "HAS_TAG",
            true
        );
    }
}
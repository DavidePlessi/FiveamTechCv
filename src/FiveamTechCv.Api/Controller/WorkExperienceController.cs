using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/work-experience")]
public class WorkExperienceController : BaseController<WorkExperience, WorkExperienceFilter, WorkExperienceDto>
{
    public WorkExperienceController(IWorkExperienceService service) : base(service)
    {
    }

    [HttpPost]
    public override async Task<string> CreateAsync(WorkExperienceDto dto)
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
                WorkExperience.HAS_PROJECT
            );
        }

        return entityId;
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
            typeof(Project), 
            WorkExperience.HAS_PROJECT
        );
    }
}
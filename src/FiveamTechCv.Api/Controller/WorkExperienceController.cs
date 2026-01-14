using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/work-experience")]
[Authorize]
public class WorkExperienceController : BaseController<WorkExperience, WorkExperienceFilter, WorkExperienceDto>
{
    private readonly ILocalizedStringService _localizedStringService;

    public WorkExperienceController(
        IWorkExperienceService service,
        ILocalizedStringService localizedStringService
    ) : base(service)
    {
        _localizedStringService = localizedStringService;
    }
}
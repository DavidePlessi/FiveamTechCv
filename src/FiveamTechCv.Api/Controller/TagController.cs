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
}
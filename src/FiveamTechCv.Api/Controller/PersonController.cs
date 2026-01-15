using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/person")]
public class PersonController : BaseController<Person, PersonFilter, PersonDto>
{
    public PersonController(IPersonService service) : base(service)
    {
    }
}

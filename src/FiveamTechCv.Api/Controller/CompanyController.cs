using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/company")]
public class CompanyController : BaseController<Company, CompanyFilter, CompanyDto>
{
    public CompanyController(ICompanyService service) : base(service)
    {
    }
}

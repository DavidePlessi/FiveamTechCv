using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface ICompanyService : INodeService<Company, CompanyFilter>
{
    Task<string> CreateAsync(Entities.DTO.CompanyDto dto);
    Task<Company> UpdateAsync(string id, Entities.DTO.CompanyDto dto);
}

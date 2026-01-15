using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface IPersonService : INodeService<Person, PersonFilter>
{
    Task<string> CreateAsync(Entities.DTO.PersonDto dto);
    Task<Person> UpdateAsync(string id, Entities.DTO.PersonDto dto);
}

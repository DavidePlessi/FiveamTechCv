using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface IWorkExperienceService : INodeService<WorkExperience, WorkExperienceFilter>
{
    Task<string> CreateAsync(Entities.DTO.WorkExperienceDto dto);
    Task<WorkExperience> UpdateAsync(string id, Entities.DTO.WorkExperienceDto dto);
}
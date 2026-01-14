using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface IProjectService : INodeService<Project, ProjectFilter>
{
    Task<string> CreateAsync(Entities.DTO.ProjectDto dto);
    Task<Project> UpdateAsync(string id, Entities.DTO.ProjectDto dto);
}
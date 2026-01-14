using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface ITagService : INodeService<Tag, TagFilter>
{
    Task<string> CreateAsync(Entities.DTO.TagDto dto);
    Task<Tag> UpdateAsync(string id, Entities.DTO.TagDto dto);
}
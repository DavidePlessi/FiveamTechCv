using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface ITagService : INodeService<Tag, TagFilter>
{
}
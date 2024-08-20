using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Graph;

namespace FiveamTechCv.Core.Services;

public class TagService(GraphDriver driver) 
    : BaseService<Entities.Nodes.Tag, TagFilter>(driver), ITagService;
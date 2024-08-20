using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;

namespace FiveamTechCv.Core.Services;

public class ProjectService(GraphDriver driver) 
    : BaseService<Project, ProjectFilter>(driver), IProjectService
{
}
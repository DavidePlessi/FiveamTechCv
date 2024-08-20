using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;

namespace FiveamTechCv.Core.Services;

public class WorkExperienceService(GraphDriver driver)
    : BaseService<WorkExperience, WorkExperienceFilter>(driver), IWorkExperienceService;
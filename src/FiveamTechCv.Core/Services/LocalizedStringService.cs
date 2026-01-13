using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;

namespace FiveamTechCv.Core.Services;

public class LocalizedStringService(GraphDriver driver) 
    : BaseService<LocalizedString, LocalizedStringFilter>(driver), ILocalizedStringService
{
}
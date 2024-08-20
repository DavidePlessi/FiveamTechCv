using FiveamTechCv.Entities.Extensions;

namespace FiveamTechCv.Entities.Filters;

public abstract class BaseNodeFilter : INodeFilter
{
    public virtual string ToCypherCondition(string alias = "n")
    {
        return this.ConvertToCondition(alias);
    }

    public virtual object ToCypherParams()
    {
        return this.ConvertToParameters();
    }
}
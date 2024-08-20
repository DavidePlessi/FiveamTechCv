namespace FiveamTechCv.Entities.Filters;

public interface INodeFilter
{
    public string ToCypherCondition(string alias = "n");
    public object ToCypherParams();
}
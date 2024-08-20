using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Filters;

namespace FiveamTechCv.Abstract.Services;

public interface INodeService<T, TFilter>
    where T : BaseNode
    where TFilter : INodeFilter
{
    Task<string> CreateAsync(T node);
    Task<T> UpdateAsync(T node);
    Task<bool> DeleteAsync(string id);
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> ListAsync(TFilter filter);
    
    Task<int> CreateRelationAsync(
        string fromId, 
        string[] toId, 
        Type toType,
        string relation,
        bool relationIncoming = false
    );
}
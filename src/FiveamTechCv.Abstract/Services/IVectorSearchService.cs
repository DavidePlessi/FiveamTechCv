using FiveamTechCv.Entities;

namespace FiveamTechCv.Abstract.Services;

public interface IVectorSearchService
{
    Task<List<BaseNode>> SearchAsync(string query, int k = 5);
    Task CreateIndexAsync();
}

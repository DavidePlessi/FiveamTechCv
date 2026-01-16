using FiveamTechCv.Entities;
using FiveamTechCv.Entities.DTO;

namespace FiveamTechCv.Abstract.Services;

public interface IVectorSearchService
{
    Task<List<SearchResultDto>> SearchAsync(string query, int k = 5);
    Task CreateIndexAsync();
    Task RecalculateEmbeddingsAsync();
}

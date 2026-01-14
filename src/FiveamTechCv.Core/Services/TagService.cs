using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Graph;

using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Core.Services;

public class TagService(GraphDriver driver, IServiceProvider serviceProvider) 
    : BaseService<Entities.Nodes.Tag, TagFilter>(driver, serviceProvider), ITagService
{
    public async Task<string> CreateAsync(TagDto dto)
    {
        var entityId = await base.CreateAsync(dto.ToEntity());
        var projectIds = (dto.ProjectIdsToLink ?? [])
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();

        if (projectIds.Length > 0)
        {
            await CreateRelationAsync(
                entityId, 
                projectIds, 
                typeof(Project), 
                Entities.Nodes.Project.HAS_TAG,
                true
            );
        }

        return entityId;
    }

    public async Task<Entities.Nodes.Tag> UpdateAsync(string id, TagDto dto)
    {
        var existingEntity = await GetByIdAsync(id);
        
        // Update Project Links
        if (dto.ProjectIdsToLink != null) 
        {
            if (existingEntity?.Projects != null && existingEntity.Projects.Any())
            {
                var existingProjectIds = existingEntity.Projects.Select(x => x.Id!).ToArray();
                await DeleteRelationAsync(
                    id, 
                    existingProjectIds, 
                    typeof(Project), 
                    Entities.Nodes.Project.HAS_TAG, 
                    true
                );
            }
             
            var projectIds = dto.ProjectIdsToLink.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            if (projectIds.Any())
            {
               await CreateRelationAsync(
                   id, 
                   projectIds, 
                   typeof(Project), 
                   Entities.Nodes.Project.HAS_TAG, 
                   true
               );
            }
        }
        
        var entity = dto.ToEntity();
        entity.Id = id;
        return await base.UpdateAsync(entity);
    }
}
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;

namespace FiveamTechCv.Core.Services;

public class ProjectService(
    GraphDriver driver, 
    ILocalizedStringService localizedStringService,
    IServiceProvider serviceProvider) 
    : BaseService<Project, ProjectFilter>(driver, serviceProvider), IProjectService
{
    public async Task<string> CreateAsync(ProjectDto dto)
    {
        var entityId = await base.CreateAsync(dto.ToEntity());
        
        // Tags
        var tagIds = (dto.TagIdsToLink ?? [])
            .Where(t => !string.IsNullOrEmpty(t))
            .ToArray();

        if (tagIds.Length > 0)
        {
            await CreateRelationAsync(
                entityId, 
                tagIds, 
                typeof(FiveamTechCv.Entities.Nodes.Tag), 
                Project.HAS_TAG
            );
        }
        
        // Description
        if (dto.Description != null)
        {
            foreach (var description in dto.Description)
            {
                var descriptionId = await localizedStringService.CreateAsync(new LocalizedString
                {
                    Language = description.Language,
                    Value = description.Value
                });
                
                await CreateRelationAsync(
                    entityId, 
                    [descriptionId], 
                    typeof(LocalizedString), 
                    Project.HAS_DESCRIPTION
                );
            }
        }

        return entityId;
    }

    public async Task<Project> UpdateAsync(string id, ProjectDto dto)
    {
        var existingEntity = await GetByIdAsync(id);
        
        // Update Tags
        if (dto.TagIdsToLink != null) 
        {
            if (existingEntity?.Tags != null && existingEntity.Tags.Any())
            {
                var existingTagIds = existingEntity.Tags.Select(x => x.Id!).ToArray();
                await DeleteRelationAsync(id, existingTagIds, typeof(FiveamTechCv.Entities.Nodes.Tag), Project.HAS_TAG);
            }
             
            var tagIds = dto.TagIdsToLink.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            if (tagIds.Any())
            {
               await CreateRelationAsync(id, tagIds, typeof(FiveamTechCv.Entities.Nodes.Tag), Project.HAS_TAG);
            }
        }

        // Update Description
        if (dto.Description != null)
        {
             if (existingEntity?.Description != null)
             {
                 foreach (var desc in existingEntity.Description)
                 {
                     if (desc.Id != null) await localizedStringService.DeleteAsync(desc.Id);
                 }
             }

             // Create new ones
             foreach (var description in dto.Description)
             {
                var descriptionId = await localizedStringService.CreateAsync(new LocalizedString
                {
                    Language = description.Language,
                    Value = description.Value
                });
                
                await CreateRelationAsync(
                    id, 
                    [descriptionId], 
                    typeof(LocalizedString), 
                    Project.HAS_DESCRIPTION
                );
             }
        }

        var entity = dto.ToEntity();
        entity.Id = id;
        return await base.UpdateAsync(entity);
    }
}
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;
using Tag = FiveamTechCv.Entities.Nodes.Tag;

namespace FiveamTechCv.Core.Services;

public class PersonService(
    GraphDriver driver,
    ILocalizedStringService localizedStringService,
    IServiceProvider serviceProvider)
    : BaseService<Person, PersonFilter>(driver, serviceProvider), IPersonService
{
    public async Task<string> CreateAsync(PersonDto dto)
    {
        var entityId = await base.CreateAsync(dto.ToEntity());
        
        await HandleRelations(entityId, dto);

        return entityId;
    }

    public async Task<Person> UpdateAsync(string id, PersonDto dto)
    {
        var existingEntity = await GetByIdAsync(id);
        
        // Clear existing complex relations to be rebuilt (simplification, can be optimized)
        if (existingEntity != null)
        {
            await ClearRelations(id, existingEntity);
        }

        await HandleRelations(id, dto);

        var entity = dto.ToEntity();
        entity.Id = id;
        return await base.UpdateAsync(entity);
    }

    private async Task HandleRelations(string entityId, PersonDto dto)
    {
        // Tags
        var tagIds = (dto.TagIdsToLink ?? []).Where(t => !string.IsNullOrEmpty(t)).ToArray();
        if (tagIds.Length > 0)
        {
            await CreateRelationAsync(entityId, tagIds, typeof(Tag), Person.HAS_TAG);
        }

        // Projects
        var projectIds = (dto.ProjectIdsToLink ?? []).Where(t => !string.IsNullOrEmpty(t)).ToArray();
        if (projectIds.Length > 0)
        {
            await CreateRelationAsync(entityId, projectIds, typeof(Project), Person.HAS_PROJECT);
        }

        // WorkExperiences
        var workExpIds = (dto.WorkExperienceIdsToLink ?? []).Where(t => !string.IsNullOrEmpty(t)).ToArray();
        if (workExpIds.Length > 0)
        {
            await CreateRelationAsync(entityId, workExpIds, typeof(WorkExperience), Person.HAS_WORK_EXPERIENCE);
        }
        
        // Localized Strings Helpers
        async Task HandleLocalizedString(List<LocalizedStringDto>? items, string relation)
        {
             if (items != null)
            {
                foreach (var item in items)
                {
                    var itemId = await localizedStringService.CreateAsync(new LocalizedString
                    {
                        Language = item.Language,
                        Value = item.Value
                    });
                    
                    await CreateRelationAsync(entityId, [itemId], typeof(LocalizedString), relation);
                }
            }
        }

        await HandleLocalizedString(dto.Info, Person.HAS_INFO);
        await HandleLocalizedString(dto.Summary, Person.HAS_SUMMARY);
        await HandleLocalizedString(dto.Mindset, Person.HAS_MINDSET);
        await HandleLocalizedString(dto.Slogan, Person.HAS_SLOGAN);
    }
    
    private async Task ClearRelations(string id, Person entity)
    {
        // Tags
        if (entity.Tags?.Any() == true)
        {
            await DeleteRelationAsync(id, entity.Tags.Select(x => x.Id!).ToArray(), typeof(Tag), Person.HAS_TAG);
        }
        
        // Projects
        if (entity.Projects?.Any() == true)
        {
             await DeleteRelationAsync(id, entity.Projects.Select(x => x.Id!).ToArray(), typeof(Project), Person.HAS_PROJECT);
        }
        
        // WorkExperiences
        if (entity.WorkExperiences?.Any() == true)
        {
             await DeleteRelationAsync(id, entity.WorkExperiences.Select(x => x.Id!).ToArray(), typeof(WorkExperience), Person.HAS_WORK_EXPERIENCE);
        }
        
        // Helper for localized strings
         async Task ClearLocalizedString(List<LocalizedString>? items, string relation)
         {
             if (items?.Any() == true)
             {
                 foreach(var item in items)
                 {
                     if(item.Id != null) await localizedStringService.DeleteAsync(item.Id);
                 }
             }
         }
         
         await ClearLocalizedString(entity.Info, Person.HAS_INFO);
         await ClearLocalizedString(entity.Summary, Person.HAS_SUMMARY);
         await ClearLocalizedString(entity.Mindset, Person.HAS_MINDSET);
         await ClearLocalizedString(entity.Slogan, Person.HAS_SLOGAN);
    }
}

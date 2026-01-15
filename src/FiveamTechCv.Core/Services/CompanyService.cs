using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;

namespace FiveamTechCv.Core.Services;

public class CompanyService(
    GraphDriver driver,
    ILocalizedStringService localizedStringService,
    IServiceProvider serviceProvider)
    : BaseService<Company, CompanyFilter>(driver, serviceProvider), ICompanyService
{
    public async Task<string> CreateAsync(CompanyDto dto)
    {
        var entityId = await base.CreateAsync(dto.ToEntity());
        
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
                    Company.HAS_DESCRIPTION
                );
            }
        }

        return entityId;
    }

    public async Task<Company> UpdateAsync(string id, CompanyDto dto)
    {
        var existingEntity = await GetByIdAsync(id);
        
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
                    Company.HAS_DESCRIPTION
                );
             }
        }
        
        var entity = dto.ToEntity();
        entity.Id = id;
        return await base.UpdateAsync(entity);
    }
}

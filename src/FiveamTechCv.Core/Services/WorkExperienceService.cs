using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.DTO;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Neo4j.Driver;

namespace FiveamTechCv.Core.Services;

public class WorkExperienceService(
    GraphDriver driver,
    ILocalizedStringService localizedStringService,
    IServiceProvider serviceProvider)
    : BaseService<WorkExperience, WorkExperienceFilter>(driver, serviceProvider), IWorkExperienceService
{
    // public async Task<string> CreateAsync(WorkExperienceDto dto)
    // {
    //     var entityId = await base.CreateAsync(dto.ToEntity());
    //     
    //     // Projects
    //     var projectIds = (dto.ProjectIdsToLink ?? [])
    //         .Where(p => !string.IsNullOrEmpty(p))
    //         .ToArray();
    //
    //     if (projectIds.Length > 0)
    //     {
    //         await CreateRelationAsync(
    //             entityId, 
    //             projectIds, 
    //             typeof(Project), 
    //             WorkExperience.HAS_PROJECT
    //         );
    //     }
    //     
    //     // Tags
    //     var tagIds = (dto.TagIdsToLink ?? [])
    //         .Where(t => !string.IsNullOrEmpty(t))
    //         .ToArray();
    //     
    //     if (tagIds.Length > 0)
    //     {
    //         await CreateRelationAsync(
    //             entityId, 
    //             tagIds, 
    //             typeof(FiveamTechCv.Entities.Nodes.Tag), 
    //             WorkExperience.HAS_TAG
    //         );
    //     }
    //     
    //     // CompanyLink
    //     var firstCompanyLinkId = dto.CompanyIdToLink?.FirstOrDefault();
    //     if (!string.IsNullOrEmpty(firstCompanyLinkId))
    //     {
    //         await CreateRelationAsync(
    //             entityId, 
    //             [firstCompanyLinkId], 
    //             typeof(Company), 
    //             WorkExperience.HAS_COMPANY
    //         );
    //     }
    //     
    //     // Description
    //     if (dto.Description != null)
    //     {
    //         foreach (var description in dto.Description)
    //         {
    //             var descriptionId = await localizedStringService.CreateAsync(new LocalizedString
    //             {
    //                 Language = description.Language,
    //                 Value = description.Value
    //             });
    //             
    //             await CreateRelationAsync(
    //                 entityId, 
    //                 [descriptionId], 
    //                 typeof(LocalizedString), 
    //                 WorkExperience.HAS_DESCRIPTION
    //             );
    //         }
    //     }
    //
    //     return entityId;
    // }
    //
    // public async Task<WorkExperience> UpdateAsync(string id, WorkExperienceDto dto)
    // {
    //     var existingEntity = await GetByIdAsync(id);
    //     
    //     // Update Project Links
    //     if (dto.ProjectIdsToLink != null) 
    //     {
    //         if (existingEntity?.Projects != null && existingEntity.Projects.Any())
    //         {
    //             var existingIds = existingEntity.Projects.Select(x => x.Id!).ToArray();
    //             await DeleteRelationAsync(
    //                 id, 
    //                 existingIds, 
    //                 typeof(Project), 
    //                 WorkExperience.HAS_PROJECT, 
    //                 true
    //             );
    //         }
    //          
    //         var newIds = dto.ProjectIdsToLink.Where(t => !string.IsNullOrEmpty(t)).ToArray();
    //         if (newIds.Any())
    //         {
    //            await CreateRelationAsync(
    //                id, 
    //                newIds, 
    //                typeof(Project), 
    //                WorkExperience.HAS_PROJECT, 
    //                true
    //            );
    //         }
    //     }
    //     
    //     // Update Tag Links
    //     if (dto.TagIdsToLink != null) 
    //     {
    //         if (existingEntity?.Tags != null && existingEntity.Tags.Any())
    //         {
    //             var existingIds = existingEntity.Tags.Select(x => x.Id!).ToArray();
    //             await DeleteRelationAsync(
    //                 id, 
    //                 existingIds, 
    //                 typeof(FiveamTechCv.Entities.Nodes.Tag), 
    //                 WorkExperience.HAS_TAG, 
    //                 true
    //             );
    //         }
    //          
    //         var newIds = dto.TagIdsToLink.Where(t => !string.IsNullOrEmpty(t)).ToArray();
    //         if (newIds.Any())
    //         {
    //            await CreateRelationAsync(
    //                id, 
    //                newIds, 
    //                typeof(FiveamTechCv.Entities.Nodes.Tag), 
    //                WorkExperience.HAS_TAG, 
    //                true
    //            );
    //         }
    //     }
    //     
    //     // Update Company Link
    //     if (dto.CompanyIdToLink != null) 
    //     {
    //         var firstExistingEntity = existingEntity?.Company?.FirstOrDefault();
    //         if (firstExistingEntity != null)
    //         {
    //             var existingId = firstExistingEntity.Id!;
    //             await DeleteRelationAsync(
    //                 id, 
    //                 [existingId], 
    //                 typeof(Company), 
    //                 WorkExperience.HAS_COMPANY, 
    //                 false 
    //             );
    //         }
    //         var firstCompanyLinkId = dto.CompanyIdToLink?.FirstOrDefault();
    //          
    //         if (!string.IsNullOrEmpty(firstCompanyLinkId))
    //         {
    //            await CreateRelationAsync(
    //                id, 
    //                [firstCompanyLinkId], 
    //                typeof(Company), 
    //                WorkExperience.HAS_COMPANY, 
    //                false
    //            );
    //         }
    //     }
    //     
    //     // Update Description
    //     if (dto.Description != null)
    //     {
    //          if (existingEntity?.Description != null)
    //          {
    //              foreach (var desc in existingEntity.Description)
    //              {
    //                  if (desc.Id != null) await localizedStringService.DeleteAsync(desc.Id);
    //              }
    //          }
    //
    //          // Create new ones
    //          foreach (var description in dto.Description)
    //          {
    //             var descriptionId = await localizedStringService.CreateAsync(new LocalizedString
    //             {
    //                 Language = description.Language,
    //                 Value = description.Value
    //             });
    //             
    //             await CreateRelationAsync(
    //                 id, 
    //                 [descriptionId], 
    //                 typeof(LocalizedString), 
    //                 WorkExperience.HAS_DESCRIPTION
    //             );
    //          }
    //     }
    //     
    //     var entity = dto.ToEntity();
    //     entity.Id = id;
    //     return await base.UpdateAsync(entity);
    // }
}
import {BaseEntityService} from "~/services/BaseEntityService";
import type {IProject, IProjectDto, IProjectFilter, ITag, ITagDto, ITagFilter} from "~/entities/entities";

export class ProjectService 
  extends BaseEntityService<IProject, IProjectFilter, IProjectDto> 
{
    constructor() {
        super('project');
    }
}
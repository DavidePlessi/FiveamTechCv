import {BaseEntityService} from "~/services/BaseEntityService";
import type {Project, ProjectDto, ProjectFilter, Tag, TagDto, ITagFilter} from "~/entities/entities";

export class ProjectService 
  extends BaseEntityService<Project, ProjectFilter, ProjectDto> 
{
    constructor() {
        super('project');
    }
}
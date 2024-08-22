import type {IWorkExperience, IWorkExperienceDto, IWorkExperienceFilter} from "~/entities/entities";
import {BaseEntityService} from "~/services/BaseEntityService";

export class WorkExperienceService extends BaseEntityService<IWorkExperience, IWorkExperienceFilter, IWorkExperienceDto> {
    constructor() {
        super('work-experience');
    }
}
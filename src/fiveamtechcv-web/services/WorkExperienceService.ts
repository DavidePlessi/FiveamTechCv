import type {WorkExperience, WorkExperienceDto, WorkExperienceFilter} from "~/entities/entities";
import {BaseEntityService} from "~/services/BaseEntityService";

export class WorkExperienceService extends BaseEntityService<WorkExperience, WorkExperienceFilter, WorkExperienceDto> {
    constructor() {
        super('work-experience');
    }
}
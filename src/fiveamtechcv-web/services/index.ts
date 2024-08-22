import {ProjectService} from "~/services/ProjectService";
import {TagService} from "~/services/TagService";
import {WorkExperienceService} from "~/services/WorkExperienceService";
import {UserService} from "~/services/UserService";

export const userService = new UserService();
export const projectService = new ProjectService();
export const tagService = new TagService();
export const workExperienceService = new WorkExperienceService();
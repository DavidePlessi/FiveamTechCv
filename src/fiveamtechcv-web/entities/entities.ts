export enum TagType
{
    Framework,
    Language,
    Technology,
    Library,
    Database,
    Platform,
    Area
}

export class BaseEntity {
    id?: string;
    updatedAt?: number|Date;
}
export class BaseEntityFilter {}
export class BaseEntityDto {}

export class Tag extends BaseEntity {
    name?: string;
    type?: TagType;
    documentationLink?: string;
    projects?: Project[];    
}
export class TagFilter extends BaseEntityFilter {}
export class TagDto extends BaseEntityDto {}

export class Project extends BaseEntity {
    name?: string;
    description?: string;
    tags?: Tag[];

}
export class ProjectFilter extends BaseEntityFilter {}
export class ProjectDto extends BaseEntityDto {}

export class WorkExperience extends BaseEntity {
    company?: string;
    position?: string;
    description?: string;
    startDate?: number|Date;
    endDate?: number|Date;
    projects?: Project[];
}
export class WorkExperienceFilter extends BaseEntityFilter {}
export class WorkExperienceDto extends BaseEntityDto {}

export class IUser extends BaseEntity {
    username?: string;
}


export class CreateUser {
    username: string;
    password: string;
    isAdmin: boolean;
}

export class LoginUser {
    username: string;
    password: string;
}

export class QueryResult {
    tags?: Tag[];
    projects?: Project[];
    workExperiences?: WorkExperience[];
}


const typeDescriptions = {
    "tagDto": [{}]
}


export function dtoToEntity<T extends BaseEntity>(dto: BaseEntityDto): T {
    return dto as T;
}

export function entityToDto<T extends BaseEntityDto>(entity: BaseEntity): T {
    return entity as T;
}

export function getPropertyTypes<T>(obj: T): (keyof T)[] {
    return Object.keys(obj) as (keyof T)[];
}
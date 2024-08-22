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

export interface IBaseEntity {
    id?: string;
    updatedAt?: number;
}
export interface IBaseEntityFilter {}
export interface IBaseEntityDto {}

export interface ITag extends IBaseEntity {
    name?: string;
    type?: TagType;
    documentationLink?: string;
    projects?: IProject[];    
}
export interface ITagFilter extends IBaseEntityFilter {}
export interface ITagDto extends IBaseEntityDto {}

export interface IProject extends IBaseEntity {
    name?: string;
    description?: string;
    tags?: ITag[];

}
export interface IProjectFilter extends IBaseEntityFilter {}
export interface IProjectDto extends IBaseEntityDto {}

export interface IWorkExperience extends IBaseEntity {
    company?: string;
    position?: string;
    description?: string;
    startDate?: number;
    endDate?: number;
    projects?: IProject[];
}
export interface IWorkExperienceFilter extends IBaseEntityFilter {}
export interface IWorkExperienceDto extends IBaseEntityDto {}

export interface IUser extends IBaseEntity {
    username?: string;
}


export interface ICreateUser {
    username: string;
    password: string;
    isAdmin: boolean;
}

export interface ILoginUser {
    username: string;
    password: string;
}

export interface IQueryResult {
    tags?: ITag[];
    projects?: IProject[];
    workExperiences?: IWorkExperience[];
}
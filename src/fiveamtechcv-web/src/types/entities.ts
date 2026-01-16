export enum TagType {
    Framework = 0,
    Language = 1,
    Technology = 2,
    Library = 3,
    Database = 4,
    Platform = 5,
    Area = 6,
    Category = 7,
    Role=8
}

export interface BaseEntity {
    id?: string;
}

export interface LocalizedString extends BaseEntity {
    language?: string;
    value?: string;
}

export interface Tag extends BaseEntity {
    name?: string;
    type?: TagType;
    documentationLink?: string;
    projectIdsToLink?: string[];
    projects?: Project[];
    order?: number;
}

export interface Project extends BaseEntity {
    name?: string;
    description?: LocalizedString[];
    tags?: Tag[];
    people?: Person[];
    tagIdsToLink?: string[];
    personIdsToLink?: string[];
    order?: number;
}

export interface Company extends BaseEntity {
    name?: string;
    website?: string;
    description?: LocalizedString[];
}

export interface WorkExperience extends BaseEntity {
    company?: Company;
    companyIdToLink?: string;

    position?: string;
    startDate?: string;
    endDate?: string;
    description?: LocalizedString[];
    projects?: Project[];
    tags?: Tag[];
    person?: Person;
    projectIdsToLink?: string[];
    tagIdsToLink?: string[];
    personIdToLink?: string;
    order?: number;
}

export interface Person extends BaseEntity {
    name?: string;
    lastName?: string;
    bornDate?: string;
    info?: LocalizedString[];
    summary?: LocalizedString[];
    mindset?: LocalizedString[];
    slogan?: LocalizedString[];
    projects?: Project[];
    workExperiences?: WorkExperience[];
    tags?: Tag[];
    projectIdsToLink?: string[];
    workExperienceIdsToLink?: string[];
    tagIdsToLink?: string[];
}

// Form Schema Types
export type FieldType = 'text' | 'textarea' | 'select' | 'date' | 'array' | 'object-array' | 'autocomplete' | 'number';

export interface FormField {
    key: string;
    label: string;
    type: FieldType;
    required?: boolean;
    options?: any[]; // For select
    itemSchema?: FormSchema; // For array/object-array
    itemLabelKey?: string; // For array of strings/objects to display
    multiple?: boolean;
}

export interface FormSchema {
    fields: FormField[];
}

export interface LoginModel {
    username?: string;
    password?: string;
}

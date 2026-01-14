export enum TagType {
    Framework = 0,
    Language = 1,
    Technology = 2,
    Library = 3,
    Database = 4,
    Platform = 5,
    Area = 6,
    Category = 7
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
    name: string;
    description?: LocalizedString[];
    tagIdsToLink?: string[];
    tags?: Tag[];
    order?: number;
}

export interface WorkExperience extends BaseEntity {
    company?: string;
    position?: string;
    description?: LocalizedString[];
    startDate?: string;
    endDate?: string;
    projectIdsToLink?: string[];
    projects?: Project[];
    tagIdsToLink?: string[];
    tags?: Tag[];
    order?: number;
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

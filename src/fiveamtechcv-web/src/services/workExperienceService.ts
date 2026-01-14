import httpClient from './httpClient';
import type { WorkExperience } from '@/types/entities';

export const workExperienceService = {
    async getAll(): Promise<WorkExperience[]> {
        const response = await httpClient.get<WorkExperience[]>('/api/work-experience');
        return response.data;
    },

    async getById(id: string): Promise<WorkExperience> {
        const response = await httpClient.get<WorkExperience>(`/api/work-experience/${id}`);
        return response.data;
    },

    async create(item: Partial<WorkExperience>): Promise<string> {
        const response = await httpClient.post<string>('/api/work-experience', item);
        return response.data;
    },

    async update(id: string, item: Partial<WorkExperience>): Promise<WorkExperience> {
        const response = await httpClient.put<WorkExperience>(`/api/work-experience/${id}`, item);
        return response.data;
    },

    async delete(id: string): Promise<void> {
        await httpClient.delete(`/api/work-experience/${id}`);
    }
};

import httpClient from './httpClient';
import type { Project } from '@/types/entities';

export const projectService = {
    async getAll(name?: string): Promise<Project[]> {
        const response = await httpClient.get<Project[]>('/api/project', { params: { Name: name } });
        return response.data;
    },

    async getById(id: string): Promise<Project> {
        const response = await httpClient.get<Project>(`/api/project/${id}`);
        return response.data;
    },

    async create(project: Partial<Project>): Promise<void> {
        await httpClient.post('/api/project', project);
    },

    async update(id: string, project: Partial<Project>): Promise<Project> {
        const response = await httpClient.put<Project>(`/api/project/${id}`, project);
        return response.data;
    },

    async delete(id: string): Promise<void> {
        await httpClient.delete(`/api/project/${id}`);
    }
};

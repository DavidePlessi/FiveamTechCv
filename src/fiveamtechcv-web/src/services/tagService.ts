import httpClient from './httpClient';
import type { Tag, TagType } from '@/types/entities';

export const tagService = {
    async getAll(name?: string, type?: TagType): Promise<Tag[]> {
        const response = await httpClient.get<Tag[]>('/api/tag', { params: { Name: name, Type: type } });
        return response.data;
    },

    async getById(id: string): Promise<Tag> {
        const response = await httpClient.get<Tag>(`/api/tag/${id}`);
        return response.data;
    },

    async create(tag: Partial<Tag>): Promise<void> {
        await httpClient.post('/api/tag', tag);
    },

    async update(id: string, tag: Partial<Tag>): Promise<Tag> {
        const response = await httpClient.put<Tag>(`/api/tag/${id}`, tag);
        return response.data;
    },

    async delete(id: string): Promise<void> {
        await httpClient.delete(`/api/tag/${id}`);
    },

    async relateProject(tagId: string, projectIds: string[]): Promise<void> {
        await httpClient.post(`/api/tag/${tagId}/relate-project`, projectIds);
    }
};

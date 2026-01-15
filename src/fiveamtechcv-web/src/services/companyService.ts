import httpClient from './httpClient';
import type { Company } from '@/types/entities';

export const companyService = {
    async getAll(): Promise<Company[]> {
        const response = await httpClient.get<Company[]>('/api/company');
        return response.data;
    },

    async getById(id: string): Promise<Company> {
        const response = await httpClient.get<Company>(`/api/company/${id}`);
        return response.data;
    },

    async create(company: Partial<Company>): Promise<string> {
        const response = await httpClient.post<string>('/api/company', company);
        return response.data;
    },

    async update(id: string, company: Partial<Company>): Promise<Company> {
        const response = await httpClient.put<Company>(`/api/company/${id}`, company);
        return response.data;
    },

    async delete(id: string): Promise<void> {
        await httpClient.delete(`/api/company/${id}`);
    }
};

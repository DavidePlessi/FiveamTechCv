import httpClient from './httpClient';
import type { WorkExperience } from '@/types/entities';

function fromServerWorkExperience(x: any) {
  return {
    ...x,
    company: x.companies?.length > 0 ? x.companies[0] : null,
    person: x.people?.length > 0 ? x.people[0] : null
  } as WorkExperience;
}

function toServerWorkExperience(x: WorkExperience) {
  return {
    ...x,
    companyIdToLink: x.companyIdToLink ? [x.companyIdToLink] : null,
    personIdToLink: x.personIdToLink ? [x.personIdToLink] : null
  };
}

export const workExperienceService = {
  async getAll(): Promise<WorkExperience[]> {
    const response = await httpClient.get('/api/work-experience');
    const data = response.data.map(fromServerWorkExperience);
    return data as WorkExperience[];
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
    const response = await httpClient.put(
      `/api/work-experience/${id}`,
      toServerWorkExperience(item)
    );
    return fromServerWorkExperience(response.data);
  },

  async delete(id: string): Promise<void> {
    await httpClient.delete(`/api/work-experience/${id}`);
  }
};

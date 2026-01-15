import type { Person } from '@/types/entities';
import api from './httpClient';

export const personService = {
    async getPeople(): Promise<Person[]> {
        const query = `
      query {
        people {
          name
          lastName
          bornDate
          info {
            language
            value
          }
          summary {
            language
            value
          }
          mindset {
            language
            value
          }
          slogan {
            language
            value
          }
          projects {
            title
            headline
          }
          workExperiences {
            company
            position
            startDate
             endDate
          }
          tags {
            name
            type
          }
        }
      }
    `;

        const response = await api.post('', { query });
        return response.data.data.people;
    },

    async getAll(name?: string): Promise<Person[]> {
        const response = await api.get<Person[]>('/api/person', { params: { Name: name } });
        return response.data;
    },

    async getById(id: string): Promise<Person> {
        const response = await api.get<Person>(`/api/person/${id}`);
        return response.data;
    },

    async create(person: Partial<Person>): Promise<void> {
        await api.post('/api/person', person);
    },

    async update(id: string, person: Partial<Person>): Promise<Person> {
        const response = await api.put<Person>(`/api/person/${id}`, person);
        return response.data;
    },

    async delete(id: string): Promise<void> {
        await api.delete(`/api/person/${id}`);
    }
};

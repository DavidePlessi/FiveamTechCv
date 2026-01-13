import httpClient from './httpClient';
import type { LoginModel } from '@/types/entities';

export const authService = {
    async login(credentials: LoginModel): Promise<string> {
        const response = await httpClient.post<string>('/api/user/login', credentials);
        return response.data;
    },

    async validateToken(): Promise<boolean> {
        try {
            await httpClient.post('/api/user/validate-token');
            return true;
        } catch {
            return false;
        }
    }
};

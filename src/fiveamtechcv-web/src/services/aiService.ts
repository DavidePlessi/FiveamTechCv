import httpClient from './httpClient';

export const aiService = {
    async ask(question: string): Promise<string> {
        const response = await httpClient.post('/api/ai/ask', { question });
        return response.data.answer;
    }
};

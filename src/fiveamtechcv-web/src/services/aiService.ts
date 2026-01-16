import httpClient from './httpClient';

export const aiService = {
    async ask(question: string, history: string[] = [], recaptchaToken: string): Promise<string> {
        const response = await httpClient.post('/api/ai/ask', { question, history }, {
            headers: {
                'x-recaptcha-token': recaptchaToken
            }
        });
        return response.data.answer;
    }
};

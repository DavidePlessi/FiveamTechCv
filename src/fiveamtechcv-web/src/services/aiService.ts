import httpClient from './httpClient';

export const aiService = {
    async ask(question: string, history: {role: string, text: string}[] = [], sessionId: string, recaptchaToken: string): Promise<string> {
        const response = await httpClient.post('/api/ai/ask', { question, history, sessionId }, {
            headers: {
                'x-recaptcha-token': recaptchaToken
            }
        });
        return response.data.answer;
    }
};

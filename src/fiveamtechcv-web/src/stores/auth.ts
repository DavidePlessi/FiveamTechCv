import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { authService } from '@/services/authService';

export const useAuthStore = defineStore('auth', () => {
    const router = useRouter();
    const isAuthenticated = ref(false);
    const user = ref<string | null>(null);

    // Initialize from local storage if needed
    if (localStorage.getItem('auth_token')) {
        isAuthenticated.value = true;
        // Ideally decode token or fetch profile, but for now assumption is fine
        user.value = 'Admin';
    }

    async function login(username: string, password: string): Promise<boolean> {
        try {
            const token = await authService.login({ username, password });
            isAuthenticated.value = true;
            user.value = username;
            localStorage.setItem('auth_token', token);
            return true;
        } catch (error) {
            console.error('Login failed', error);
            return false;
        }
    }

    function logout() {
        isAuthenticated.value = false;
        user.value = null;
        localStorage.removeItem('auth_token');
        router.push('/login');
    }

    return { isAuthenticated, user, login, logout };
});

import { defineStore } from 'pinia';
import { ref } from 'vue';
import { personService } from '@/services/personService';
import type { Person } from '@/types/entities';

export const useContextStore = defineStore('context', () => {
    const selectedPersonId = ref<string | null>(null);
    const people = ref<Person[]>([]);
    const loading = ref(false);

    async function fetchPeople() {
        if (people.value.length > 0) return;

        loading.value = true;
        try {
            people.value = await personService.getAll();
        } catch (error) {
            console.error('Failed to fetch people for context', error);
        } finally {
            loading.value = false;
        }
    }

    function setSelectedPerson(id: string | null) {
        selectedPersonId.value = id;
    }

    return {
        selectedPersonId,
        people,
        loading,
        fetchPeople,
        setSelectedPerson
    };
});

<template>
  <v-text-field
      v-model="stringDate"
      v-bind="{ ...$attrs }"
      @update:modelValue="onInput"
      placeholder="DD/MM/YYYY"
      persistent-placeholder
      ref="dateFieldRef"
      class="cyber-input"
      variant="outlined"
      density="comfortable"
  >
    <template v-slot:append-inner>
      <v-menu
          v-model="menu"
          transition="scale-transition"
          offset-y
          min-width="290px"
          :close-on-content-click="false"
      >
        <template v-slot:activator="{ props }">
          <v-icon v-bind="props" class="cursor-pointer">mdi-calendar</v-icon>
        </template>

        <v-date-picker
            v-model="selectedDate"
            color="primary"
            @update:model-value="onDateSelected"
            hide-header
        />
      </v-menu>
    </template>
  </v-text-field>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted } from "vue";

// Utility function to format Date to DD/MM/YYYY
const formatDateToDisplay = (date: Date | null): string => {
  if (!date) return "";
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  return `${day}/${month}/${year}`;
};

const props = defineProps<{
  modelValue: string | null | undefined; // Expecting ISO string or null
}>();

const emit = defineEmits<{
  (event: "update:modelValue", value: string | null): void;
}>();

const menu = ref<boolean>(false);
const dateFieldRef = ref<any>(null); // equivalent to useTemplateRef for broader compatibility

// Initialize selectedDate from ISO string prop
const parseIsoProp = (val: string | null | undefined): Date | null => {
    if (!val) return null;
    const d = new Date(val);
    return isNaN(d.getTime()) ? null : d;
}

const selectedDate = ref<Date | null>(parseIsoProp(props.modelValue));
const stringDate = ref<string>(formatDateToDisplay(selectedDate.value));

const onDateSelected = (value: unknown) => {
    // Vuetify 3 date picker emits unknown/Date
    const dateVal = value as Date;
  if (dateVal) {
    selectedDate.value = dateVal;
    stringDate.value = formatDateToDisplay(dateVal);
    // Emit ISO String
    emit("update:modelValue", dateVal.toISOString());
    menu.value = false;
  } else {
    selectedDate.value = null;
    emit("update:modelValue", null);
  }
};

watch(() => props.modelValue, (newValue) => {
  // If external change (e.g. initial load), update internal state
  // Check if different to avoid loops if we wanted strict equality, but here simple parsing is safe
  const newDate = parseIsoProp(newValue);
  if (newDate?.getTime() !== selectedDate.value?.getTime()) {
      selectedDate.value = newDate;
      stringDate.value = formatDateToDisplay(newDate);
  }
});

const onInput = (value: string) => {
    // Masking logic
  let newValue = value?.replace(/\D/g, "") ?? "";
  
  // Start formatting
  if (newValue.length > 4) {
    newValue = newValue.slice(0, 2) + "/" + newValue.slice(2, 4) + "/" + newValue.slice(4, 8);
  }
  else if (newValue.length > 2){
    newValue = newValue.slice(0, 2) + "/" + newValue.slice(2);
  }
  
  // Limiting length
  if (newValue.length > 10) {
      newValue = newValue.slice(0, 10);
  }

  stringDate.value = newValue;
  validateDate(newValue);
};

const validateDate = (input: string) => {
  if(!input || input.length < 10) {
      // Incomplete date
      // We do not emit null immediately to allow typing? 
      // Or we wait for full date. 
      // User code emits null if invalid.
    selectedDate.value = null;
    emit("update:modelValue", null);
    return;
  }

  const parts = input.split("/");
  if (parts.length === 3) {
    const nums = parts.map(Number);
    const day = nums[0];
    const month = nums[1];
    const year = nums[2];
    
    if (day === undefined || month === undefined || year === undefined) {
         selectedDate.value = null;
         emit("update:modelValue", null);
         return;
    }

    // Month is 0-indexed in JS Date
    const date = new Date(year, month - 1, day);

    if (
        date.getDate() === day &&
        date.getMonth() === month - 1 &&
        date.getFullYear() === year
    ) {
      selectedDate.value = date;
      // Adjust for timezone to avoid "previous day" issues when converting to ISO only for date part?
      // Actually standard ISO includes time. 
      // To represent "Start Date" without time issues, often it's best to set time to 12:00 or handle UTC.
      // But let's stick to standard ISO for now.
      // User's backend uses DateTimeOffset.
      // If we send Local time ISO, backend gets offset.
      const offsetDate = new Date(date.getTime() - (date.getTimezoneOffset() * 60000));
      emit("update:modelValue", offsetDate.toISOString());
    } else {
      // Invalid date logical (e.g. 30/02)
      // stringDate.value = ""; // User code cleared it, maybe annoying while typing? 
      // Let's keep the string but emit null
      selectedDate.value = null;
      emit("update:modelValue", null);
    }
  }
};
</script>

<style scoped>
.cursor-pointer {
    cursor: pointer;
}
/* Reusing Cyber Input Styles */
:deep(.v-field__outline__start),
:deep(.v-field__outline__end),
:deep(.v-field__outline__notch::before),
:deep(.v-field__outline__notch::after) {
    border-color: rgba(255, 255, 255, 0.2) !important;
}

:deep(.v-field--focused .v-field__outline__start),
:deep(.v-field--focused .v-field__outline__end),
:deep(.v-field--focused .v-field__outline__notch::before),
:deep(.v-field--focused .v-field__outline__notch::after) {
    border-color: var(--accent-orange) !important;
}

:deep(.v-label) {
    color: var(--text-dim) !important;
    font-family: var(--code-font, monospace);
}

:deep(.v-field__input) {
    color: #fff !important;
    font-family: var(--code-font, monospace);
}
</style>

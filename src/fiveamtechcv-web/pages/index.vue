<template>
  <entity-form 
      :submit-form="(e) => console.log(e)" 
      :entity-class="Tag"  
      :value="firstTag"
  />
</template>

<script setup lang="ts">
import {Tag, TagType} from "~/entities/entities";
import EntityForm from "~/components/form/entityForm.vue";
import {dateToTicks} from "~/utils/dateUtilities";


const firstTag = ref<Tag>(new Tag({
  id: 1,
  updatedAt: dateToTicks(new Date()),
  name: 'First Tag',
  type: TagType.Framework,
  documentationLink: 'https://www.google.com'
}));


</script>
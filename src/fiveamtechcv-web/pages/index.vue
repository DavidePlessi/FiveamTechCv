<template>
  <div>
    <div v-for="tag in data?.tags ?? []" :key="tag?.id">
      <p>{{ tag?.name }}</p>
      <p>{{ tag?.type }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">

import type {IQueryResult} from "~/entities/entities";

const query = gql`
  query GetTags {
    tags {
      documentationLink
      id
      name
      type
      updatedAt
      projects {
        description
        id
        name
        updatedAt
      }
    }
  }
`

const {data} = await useAsyncQuery<IQueryResult>(query, {})
</script>
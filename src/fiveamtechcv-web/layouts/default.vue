<template>
  <Title>Navbar</Title>
  <main>
    <slot />  
  </main>  
  <Title>Footer</Title>
</template>
<script setup lang="ts">
import Title from "~/components/text/title.vue";
</script>
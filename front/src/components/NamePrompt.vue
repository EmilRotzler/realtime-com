<script setup lang="ts">
import { computed, ref } from 'vue'

const emit = defineEmits<{ join: [name: string] }>()

const name = ref('')
const trimmed = computed(() => name.value.trim())

function submit() {
  if (trimmed.value) emit('join', trimmed.value)
}
</script>

<template>
  <form class="name-prompt" @submit.prevent="submit">
    <label for="display-name">Choose a display name</label>
    <input id="display-name" v-model="name" maxlength="32" autocomplete="off" autofocus />
    <button type="submit" :disabled="!trimmed">Join</button>
  </form>
</template>

<style scoped>
.name-prompt {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-width: 20rem;
}
</style>

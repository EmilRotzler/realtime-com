<script setup lang="ts">
import { ref } from 'vue'
import ChatView from './components/ChatView.vue'
import NamePrompt from './components/NamePrompt.vue'

// sessionStorage is per tab, so each tab is its own user but survives a refresh
const NAME_KEY = 'chatName'

const name = ref<string | null>(readName())

function readName(): string | null {
  try {
    return sessionStorage.getItem(NAME_KEY)
  } catch {
    return null
  }
}

function join(newName: string) {
  name.value = newName
  try {
    sessionStorage.setItem(NAME_KEY, newName)
  } catch {
    // Storage unavailable: the name lasts until this page is closed
  }
}
</script>

<template>
  <main class="app">
    <h1>Realtime Chat</h1>
    <NamePrompt v-if="!name" @join="join" />
    <ChatView v-else :name="name" />
  </main>
</template>

<style>
body {
  margin: 0;
  font-family: system-ui, sans-serif;
  background: #fff;
  color: #222;
}
.app {
  max-width: 48rem;
  margin: 0 auto;
  padding: 1rem;
}
</style>

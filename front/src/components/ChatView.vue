<script setup lang="ts">
import { useChatHub } from '@/composables/useChatHub'
import ChatBox from './ChatBox.vue'
import MessageChart from './MessageChart.vue'

const props = defineProps<{ name: string }>()

const { messages, stats, status, error, send } = useChatHub()

function onSend(text: string) {
  void send(props.name, text)
}
</script>

<template>
  <div class="chat-view">
    <div class="status-bar">
      <span>Signed in as <strong>{{ name }}</strong></span>
      <span class="status" :data-status="status" data-testid="connection-status">{{ status }}</span>
    </div>
    <p v-if="error" class="error" role="alert">
      {{ error }}
      <button type="button" aria-label="Dismiss error" @click="error = null">×</button>
    </p>
    <MessageChart :stats="stats" />
    <ChatBox :messages="messages" :current-user="name" :connected="status === 'connected'" @send="onSend" />
  </div>
</template>

<style scoped>
.chat-view {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.status-bar {
  display: flex;
  justify-content: space-between;
}
.status {
  font-size: 0.85rem;
  padding: 0.1rem 0.5rem;
  border-radius: 999px;
  background: #eee;
}
.status[data-status='connected'] {
  background: #d4f3e3;
}
.status[data-status='reconnecting'],
.status[data-status='connecting'] {
  background: #fff1c2;
}
.status[data-status='disconnected'] {
  background: #fbd5d5;
}
.error {
  margin: 0;
  padding: 0.5rem;
  border-radius: 6px;
  background: #fbd5d5;
  display: flex;
  justify-content: space-between;
}
</style>

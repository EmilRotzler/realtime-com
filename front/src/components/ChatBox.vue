<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import type { ChatMessage } from '@/types'

const props = defineProps<{ messages: ChatMessage[]; currentUser: string; connected: boolean }>()
const emit = defineEmits<{ send: [text: string] }>()

const draft = ref('')
const list = ref<HTMLElement | null>(null)
const canSend = computed(() => props.connected && draft.value.trim().length > 0)

const timeFormat = new Intl.DateTimeFormat(undefined, {
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

function submit() {
  if (!canSend.value) return
  emit('send', draft.value.trim())
  draft.value = ''
}

// Keep the newest message in view, both on push and when history replaces the list
watch(
  () => [props.messages, props.messages.length],
  async () => {
    await nextTick()
    list.value?.scrollTo({ top: list.value.scrollHeight })
  },
)
</script>

<template>
  <section class="chat-box">
    <ul ref="list" class="messages" data-testid="messages">
      <li v-for="(message, i) in messages" :key="i" :class="{ own: message.user === currentUser }">
        <div class="meta">
          <strong>{{ message.user }}</strong>
          <time :datetime="message.sentAt">{{ timeFormat.format(new Date(message.sentAt)) }}</time>
        </div>
        <p>{{ message.text }}</p>
      </li>
      <li v-if="messages.length === 0" class="empty">No messages yet. Say hi!</li>
    </ul>
    <form @submit.prevent="submit">
      <input
        v-model="draft"
        aria-label="Message"
        placeholder="Type a message…"
        maxlength="500"
        autocomplete="off"
        :disabled="!connected"
      />
      <button type="submit" :disabled="!canSend">Send</button>
    </form>
  </section>
</template>

<style scoped>
.messages {
  list-style: none;
  margin: 0;
  padding: 0.5rem;
  height: 320px;
  overflow-y: auto;
  border: 1px solid #ddd;
  border-radius: 6px;
}
.messages li {
  margin-bottom: 0.5rem;
}
.messages li.own {
  text-align: right;
}
.messages li.own p {
  background: #e3f6ee;
}
.meta {
  font-size: 0.8rem;
  color: #666;
  display: flex;
  gap: 0.5rem;
}
.own .meta {
  justify-content: flex-end;
}
p {
  display: inline-block;
  margin: 0.15rem 0 0;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  background: #f2f2f2;
  white-space: pre-wrap;
  word-break: break-word;
}
.empty {
  color: #999;
}
form {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}
input {
  flex: 1;
}
</style>

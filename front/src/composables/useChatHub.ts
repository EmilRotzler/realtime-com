import { onUnmounted, ref } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type { ChatMessage, ConnectionStatus, MinuteCount } from '@/types'

export function useChatHub() {
  const messages = ref<ChatMessage[]>([])
  const stats = ref<MinuteCount[]>([])
  const status = ref<ConnectionStatus>('disconnected')
  const error = ref<string | null>(null)

  const apiUrl = import.meta.env.VITE_API_URL
  if (!apiUrl) {
    error.value = 'VITE_API_URL is not configured'
    return { messages, stats, status, error, send: async () => {} }
  }

  const connection = new HubConnectionBuilder()
    .withUrl(`${apiUrl}/hub`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  // On (re)connect the server sends history and stats, so the client resyncs itself
  connection.on('ReceiveHistory', (history: ChatMessage[]) => {
    messages.value = history
  })
  connection.on('ReceiveMessage', (message: ChatMessage) => {
    messages.value.push(message)
  })
  connection.on('ReceiveStats', (snapshot: MinuteCount[]) => {
    stats.value = snapshot
  })
  connection.onreconnecting(() => (status.value = 'reconnecting'))
  connection.onreconnected(() => (status.value = 'connected'))
  connection.onclose(() => (status.value = 'disconnected'))

  status.value = 'connecting'
  connection
    .start()
    .then(() => (status.value = 'connected'))
    .catch((e: unknown) => {
      status.value = 'disconnected'
      error.value = `Could not connect: ${errorMessage(e)}`
    })

  onUnmounted(() => void connection.stop())

  async function send(user: string, text: string) {
    try {
      await connection.invoke('SendMessage', user, text)
    } catch (e) {
      error.value = errorMessage(e)
    }
  }

  return { messages, stats, status, error, send }
}

// HubException text arrives wrapped as "An unexpected error ... HubException: <message>"
function errorMessage(e: unknown): string {
  const message = e instanceof Error ? e.message : String(e)
  return message.replace(/^.*HubException: /, '')
}

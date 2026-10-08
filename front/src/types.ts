// Mirrors the backend records (SignalR serializes them camelCase, dates as ISO strings)
export interface ChatMessage {
  user: string
  text: string
  sentAt: string
}

export interface MinuteCount {
  minute: string
  count: number
}

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

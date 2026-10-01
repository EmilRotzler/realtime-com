import process from 'node:process'

// Shared by playwright.config.ts and the specs
export const BACKEND_URL = 'http://localhost:5222'
export const FRONTEND_PORT = process.env.CI ? 4173 : 5173
export const FRONTEND_URL = `http://localhost:${FRONTEND_PORT}`

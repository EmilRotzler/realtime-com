import { test, expect } from '@playwright/test'
import { BACKEND_URL } from './urls'

test('shows the app title and name prompt', async ({ page }) => {
  await page.goto('/')
  await expect(page.locator('h1')).toHaveText('Realtime Chat')
  await expect(page.getByLabel('Choose a display name')).toBeVisible()
})

test('backend is healthy', async ({ request }) => {
  const response = await request.get(`${BACKEND_URL}/health`)
  expect(response.ok()).toBe(true)
  expect(await response.text()).toBe('Healthy')
})

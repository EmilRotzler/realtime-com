import { test, expect } from '@playwright/test'
import { BACKEND_URL } from './urls'

test('visits the app root url', async ({ page }) => {
  await page.goto('/')
  await expect(page.locator('h1')).toHaveText('You did it!')
})

test('backend is healthy', async ({ request }) => {
  const response = await request.get(`${BACKEND_URL}/health`)
  expect(response.ok()).toBe(true)
  expect(await response.text()).toBe('Healthy')
})

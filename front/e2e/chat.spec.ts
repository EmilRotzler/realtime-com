import { test, expect, type Browser, type Page } from '@playwright/test'

// Each tab gets its own browser context, so its own sessionStorage, like separate users
async function joinAs(browser: Browser, name: string): Promise<Page> {
  const context = await browser.newContext()
  const page = await context.newPage()
  await page.goto('/')
  await page.getByLabel('Choose a display name').fill(name)
  await page.getByRole('button', { name: 'Join' }).click()
  await expect(page.getByTestId('connection-status')).toHaveText('connected')
  return page
}

// The label only renders once the first stats snapshot has arrived
async function currentMinuteCount(page: Page): Promise<number> {
  const text = await page.getByTestId('current-minute-count').textContent()
  return Number(text?.replace(/\D/g, ''))
}

// The backend is shared across tests and runs, and a test can cross a minute
// boundary, so only check that the count moved and is at least 1
async function expectCountToChangeFrom(page: Page, before: number) {
  await expect
    .poll(async () => {
      const now = await currentMinuteCount(page)
      return now !== before && now >= 1
    })
    .toBe(true)
}

async function sendMessage(page: Page, text: string) {
  await page.getByLabel('Message').fill(text)
  await page.getByRole('button', { name: 'Send' }).click()
}

test('a message and the minute count reach every tab', async ({ browser }) => {
  const alice = await joinAs(browser, 'Alice')
  const bob = await joinAs(browser, 'Bob')
  const aliceBefore = await currentMinuteCount(alice)
  const bobBefore = await currentMinuteCount(bob)
  const text = `hello from alice ${Date.now()}`

  await sendMessage(alice, text)

  const bobsCopy = bob.getByTestId('messages').locator('li', { hasText: text })
  await expect(bobsCopy).toBeVisible()
  await expect(bobsCopy).toContainText('Alice')
  await expect(alice.getByLabel('Message')).toHaveValue('')
  await expectCountToChangeFrom(alice, aliceBefore)
  await expectCountToChangeFrom(bob, bobBefore)

  await alice.context().close()
  await bob.context().close()
})

test('a tab opened later sees earlier messages', async ({ browser }) => {
  const alice = await joinAs(browser, 'Alice')
  const text = `history check ${Date.now()}`
  await sendMessage(alice, text)
  await expect(alice.getByTestId('messages')).toContainText(text)

  const carol = await joinAs(browser, 'Carol')

  await expect(carol.getByTestId('messages')).toContainText(text)

  await alice.context().close()
  await carol.context().close()
})

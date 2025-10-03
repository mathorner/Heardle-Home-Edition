import { test, expect } from '@playwright/test';

test('page title is Heardle Home Edition', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/Heardle Home Edition/);
});

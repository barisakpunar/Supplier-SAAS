import { expect, type Page } from '@playwright/test';

export async function login(page: Page, email: string, password: string, returnUrl = '/admin'): Promise<void> {
  await page.goto(`/login?returnUrl=${encodeURIComponent(returnUrl)}`);
  await page.getByLabel(/^(Email|E-posta):/i).fill(email);
  await page.getByLabel(/^(Password|Şifre):/i).fill(password);
  await page.getByRole('button', { name: /^(Log in|Giriş yap)$/i }).click();
  await page.waitForLoadState('networkidle');
}

export async function loginAsStoreOwner(page: Page, returnUrl = '/admin'): Promise<void> {
  await login(page, 'owner-a@test.local', 'Test123!', returnUrl);
  await expect(page).toHaveURL(/\/admin/i);
}

export async function loginAsBuyerA1(page: Page, returnUrl = '/'): Promise<void> {
  await login(page, 'buyer-a1@example.com', 'Test123!', returnUrl);
}

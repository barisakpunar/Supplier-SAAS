import { expect, test } from '@playwright/test';
import { loginAsStoreOwner } from './helpers/auth';

const financeCollectionsHeading = /(?:dealer collections|bayi tahsilatlari)/i;
const financeTransactionsHeading = /(?:dealer transactions|bayi işlemleri)/i;
const financeInstrumentsHeading = /(?:dealer financial instruments|bayi finansal belgeleri)/i;
const addCollectionHeading = /(?:add new collection|yeni tahsilat ekle)/i;
const saveButton = /^(save|kaydet)$/i;
const accessDeniedText = /(?:access denied|giriş engellendi|seçili işlemi gerçekleştirmek için izniniz yok)/i;

test.describe('Dealer finance admin smoke', () => {
  test('store owner can open finance list pages', async ({ page }) => {
    await loginAsStoreOwner(page);

    await page.goto('/Admin/Dealer/Collections');
    await expect(page.getByRole('heading', { name: financeCollectionsHeading })).toBeVisible();
    await expect(page.locator('table')).toBeVisible();

    await page.goto('/Admin/Dealer/Transactions?SearchDealerId=2');
    await expect(page.getByRole('heading', { name: financeTransactionsHeading })).toBeVisible();

    await page.goto('/Admin/Dealer/FinancialInstruments');
    await expect(page.getByRole('heading', { name: financeInstrumentsHeading })).toBeVisible();
  });

  test('store owner can create a cash collection for dealer a', async ({ page }) => {
    const referenceNo = `PW-CASH-${Date.now()}`;

    await loginAsStoreOwner(page);
    await page.goto('/Admin/Dealer/CreateCollection?storeId=2&dealerId=2&customerId=26');

    await expect(page.getByRole('heading', { name: addCollectionHeading })).toBeVisible();
    await page.locator('[name="CollectionMethodId"]').selectOption('10');
    await page.locator('[name="Amount"]').fill('123.45');
    await page.locator('[name="ReferenceNo"]').fill(referenceNo);
    await page.locator('[name="Note"]').fill('Playwright smoke cash collection');
    await page.getByRole('button', { name: saveButton }).click();

    await expect(page).toHaveURL(/\/Admin\/Dealer\/Collections/i);
    await expect(page.locator('body')).toContainText(referenceNo);
  });

  test('store owner cannot access store b dealer edit page', async ({ page }) => {
    await loginAsStoreOwner(page);
    await page.goto('/Admin/Dealer/Edit/1');

    await expect(page.locator('body')).toContainText(accessDeniedText);
  });
});

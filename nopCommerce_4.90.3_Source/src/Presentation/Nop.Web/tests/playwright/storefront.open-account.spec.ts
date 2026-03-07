import { expect, test, type Page } from '@playwright/test';
import { loginAsBuyerA1, loginAsStoreOwner } from './helpers/auth';
const thankYouHeading = /(?:thank you|teşekkür)/i;

async function waitUntilVisible(page: Page, selector: string, timeout = 15000): Promise<boolean> {
  try {
    await page.locator(selector).waitFor({ state: 'visible', timeout });
    return true;
  } catch {
    return false;
  }
}

test.describe('Dealer finance storefront smoke', () => {
  test('buyer a1 can checkout with open account', async ({ page }) => {
    await loginAsBuyerA1(page, '/p-a-1');

    await page.goto('/p-a-1');
    await expect(page.locator('#add-to-cart-button-48')).toBeVisible();
    await page.locator('#add-to-cart-button-48').click();
    await expect(page.locator('.bar-notification.success')).toBeVisible();

    await page.goto('/cart');
    await expect(page.locator('#checkout')).toBeVisible();
    if (await waitUntilVisible(page, '#termsofservice', 5000))
      await page.locator('#termsofservice').check();
    await page.locator('#checkout').click();

    await expect(page).toHaveURL(/\/onepagecheckout/i);
    await expect(page.locator('#checkout-step-billing')).toBeVisible();

    await page.locator('#billing-buttons-container .new-address-next-step-button').click();

    if (await waitUntilVisible(page, '#checkout-step-shipping'))
      await page.locator('#shipping-buttons-container .new-address-next-step-button').click();

    if (await waitUntilVisible(page, '#checkout-step-shipping-method'))
      await page.locator('#shipping-method-buttons-container .shipping-method-next-step-button').click();

    await expect(page.locator('input[name="paymentmethod"][value="Payments.OpenAccount"]')).toBeVisible({ timeout: 15000 });
    await page.locator('input[name="paymentmethod"][value="Payments.OpenAccount"]').check();
    await page.locator('#payment-method-buttons-container .payment-method-next-step-button').click();

    await expect(page.locator('#checkout-step-payment-info')).toBeVisible({ timeout: 15000 });
    await page.locator('#payment-info-buttons-container .payment-info-next-step-button').click();

    await expect(page.locator('#checkout-step-confirm-order')).toBeVisible({ timeout: 15000 });
    if (await waitUntilVisible(page, '#termsofservice', 5000))
      await page.locator('#termsofservice').check();
    const confirmResponsePromise = page.waitForResponse(response =>
      response.request().method() === 'POST' && response.url().includes('/checkout/OpcConfirmOrder/'));
    await page.locator('#confirm-order-buttons-container .confirm-order-next-step-button').click();

    const confirmResponse = await confirmResponsePromise;
    expect(confirmResponse.ok()).toBeTruthy();
    await page.goto('/checkout/completed');

    await expect(page).toHaveURL(/\/checkout\/completed/i, { timeout: 30000 });
    await expect(page.getByRole('heading', { name: thankYouHeading })).toBeVisible();

    const orderDetailsHref = await page.locator('.details-link a').getAttribute('href');
    expect(orderDetailsHref).toBeTruthy();

    const orderId = orderDetailsHref?.match(/orderdetails\/(\d+)/i)?.[1];
    expect(orderId).toBeTruthy();
    await page.goto(orderDetailsHref!);
    await expect(page.locator('body')).toContainText(/open account/i);

    await page.goto('/logout');
    await loginAsStoreOwner(page, '/Admin/Dealer/Transactions?SearchDealerId=2');
    await expect(page).toHaveURL(/\/Admin\/Dealer\/Transactions/i);
    await expect(page.locator('body')).toContainText(/open account order posted/i);
    await expect(page.locator('body')).toContainText(orderId!);
    await expect(page.locator('body')).toContainText(/148/);
  });
});

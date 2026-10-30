import { test, expect } from '@playwright/test';

const managerUsername = process.env.DEMO_MANAGER_USERNAME ?? 'demo.manager';
const managerPassword = process.env.DEMO_MANAGER_PASSWORD ?? 'ChangeMe123!';
const uniqueInvoiceName = `Manager Over Limit Test ${Date.now()}`;

test('rejects a Manager over-limit approval through the UI', async ({ page }) => {
  await page.goto('/');

  await page.getByLabel('Username').fill(managerUsername);
  await page.getByLabel('Password').fill(managerPassword);
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByRole('heading', { name: 'Invoice Approvals' })).toBeVisible();

  await page.getByRole('tab', { name: 'Invoices' }).click();
  await page.getByLabel('Customer name').fill(uniqueInvoiceName);
  await page.getByLabel('Amount').fill('60000');
  await page.getByRole('button', { name: 'Create invoice' }).click();

  const invoiceRow = page.locator('tr').filter({ hasText: uniqueInvoiceName }).filter({ hasText: '$60,000.00' });
  await expect(invoiceRow).toHaveCount(1);
  await invoiceRow.getByRole('button', { name: /Submit invoice/i }).click();

  await page.getByRole('tab', { name: 'Approvals' }).click();
  const pendingItem = page.getByRole('listitem').filter({ hasText: uniqueInvoiceName }).filter({ hasText: '$60,000.00' });
  await expect(pendingItem).toHaveCount(1);
  await pendingItem.getByRole('button', { name: /Approve invoice/i }).click();

  const rejectionAlert = page.getByRole('alert');
  await expect(rejectionAlert).toContainText(/Managers cannot approve invoices above 50,000/i);

  await page.getByRole('tab', { name: 'Invoices' }).click();
  const invoiceAfterAttempt = page.locator('tr').filter({ hasText: uniqueInvoiceName }).filter({ hasText: 'PendingApproval' });
  await expect(invoiceAfterAttempt).toHaveCount(1);
  await expect(invoiceAfterAttempt).toContainText('PendingApproval');

  // The current app UI does not expose approval history; if it is added later, assert no new history entry exists for the rejected attempt.
});

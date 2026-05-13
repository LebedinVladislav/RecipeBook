import { expect, Page } from '@playwright/test';

export class DishDetailPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async expectField(label: string, value: string) {
    const paragraph = this.page.locator('p', { hasText: `${label}` });
    await expect(paragraph).toContainText(value);
  }

  async expectIngredient(name: string, amount: number) {
    await expect(this.page.getByText(`${name}: ${amount} г`)).toBeVisible();
  }
}

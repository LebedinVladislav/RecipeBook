import { expect, Page } from '@playwright/test';

export class ProductDetailPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async getId() {
    const text = await this.page.locator('.small-text').textContent();
    const match = text?.match(/ID\s*(\d+)/);
    return match ? Number(match[1]) : undefined;
  }

  async expectField(label: string, value: string) {
    const paragraph = this.page.locator('p', { hasText: `${label}` });
    await expect(paragraph).toContainText(value);
  }

  async expectPhotosCount(count: number) {
    await expect(this.page.locator('.image-grid img')).toHaveCount(count);
  }
}

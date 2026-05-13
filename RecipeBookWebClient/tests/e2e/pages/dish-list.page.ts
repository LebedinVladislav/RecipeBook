import { Locator, Page } from '@playwright/test';

export class DishListPage {
  readonly page: Page;
  readonly createButton: Locator;
  readonly searchInput: Locator;
  readonly categorySelect: Locator;

  constructor(page: Page) {
    this.page = page;
    this.createButton = page.getByRole('link', { name: 'Создать блюдо' });
    this.searchInput = page.getByLabel('Поиск');
    this.categorySelect = page.getByLabel('Категория');
  }

  async goto() {
    await this.page.goto('/dishes');
  }

  async createNew() {
    await this.createButton.click();
  }

  async search(term: string) {
    await this.searchInput.fill(term);
  }

  async selectCategory(category: string) {
    await this.categorySelect.selectOption({ label: category });
  }

  async filterFlag(flagLabel: string) {
    await this.page.getByLabel(flagLabel).check();
  }

  getDishCard(name: string): Locator {
    return this.page.locator('.product-card', { has: this.page.getByText(name) });
  }

  async openDish(name: string) {
    await this.getDishCard(name).getByRole('link', { name }).click();
  }

  async deleteDish(name: string) {
    const card = this.getDishCard(name);
    this.page.once('dialog', (dialog) => dialog.accept());
    await card.getByRole('button', { name: 'Удалить' }).click();
  }
}

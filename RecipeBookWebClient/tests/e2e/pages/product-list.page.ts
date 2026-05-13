import { Locator, Page } from '@playwright/test';

export class ProductListPage {
  readonly page: Page;
  readonly createButton: Locator;
  readonly searchInput: Locator;
  readonly categorySelect: Locator;
  readonly cookingSelect: Locator;
  readonly sortSelect: Locator;

  constructor(page: Page) {
    this.page = page;
    this.createButton = page.getByRole('link', { name: 'Создать продукт' });
    this.searchInput = page.getByLabel('Поиск');
    this.categorySelect = page.getByLabel('Категория');
    this.cookingSelect = page.getByLabel('Необходимость готовки');
    this.sortSelect = page.getByLabel('Сортировка');
  }

  async goto() {
    await this.page.goto('/products');
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

  async selectCookingRequired(option: string) {
    await this.cookingSelect.selectOption({ label: option });
  }

  async filterFlag(flagLabel: string) {
    await this.page.getByLabel(flagLabel).check();
  }

  async sortBy(option: string) {
    await this.sortSelect.selectOption({ label: option });
  }

  getProductCards(): Locator {
    return this.page.locator('.product-card');
  }

  getProductCard(name: string): Locator {
    return this.page.locator('.product-card', { has: this.page.getByText(name) });
  }

  async openProduct(name: string) {
    await this.getProductCard(name).getByRole('link', { name }).click();
  }

  async editProduct(name: string) {
    await this.getProductCard(name).getByRole('link', { name: 'Редактировать' }).click();
  }

  async deleteProduct(name: string) {
    const card = this.getProductCard(name);
    this.page.once('dialog', (dialog) => dialog.accept());
    await card.getByRole('button', { name: 'Удалить' }).click();
  }
}

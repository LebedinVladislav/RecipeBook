import { expect, Locator, Page } from '@playwright/test';

export interface DishFormData {
  name: string;
  photoUrls: string[];
  portionSize: number;
  category?: string;
  flags?: string[];
  ingredients: Array<{ productName: string; amount: number }>;
}

export class DishFormPage {
  readonly page: Page;
  readonly nameInput: Locator;
  readonly photoUrlInput: Locator;
  readonly addPhotoButton: Locator;
  readonly portionSizeInput: Locator;
  readonly categorySelect: Locator;
  readonly saveButton: Locator;
  readonly caloriesInput: Locator;
  readonly proteinsInput: Locator;
  readonly fatsInput: Locator;
  readonly carbsInput: Locator;
  readonly addIngredientButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nameInput = page.getByLabel('Название');
    this.photoUrlInput = page.getByPlaceholder('URL фотографии');
    this.addPhotoButton = page.getByRole('button', { name: 'Добавить' });
    this.portionSizeInput = page.getByLabel('Размер порции (г)');
    this.categorySelect = page.getByLabel('Категория');
    this.saveButton = page.getByRole('button', { name: /Сохранить/ });
    this.caloriesInput = page.getByLabel('Калории на порцию');
    this.proteinsInput = page.getByLabel('Белки на порцию');
    this.fatsInput = page.getByLabel('Жиры на порцию');
    this.carbsInput = page.getByLabel('Углеводы на порцию');
    this.addIngredientButton = page.getByRole('button', { name: 'Добавить продукт' });
  }

  async gotoNew() {
    await this.page.goto('/dishes/new');
  }

  async fillName(name: string) {
    await this.nameInput.fill(name);
  }

  async addPhotoUrls(urls: string[]) {
    for (const url of urls) {
      await this.photoUrlInput.fill(url);
      await this.addPhotoButton.click();
    }
  }

  async getPhotoCount() {
    return this.page.locator('.photo-preview').count();
  }

  async removePhoto(index: number) {
    await this.page.locator('.photo-remove').nth(index).click();
  }

  async fillPortionSize(value: number) {
    await this.portionSizeInput.fill(String(value));
  }

  async selectCategory(category: string) {
    await this.categorySelect.selectOption({ label: category });
  }

  async setFlag(flag: string) {
    await this.page.getByLabel(flag).check();
  }

  async expectFlagEnabled(flag: string, enabled: boolean) {
    const checkbox = this.page.getByLabel(flag);
    if (enabled) {
      await expect(checkbox).toBeEnabled();
    } else {
      await expect(checkbox).toBeDisabled();
    }
  }

  async fillIngredient(index: number, productName: string, amount: number) {
    const row = this.page.locator('.card').nth(index);
    await row.getByLabel('Продукт').selectOption({ label: productName });
    await row.getByLabel('Вес (г)').fill(String(amount));
  }

  async addIngredient() {
    await this.addIngredientButton.click();
  }

  async submit() {
    await this.saveButton.click();
  }

  async getNutritionValues() {
    return {
      calories: await this.caloriesInput.inputValue(),
      proteins: await this.proteinsInput.inputValue(),
      fats: await this.fatsInput.inputValue(),
      carbs: await this.carbsInput.inputValue()
    };
  }
}

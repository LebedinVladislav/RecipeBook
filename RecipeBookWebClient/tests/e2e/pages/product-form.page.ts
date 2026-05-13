import { expect, Locator, Page } from '@playwright/test';

export interface ProductFormData {
  name: string;
  photoUrls: string[];
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  composition?: string;
  category: string;
  cookingRequired: string;
  flags?: string[];
}

export class ProductFormPage {
  readonly page: Page;
  readonly nameInput: Locator;
  readonly photoFileInput: Locator;
  readonly photoUrlInput: Locator;
  readonly addPhotoButton: Locator;
  readonly caloriesInput: Locator;
  readonly proteinsInput: Locator;
  readonly fatsInput: Locator;
  readonly carbsInput: Locator;
  readonly compositionInput: Locator;
  readonly categorySelect: Locator;
  readonly cookingSelect: Locator;
  readonly saveButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nameInput = page.getByLabel('Название');
    this.photoFileInput = page.locator('input[type="file"]');
    this.photoUrlInput = page.getByPlaceholder('URL фотографии');
    this.addPhotoButton = page.getByRole('button', { name: 'Добавить' });
    this.caloriesInput = page.getByLabel('Калорийность на 100 г');
    this.proteinsInput = page.getByLabel('Белки на 100 г');
    this.fatsInput = page.getByLabel('Жиры на 100 г');
    this.carbsInput = page.getByLabel('Углеводы на 100 г');
    this.compositionInput = page.getByLabel('Состав');
    this.categorySelect = page.getByLabel('Категория');
    this.cookingSelect = page.getByLabel('Необходимость готовки');
    this.saveButton = page.getByRole('button', { name: /Сохранить/ });
  }

  async gotoNew() {
    await this.page.goto('/products/new');
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

  async removePhoto(index: number) {
    await this.page.locator('.photo-remove').nth(index).click();
  }

  async getPhotoCount() {
    return this.page.locator('.photo-preview').count();
  }

  async fillNutrition(calories: number, proteins: number, fats: number, carbs: number) {
    await this.caloriesInput.fill(String(calories));
    await this.proteinsInput.fill(String(proteins));
    await this.fatsInput.fill(String(fats));
    await this.carbsInput.fill(String(carbs));
  }

  async fillComposition(composition: string) {
    await this.compositionInput.fill(composition);
  }

  async selectCategory(category: string) {
    await this.categorySelect.selectOption({ label: category });
  }

  async selectCookingRequired(option: string) {
    await this.cookingSelect.selectOption({ label: option });
  }

  async toggleFlag(flagLabel: string) {
    await this.page.getByLabel(flagLabel).check();
  }

  async uncheckFlag(flagLabel: string) {
    await this.page.getByLabel(flagLabel).uncheck();
  }

  async submit() {
    await this.saveButton.click();
  }

  async expectFieldInvalid(label: string) {
    const input = this.page.getByLabel(label);
    await expect(input).toBeVisible();
    const validity = await input.evaluate((element) => (element as HTMLInputElement).validity);
    expect(validity.valid).toBe(false);
  }
}

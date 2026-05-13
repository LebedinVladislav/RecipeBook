import { expect, test } from './fixtures';
import { ProductListPage } from './pages/product-list.page';
import { ProductFormPage } from './pages/product-form.page';
import { ProductDetailPage } from './pages/product-detail.page';
import { createProduct, createDish } from './helpers/api-helper';

const unique = () => `test-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`;

const defaultProduct = (nameSuffix: string) => ({
  name: `Test Product ${nameSuffix}`,
  photos: ['https://via.placeholder.com/120'],
  calories: 10,
  proteins: 10,
  fats: 10,
  carbs: 10,
  composition: 'test composition',
  category: 'Овощи',
  cookingRequired: 'ГотовыйКУпотреблению',
  flags: ['Веган']
});

test.afterEach(async ({ cleanupEntities }) => {
  await cleanupEntities();
});

const invalidKbjuCases = [
  { label: 'Невалидные калории', values: { calories: -1, proteins: 0, fats: 0, carbs: 0 } },
  { label: 'Невалидные белки', values: { calories: 0, proteins: -1, fats: 0, carbs: 0 } },
  { label: 'Невалидные жиры', values: { calories: 0, proteins: 0, fats: -1, carbs: 0 } },
  { label: 'Невалидные углеводы', values: { calories: 0, proteins: 0, fats: 0, carbs: -1 } }
];

test.describe('Product management', () => {
  test('should show native minlength validation for product name shorter than 2 symbols', async ({ page }) => {
    const form = new ProductFormPage(page);
    await form.gotoNew();
    await form.fillName('A');
    await form.fillNutrition(0, 0, 0, 0);
    await form.submit();
    await form.expectFieldInvalid('Название');
  });

  for (const caseItem of invalidKbjuCases) {
    test(`should reject invalid KBJU field ${caseItem.label} with native min validation`, async ({ page }) => {
      const form = new ProductFormPage(page);
      await form.gotoNew();
      await form.fillName(`Valid ${unique()}`);
      await form.fillNutrition(
        caseItem.values.calories,
        caseItem.values.proteins,
        caseItem.values.fats,
        caseItem.values.carbs
      );
      await form.submit();
      await form.expectFieldInvalid(caseItem.label);
    });
  }

  test('should create product with photo limits and remove photos leaving a single image', async ({ page, createdProductIds }) => {
    const name = `Photo product ${unique()}`;
    const form = new ProductFormPage(page);
    const list = new ProductListPage(page);
    const detail = new ProductDetailPage(page);

    await form.gotoNew();
    await form.fillName(name);
    await form.addPhotoUrls([
      'https://via.placeholder.com/1',
      'https://via.placeholder.com/2',
      'https://via.placeholder.com/3',
      'https://via.placeholder.com/4',
      'https://via.placeholder.com/5',
      'https://via.placeholder.com/6',
      'https://via.placeholder.com/7'
    ]);

    expect(await form.getPhotoCount()).toBe(5);

    while ((await form.getPhotoCount()) > 1) {
      await form.removePhoto(0);
    }
    expect(await form.getPhotoCount()).toBe(1);

    await form.fillNutrition(100, 20, 5, 15);
    await form.fillComposition('Рецепт из теста');
    await form.selectCategory('Овощи');
    await form.selectCookingRequired('Готовый к употреблению');
    await form.toggleFlag('Веган');
    await form.submit();

    await list.goto();
    await list.search(name);
    await expect(await list.getProductCards().count()).toBe(1);
    await list.openProduct(name);
    const id = await detail.getId();
    expect(id).toBeGreaterThan(0);
    if (id) createdProductIds.push(id);
  });

  test('should search, filter and sort products correctly', async ({ page, createdProductIds }) => {
    const productA = await createProduct(page.request, {
      ...defaultProduct(`${unique()}-A`),
      name: `Product Search A ${unique()}`,
      category: 'Мясной',
      cookingRequired: 'Полуфабрикат',
      flags: ['БезГлютена']
    });
    const productB = await createProduct(page.request, {
      ...defaultProduct(`${unique()}-B`),
      name: `Product Search B ${unique()}`,
      category: 'Овощи',
      cookingRequired: 'ГотовыйКУпотреблению',
      flags: ['Веган']
    });
    createdProductIds.push(productA.id, productB.id);

    const list = new ProductListPage(page);
    await list.goto();

    await list.search('Search A');
    expect(await list.getProductCards().count()).toBe(1);

    await list.search('');
    await list.selectCategory('Мясной');
    expect(await list.getProductCards().count()).toBeGreaterThan(0);

    await list.selectCategory('Овощи');
    await list.filterFlag('Веган');
    expect(await list.getProductCards().count()).toBe(1);
  });

  test('should edit product successfully', async ({ page, createdProductIds }) => {
    const product = await createProduct(page.request, defaultProduct(`${unique()}-edit`));
    createdProductIds.push(product.id);

    const list = new ProductListPage(page);
    await list.goto();
    await list.openProduct(product.name);
    await page.getByRole('link', { name: 'Редактировать' }).click();

    const form = new ProductFormPage(page);
    await form.fillName(`${product.name} Updated`);
    await form.fillNutrition(101, 11, 11, 11);
    await form.submit();

    await list.goto();
    await list.search('Updated');
    expect(await list.getProductCards().count()).toBe(1);
  });

  test('should delete product that is not used in a dish', async ({ page, createdProductIds }) => {
    const product = await createProduct(page.request, defaultProduct(`${unique()}-delete`));
    createdProductIds.push(product.id);

    const list = new ProductListPage(page);
    await list.goto();
    await list.search(product.name);
    expect(await list.getProductCards().count()).toBe(1);
    await list.deleteProduct(product.name);

    await list.search(product.name);
    expect(await list.getProductCards().count()).toBe(0);
    const index = createdProductIds.indexOf(product.id);
    if (index !== -1) createdProductIds.splice(index, 1);
  });

  test('should not delete product used in a dish', async ({ page, createdProductIds, createdDishIds }) => {
    const product = await createProduct(page.request, defaultProduct(`${unique()}-used`));
    createdProductIds.push(product.id);

    const dish = await createDish(page.request, {
      name: `Dish Uses Product ${unique()}`,
      calories: 100,
      proteins: 10,
      fats: 10,
      carbs: 10,
      ingredients: [{ productId: product.id, amount: 100 }],
      portionSize: 100,
      category: 'Первое',
      flags: ['Веган']
    });
    createdDishIds.push(dish.id);

    const list = new ProductListPage(page);
    await list.goto();
    await list.search(product.name);
    expect(await list.getProductCards().count()).toBe(1);

    await list.deleteProduct(product.name);
    await expect(page.locator('[role="status"]')).toContainText(/Ошибка удаления/i);
    await list.search(product.name);
    expect(await list.getProductCards().count()).toBe(1);
  });

  test('should display product details correctly', async ({ page, createdProductIds }) => {
    const product = await createProduct(page.request, {
      ...defaultProduct(`${unique()}-detail`),
      name: `Product Detail ${unique()}`,
      photos: ['https://via.placeholder.com/100', 'https://via.placeholder.com/101'],
      calories: 54,
      proteins: 12,
      fats: 8,
      carbs: 14,
      composition: 'Some ingredients',
      category: 'Сладости',
      cookingRequired: 'ТребуетПриготовления',
      flags: ['БезСахара']
    });
    createdProductIds.push(product.id);

    const list = new ProductListPage(page);
    await list.goto();
    await list.search(product.name);
    await list.openProduct(product.name);

    const detail = new ProductDetailPage(page);
    await detail.expectField('Категория:', 'Сладости');
    await detail.expectField('Готовность:', 'Требует приготовления');
    await detail.expectField('Флаги:', 'Без сахара');
    await detail.expectField('Калории / 100 г:', '54');
    await detail.expectField('Белки:', '12 г / 100 г');
    await detail.expectField('Жиры:', '8 г / 100 г');
    await detail.expectField('Углеводы:', '14 г / 100 г');
    await detail.expectField('Состав:', 'Some ingredients');
    await detail.expectPhotosCount(2);
  });
});

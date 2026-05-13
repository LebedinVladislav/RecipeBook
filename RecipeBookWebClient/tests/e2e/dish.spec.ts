import { expect, test } from './fixtures';
import { DishListPage } from './pages/dish-list.page';
import { DishFormPage } from './pages/dish-form.page';
import { DishDetailPage } from './pages/dish-detail.page';
import { createProduct, createDish } from './helpers/api-helper';

const unique = () => `e2e-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`;

const productFixture = (nameSuffix: string, flags: string[]) => ({
  name: `Dish product ${nameSuffix}`,
  photos: ['https://via.placeholder.com/100'],
  calories: 50,
  proteins: 5,
  fats: 5,
  carbs: 10,
  composition: 'ingredient',
  category: 'Овощи',
  cookingRequired: 'ГотовыйКУпотреблению',
  flags
});

test.afterEach(async ({ cleanupEntities }) => {
  await cleanupEntities();
});

test.describe('Dish management', () => {
  test('should create dish with ingredients, auto nutrition and flags availability', async ({ page, createdProductIds }) => {
    const veganProduct = await createProduct(page.request, productFixture(`${unique()}-vegan`, ['Веган', 'БезГлютена', 'БезСахара']));
    const glutenFreeProduct = await createProduct(page.request, productFixture(`${unique()}-gluten`, ['БезГлютена']));
    createdProductIds.push(veganProduct.id, glutenFreeProduct.id);

    const list = new DishListPage(page);
    const form = new DishFormPage(page);
    const detail = new DishDetailPage(page);

    await list.goto();
    await list.createNew();

    const dishName = `!десерт Автоеда ${unique()}`;
    await form.fillName(dishName);
    await form.fillPortionSize(200);
    await form.selectCategory('Первое');
    await form.addPhotoUrls(['https://via.placeholder.com/1', 'https://via.placeholder.com/2']);

    await form.addIngredient();
    await form.fillIngredient(0, veganProduct.name, 100);
    await form.addIngredient();
    await form.fillIngredient(1, glutenFreeProduct.name, 100);

    const nutrition = await form.getNutritionValues();
    expect(nutrition.calories).toBe('100');
    expect(nutrition.proteins).toBe('10');
    expect(nutrition.fats).toBe('10');
    expect(nutrition.carbs).toBe('20');

    await form.expectFlagEnabled('Веган', false);
    await form.expectFlagEnabled('Без глютена', true);
    await form.expectFlagEnabled('Без сахара', false);
    await form.setFlag('Без глютена');
    await form.submit();

    await list.goto();
    await list.search('Автоеда');
    await expect(list.getDishCard('Автоеда')).toBeVisible();
  });

  test('should prefer manual category selection over macro if both are set', async ({ page, createdProductIds }) => {
    const product = await createProduct(page.request, productFixture(`${unique()}-macro`, ['Веган']));
    createdProductIds.push(product.id);

    const list = new DishListPage(page);
    const form = new DishFormPage(page);
    const detail = new DishDetailPage(page);

    await list.goto();
    await list.createNew();

    await form.fillName('!десерт МакроСуп');
    await form.selectCategory('Первое');
    await form.addIngredient();
    await form.fillIngredient(0, product.name, 100);
    await form.fillPortionSize(150);
    await form.submit();

    const dishName = 'МакроСуп';
    await list.goto();
    await list.search(dishName);
    await list.openDish(dishName);

    await detail.expectField('Категория:', 'Первое');
    await expect(page.locator('h2')).toContainText(dishName);
  });

  test('should edit dish values and display updated data', async ({ page, createdProductIds, createdDishIds }) => {
    const product = await createProduct(page.request, productFixture(`${unique()}-edit`, ['Веган', 'БезГлютена']));
    createdProductIds.push(product.id);

    const dish = await createDish(page.request, {
      name: `Редактировать блюдо ${unique()}`,
      calories: 50,
      proteins: 5,
      fats: 5,
      carbs: 10,
      ingredients: [{ productId: product.id, amount: 100 }],
      portionSize: 120,
      category: 'Салат',
      flags: ['Веган']
    });
    createdDishIds.push(dish.id);

    const list = new DishListPage(page);
    await list.goto();
    await list.search(dish.name);
    await list.getDishCard(dish.name).getByRole('link', { name: 'Редактировать' }).click();

    const form = new DishFormPage(page);
    await form.fillName(`${dish.name} Updated`);
    await form.fillPortionSize(180);
    await form.submit();

    await list.goto();
    await list.search('Updated');
    expect(await list.getDishCard(`${dish.name} Updated`).count()).toBe(1);
  });

  test('should delete dish successfully', async ({ page, createdProductIds, createdDishIds }) => {
    const product = await createProduct(page.request, productFixture(`${unique()}-delete`, ['Веган']));
    createdProductIds.push(product.id);

    const dish = await createDish(page.request, {
      name: `Delete Dish ${unique()}`,
      calories: 100,
      proteins: 10,
      fats: 10,
      carbs: 10,
      ingredients: [{ productId: product.id, amount: 100 }],
      portionSize: 120,
      category: 'Напиток',
      flags: ['Веган']
    });
    createdDishIds.push(dish.id);

    const list = new DishListPage(page);
    await list.goto();
    await list.search(dish.name);
    expect(await list.getDishCard(dish.name).count()).toBe(1);
    await list.deleteDish(dish.name);

    await list.search(dish.name);
    expect(await list.getDishCard(dish.name).count()).toBe(0);
    createdDishIds = createdDishIds.filter((id) => id !== dish.id);
  });

  test('should search and filter dishes correctly', async ({ page, createdProductIds, createdDishIds }) => {
    const product = await createProduct(page.request, productFixture(`${unique()}-filter`, ['БезГлютена', 'БезСахара']));
    createdProductIds.push(product.id);

    const dishA = await createDish(page.request, {
      name: `Search Dish A ${unique()}`,
      calories: 120,
      proteins: 8,
      fats: 8,
      carbs: 20,
      ingredients: [{ productId: product.id, amount: 100 }],
      portionSize: 200,
      category: 'Суп',
      flags: ['БезГлютена']
    });
    const dishB = await createDish(page.request, {
      name: `Search Dish B ${unique()}`,
      calories: 80,
      proteins: 4,
      fats: 2,
      carbs: 14,
      ingredients: [{ productId: product.id, amount: 100 }],
      portionSize: 150,
      category: 'Салат',
      flags: ['БезСахара']
    });
    createdDishIds.push(dishA.id, dishB.id);

    const list = new DishListPage(page);
    await list.goto();
    await list.search('Dish A');
    expect(await list.getDishCard(dishA.name).count()).toBe(1);

    await list.search('');
    await list.selectCategory('Суп');
    expect(await list.getDishCard(dishA.name).count()).toBe(1);
    expect(await list.getDishCard(dishB.name).count()).toBe(0);
  });

  test('should display full dish details and ingredient amounts correctly', async ({ page, createdProductIds, createdDishIds }) => {
    const product = await createProduct(page.request, productFixture(`${unique()}-detail`, ['Веган']));
    createdProductIds.push(product.id);

    const dish = await createDish(page.request, {
      name: `Detail Dish ${unique()}`,
      calories: 77,
      proteins: 7,
      fats: 5,
      carbs: 10,
      ingredients: [{ productId: product.id, amount: 150 }],
      portionSize: 250,
      category: 'Перекус',
      flags: ['Веган']
    });
    createdDishIds.push(dish.id);

    const list = new DishListPage(page);
    await list.goto();
    await list.search(dish.name);
    await list.openDish(dish.name);

    const detail = new DishDetailPage(page);
    await detail.expectField('Категория:', 'Перекус');
    await detail.expectField('Размер порции:', '250 г');
    await detail.expectField('Калории:', '77');
    await detail.expectField('Белки:', '7 г');
    await detail.expectField('Жиры:', '5 г');
    await detail.expectField('Углеводы:', '10 г');
    await detail.expectIngredient(product.name, 150);
  });
});

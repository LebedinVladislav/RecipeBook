import {
  DishCategory,
  DishFlags,
  DishFlag,
  DishIngredientDto,
  NutritionResponse,
  ProductResponse,
  ProductFlag,
  CookingRequired
} from './types';

const macroMap: [RegExp, DishCategory][] = [
  [/!десерт/i, 'Десерт'],
  [/!первое/i, 'Первое'],
  [/!второе/i, 'Второе'],
  [/!напиток/i, 'Напиток'],
  [/!салат/i, 'Салат'],
  [/!суп/i, 'Суп'],
  [/!перекус/i, 'Перекус']
];

const camelCaseWords: Record<string, string> = {
  'ГотовыйКУпотреблению': 'Готовый к употреблению',
  'Полуфабрикат': 'Полуфабрикат',
  'ТребуетПриготовления': 'Требует приготовления'
};

const flagLabelMap: Record<ProductFlag, string> = {
  Веган: 'Веган',
  БезГлютена: 'Без глютена',
  БезСахара: 'Без сахара'
};

export function formatCookingOption(value: CookingRequired | string): string {
  return camelCaseWords[value] || value;
}

export function formatFlagLabel(value: ProductFlag | string): string {
  return flagLabelMap[value as ProductFlag] || value;
}

export function formatFlags(value?: string): string {
  if (!value) {
    return 'нет';
  }
  return value
    .split(',')
    .map((item) => formatFlagLabel(item.trim()))
    .filter(Boolean)
    .join(', ');
}

export function parseDishNameMacro(value: string): { name: string; category: DishCategory | undefined } {
  const macro = macroMap.find(([pattern]) => pattern.test(value));
  if (!macro) {
    return { name: value.trim(), category: undefined };
  }

  const [pattern, category] = macro;
  const cleaned = value.replace(pattern, '').trim().replace(/\s+/g, ' ');
  return { name: cleaned, category };
}

export function parseFlags(value?: string): ProductFlag[] {
  return value
    ? value
        .split(',')
        .map((item) => item.trim())
        .filter((item): item is ProductFlag => item === 'Веган' || item === 'БезГлютена' || item === 'БезСахара')
    : [];
}

export function calculateNutrition(
  ingredients: DishIngredientDto[],
  products: ProductResponse[]
): NutritionResponse {
  const totals = { calories: 0, proteins: 0, fats: 0, carbs: 0 };

  ingredients.forEach((ingredient) => {
    const product = products.find((item) => item.id === ingredient.productId);
    if (!product || ingredient.amount <= 0) {
      return;
    }
    const ratio = ingredient.amount / 100;
    totals.calories += product.calories * ratio;
    totals.proteins += product.proteins * ratio;
    totals.fats += product.fats * ratio;
    totals.carbs += product.carbs * ratio;
  });

  return {
    calories: Number(totals.calories.toFixed(1)),
    proteins: Number(totals.proteins.toFixed(1)),
    fats: Number(totals.fats.toFixed(1)),
    carbs: Number(totals.carbs.toFixed(1))
  };
}

export function getAllowedDishFlags(
  ingredients: DishIngredientDto[],
  products: ProductResponse[]
): DishFlags[] {
  if (!ingredients.length) {
    return ['None'];
  }

  const included = ingredients
    .map((ingredient) => products.find((product) => product.id === ingredient.productId))
    .filter((product): product is ProductResponse => Boolean(product));

  if (!included.length) {
    return ['None'];
  }

  const allVegan = included.every((product) => parseFlags(product.flags).includes('Веган'));
  const allGlutenFree = included.every((product) => parseFlags(product.flags).includes('БезГлютена'));
  const allSugarFree = included.every((product) => parseFlags(product.flags).includes('БезСахара'));

  const flags: DishFlags[] = ['None'];
  if (allVegan) flags.push('Веган');
  if (allGlutenFree) flags.push('БезГлютена');
  if (allSugarFree) flags.push('БезСахара');
  return flags;
}

export function isBjuSumValid(proteins: number, fats: number, carbs: number) {
  return proteins + fats + carbs <= 100;
}

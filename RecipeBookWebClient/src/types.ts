export const productCategories = [
  'Замороженный',
  'Мясной',
  'Овощи',
  'Зелень',
  'Специи',
  'Крупы',
  'Консервы',
  'Жидкость',
  'Сладости'
] as const;

export const cookingOptions = [
  'ГотовыйКУпотреблению',
  'Полуфабрикат',
  'ТребуетПриготовления'
] as const;

export const flagOptions = ['None', 'Веган', 'БезГлютена', 'БезСахара'] as const;
export const singleFlagOptions = ['Веган', 'БезГлютена', 'БезСахара'] as const;

export const dishCategories = [
  'Десерт',
  'Первое',
  'Второе',
  'Напиток',
  'Салат',
  'Суп',
  'Перекус'
] as const;

export type ProductCategory = (typeof productCategories)[number];
export type CookingRequired = (typeof cookingOptions)[number];
export type ProductFlags = (typeof flagOptions)[number];
export type ProductFlag = (typeof singleFlagOptions)[number];
export type DishCategory = (typeof dishCategories)[number];
export type DishFlags = (typeof flagOptions)[number];
export type DishFlag = (typeof singleFlagOptions)[number];

export interface ProductResponse {
  id: number;
  name: string;
  photos: string[];
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  composition?: string | null;
  category: ProductCategory;
  cookingRequired: CookingRequired;
  flags: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProductRequest {
  name: string;
  photos?: string[];
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  composition?: string | null;
  category: ProductCategory;
  cookingRequired: CookingRequired;
  flags?: ProductFlag[];
}

export interface DishIngredientDto {
  productId: number;
  amount: number;
}

export interface DishRequest {
  name: string;
  photos?: string[] | null;
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  ingredients: DishIngredientDto[];
  portionSize: number;
  category?: DishCategory;
  flags?: DishFlag[];
}

export interface DishResponse {
  id: number;
  name: string;
  photos: string[];
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  ingredients: DishIngredientDto[];
  portionSize: number;
  category: DishCategory;
  flags: string;
  createdAt: string;
  updatedAt: string;
}

export interface NutritionResponse {
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
}

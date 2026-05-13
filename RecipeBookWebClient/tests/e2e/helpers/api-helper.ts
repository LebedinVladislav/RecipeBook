import type { APIRequestContext } from '@playwright/test';

export interface ProductRequest {
  name: string;
  photos?: string[];
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  composition?: string;
  category: string;
  cookingRequired: string;
  flags?: string[];
}

export interface DishRequest {
  name: string;
  photos?: string[];
  calories: number;
  proteins: number;
  fats: number;
  carbs: number;
  ingredients: Array<{ productId: number; amount: number }>;
  portionSize: number;
  category?: string;
  flags?: string[];
}

export async function createProduct(request: APIRequestContext, product: ProductRequest) {
  const response = await request.post('/api/Products', { data: product });
  return response.json();
}

export async function deleteProduct(request: APIRequestContext, id: number) {
  await request.delete(`/api/Products/${id}`);
}

export async function createDish(request: APIRequestContext, dish: DishRequest) {
  const response = await request.post('/api/Dishes', { data: dish });
  return response.json();
}

export async function deleteDish(request: APIRequestContext, id: number) {
  await request.delete(`/api/Dishes/${id}`);
}

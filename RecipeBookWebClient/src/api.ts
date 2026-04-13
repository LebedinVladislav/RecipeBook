import axios from 'axios';
import {
  CookingRequired,
  DishCategory,
  DishFlags,
  DishIngredientDto,
  DishRequest,
  DishResponse,
  NutritionResponse,
  ProductCategory,
  ProductFlags,
  ProductRequest,
  ProductResponse
} from './types';

function serializeFlags(flags: string[] | undefined) {
  if (!flags?.length) {
    return undefined;
  }
  return flags.join(', ');
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || ''
});

export async function getProducts(params: {
  category?: ProductCategory;
  cookingRequired?: CookingRequired;
  flags?: ProductFlags;
  search?: string;
  sortBy?: 'Name' | 'Calories' | 'Proteins' | 'Fats' | 'Carbs';
}) {
  const response = await api.get<ProductResponse[]>('/api/Products', { params });
  return response.data;
}

export async function getProduct(id: number) {
  const response = await api.get<ProductResponse>(`/api/Products/${id}`);
  return response.data;
}

export async function createProduct(product: ProductRequest) {
  const payload = {
    ...product,
    flags: serializeFlags(product.flags)
  };
  const response = await api.post<ProductResponse>('/api/Products', payload);
  return response.data;
}

export async function updateProduct(id: number, product: ProductRequest) {
  const payload = {
    ...product,
    flags: serializeFlags(product.flags)
  };
  await api.put(`/api/Products/${id}`, payload);
}

export async function deleteProduct(id: number) {
  await api.delete(`/api/Products/${id}`);
}

export async function getDishes(params: {
  category?: DishCategory;
  flags?: DishFlags;
  search?: string;
}) {
  const response = await api.get<DishResponse[]>('/api/Dishes', { params });
  return response.data;
}

export async function getDish(id: number) {
  const response = await api.get<DishResponse>(`/api/Dishes/${id}`);
  return response.data;
}

export async function createDish(dish: DishRequest) {
  const payload = {
    ...dish,
    flags: serializeFlags(dish.flags)
  };
  const response = await api.post<DishResponse>('/api/Dishes', payload);
  return response.data;
}

export async function updateDish(id: number, dish: DishRequest) {
  const payload = {
    ...dish,
    flags: serializeFlags(dish.flags)
  };
  await api.put(`/api/Dishes/${id}`, payload);
}

export async function deleteDish(id: number) {
  await api.delete(`/api/Dishes/${id}`);
}

export async function calculateNutritionOnServer(ingredients: DishIngredientDto[]) {
  const response = await api.post<NutritionResponse>('/api/Dishes/calculate-nutrition', {
    ingredients
  });
  return response.data;
}

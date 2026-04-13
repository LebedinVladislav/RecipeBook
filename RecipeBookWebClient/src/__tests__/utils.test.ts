import { describe, expect, it } from 'vitest';
import { calculateNutrition, parseDishNameMacro } from '../utils';
import type { ProductResponse } from '../types';

describe('utils', () => {
  it('parses dish macro and removes macro from name', () => {
    const result = parseDishNameMacro('Мой рецепт !десерт для всех');
    expect(result.name).toBe('Мой рецепт для всех');
    expect(result.category).toBe('Десерт');
  });

  it('calculates nutrition based on product amounts', () => {
    const products: ProductResponse[] = [
      {
        id: 1,
        name: 'Яблоко',
        photos: [],
        calories: 52,
        proteins: 0.3,
        fats: 0.2,
        carbs: 14,
        category: 'Овощи',
        cookingRequired: 'ГотовыйКУпотреблению',
        flags: 'Веган',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z'
      },
      {
        id: 2,
        name: 'Творог',
        photos: [],
        calories: 98,
        proteins: 16.5,
        fats: 4.5,
        carbs: 3.2,
        category: 'Мясной',
        cookingRequired: 'ГотовыйКУпотреблению',
        flags: 'None',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z'
      }
    ];

    const result = calculateNutrition([
      { productId: 1, amount: 150 },
      { productId: 2, amount: 100 }
    ], products);

    expect(result.calories).toBeCloseTo(177, 0);
    expect(result.proteins).toBeCloseTo(16.95, 2);
    expect(result.fats).toBeCloseTo(4.8, 2);
    expect(result.carbs).toBeCloseTo(24.2, 2);
  });
});

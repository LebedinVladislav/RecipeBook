import { APIRequestContext, expect as baseExpect, test as baseTest } from '@playwright/test';
import { deleteDish, deleteProduct } from './helpers/api-helper';

export const test = baseTest.extend<{
  createdProductIds: number[];
  createdDishIds: number[];
  cleanupEntities: () => Promise<void>;
}>({
  createdProductIds: async ({}, use) => {
    const ids: number[] = [];
    await use(ids);
  },
  createdDishIds: async ({}, use) => {
    const ids: number[] = [];
    await use(ids);
  },
  cleanupEntities: async ({ page, createdProductIds, createdDishIds }, use) => {
    await use(async () => {
      for (const id of createdDishIds.splice(0)) {
        try {
          await deleteDish(page.request, id);
        } catch {
          // ignore cleanup failures
        }
      }
      for (const id of createdProductIds.splice(0)) {
        try {
          await deleteProduct(page.request, id);
        } catch {
          // ignore cleanup failures
        }
      }
    });
  }
});

export const expect = baseExpect;

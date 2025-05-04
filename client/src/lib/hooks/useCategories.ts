import { useQuery } from '@tanstack/react-query';
import { CategoryResponseDto } from '../types';
import { customFetch } from '../customFetch';

export const useCategories = (categoryId?: number) => {
  const { data: categories, isLoading: loadingCategories } = useQuery({
    queryKey: ['categories'],
    queryFn: async () => {
      const data = await customFetch<CategoryResponseDto[]>('/categories');
      return data;
    },
  });

  const { data: category, isLoading: loadingCategory } = useQuery({
    queryKey: ['categories', categoryId],
    queryFn: async () => {
      if (!categoryId) return null;
      const data = await customFetch<CategoryResponseDto>(
        `/categories/${categoryId}`
      );
      return data;
    },
    enabled: !!categoryId,
  });

  return {
    // List categories
    categories,
    loadingCategories,

    // Get category by ID
    category,
    loadingCategory,
  };
};

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { CategoryResponseDto } from '../types';
import { customFetch } from '../customFetch';
import {
  CategoryWithoutChildResponseDto,
  CreateCategoryRequestDto,
  CreateSubcategoryRequestDto,
  EditCategoryRequestDto,
  EditSubcategoryRequestDto,
  SubcategoryResponseDto,
} from '../types';

export const useCategories = (categoryId?: number, subcategoryId?: number) => {
  const queryClient = useQueryClient();

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

  const { data: subcategory, isLoading: loadingSubcategory } = useQuery({
    queryKey: ['subcategories', subcategoryId],
    queryFn: async () => {
      if (!subcategoryId) return null;
      const data = await customFetch<SubcategoryResponseDto>(
        `/categories/subcategories/${subcategoryId}`
      );
      return data;
    },
    enabled: !!subcategoryId,
  });

  const createCategory = useMutation({
    mutationFn: async (categoryData: CreateCategoryRequestDto) => {
      return await customFetch<CategoryWithoutChildResponseDto>('/categories', {
        method: 'POST',
        body: JSON.stringify(categoryData),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  const editCategory = useMutation({
    mutationFn: async ({
      id,
      categoryData,
    }: {
      id: number;
      categoryData: EditCategoryRequestDto;
    }) => {
      return await customFetch<CategoryWithoutChildResponseDto>(
        `/categories/${id}`,
        {
          method: 'PUT',
          body: JSON.stringify(categoryData),
        }
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      queryClient.invalidateQueries({ queryKey: ['categories', variables.id] });
    },
  });

  const deleteCategory = useMutation({
    mutationFn: async (id: number) => {
      await customFetch(`/categories/${id}`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  const createSubcategory = useMutation({
    mutationFn: async (subcategoryData: CreateSubcategoryRequestDto) => {
      return await customFetch<SubcategoryResponseDto>(
        '/categories/subcategories',
        {
          method: 'POST',
          body: JSON.stringify(subcategoryData),
        }
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      if (variables.categoryId) {
        queryClient.invalidateQueries({
          queryKey: ['categories', variables.categoryId],
        });
      }
    },
  });

  const editSubcategory = useMutation({
    mutationFn: async ({
      id,
      subcategoryData,
    }: {
      id: number;
      subcategoryData: EditSubcategoryRequestDto;
    }) => {
      return await customFetch<SubcategoryResponseDto>(
        `/categories/subcategories/${id}`,
        {
          method: 'PUT',
          body: JSON.stringify(subcategoryData),
        }
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      queryClient.invalidateQueries({
        queryKey: ['subcategories', variables.id],
      });
      if (variables.subcategoryData.categoryId) {
        queryClient.invalidateQueries({
          queryKey: ['categories', variables.subcategoryData.categoryId],
        });
      }
    },
  });

  const deleteSubcategory = useMutation({
    mutationFn: async (id: number) => {
      await customFetch(`/categories/subcategories/${id}`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      queryClient.invalidateQueries({ queryKey: ['subcategories'] });
    },
  });

  return {
    // Categories queries
    categories,
    loadingCategories,
    category,
    loadingCategory,

    // Subcategories queries
    subcategory,
    loadingSubcategory,

    // Categories mutations
    createCategory,
    editCategory,
    deleteCategory,

    // Subcategories mutations
    createSubcategory,
    editSubcategory,
    deleteSubcategory,
  };
};

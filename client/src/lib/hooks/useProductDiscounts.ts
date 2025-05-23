// filepath: /home/hy/repos/Ecommerce/client/src/lib/hooks/useProductDiscounts.ts
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  AddProductDiscountRequestDto,
  EditProductDiscountRequestDto,
  ProductDiscountResponseDto,
} from '../types';

export const useProductDiscounts = (
  productId?: number,
  discountId?: number
) => {
  const queryClient = useQueryClient();

  // Get all discounts for a product
  const { data: productDiscounts, isLoading: loadingProductDiscounts } =
    useQuery({
      queryKey: ['productDiscounts', productId],
      queryFn: async () => {
        if (!productId) return [];
        return await customFetch<ProductDiscountResponseDto[]>(
          `/products/${productId}/discounts`
        );
      },
      enabled: !!productId,
    });

  // Get a specific discount for a product
  const { data: productDiscount, isLoading: loadingProductDiscount } = useQuery(
    {
      queryKey: ['productDiscount', productId, discountId],
      queryFn: async () => {
        if (!productId) return null;
        return await customFetch<ProductDiscountResponseDto>(
          `/products/${productId}/discounts/${discountId}`
        );
      },
      enabled: !!productId && !!discountId,
    }
  );

  // Add a new discount to a product
  const addProductDiscount = useMutation({
    mutationFn: async ({
      productId,
      discountData,
    }: {
      productId: number;
      discountData: AddProductDiscountRequestDto;
    }) => {
      return await customFetch<ProductDiscountResponseDto>(
        `/products/${productId}/discounts`,
        {
          method: 'POST',
          body: JSON.stringify(discountData),
        }
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({
        queryKey: ['productDiscounts', variables.productId],
      });
    },
  });

  // Edit an existing product discount
  const editProductDiscount = useMutation({
    mutationFn: async ({
      productId,
      discountId,
      discountData,
    }: {
      productId: number;
      discountId: number;
      discountData: EditProductDiscountRequestDto;
    }) => {
      return await customFetch<ProductDiscountResponseDto>(
        `/products/${productId}/discounts/${discountId}`,
        {
          method: 'PUT',
          body: JSON.stringify(discountData),
        }
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({
        queryKey: ['productDiscounts', variables.productId],
      });
      queryClient.invalidateQueries({
        queryKey: [
          'productDiscount',
          variables.productId,
          variables.discountId,
        ],
      });
    },
  });

  // Delete a product discount
  const deleteProductDiscount = useMutation({
    mutationFn: async ({
      productId,
      discountId,
    }: {
      productId: number;
      discountId: number;
    }) => {
      await customFetch(`/products/${productId}/discounts/${discountId}`, {
        method: 'DELETE',
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
      queryClient.invalidateQueries({
        queryKey: ['productDiscounts', variables.productId],
      });
    },
  });

  return {
    // Queries
    productDiscounts,
    loadingProductDiscounts,
    productDiscount,
    loadingProductDiscount,

    // Mutations
    addProductDiscount,
    editProductDiscount,
    deleteProductDiscount,
  };
};

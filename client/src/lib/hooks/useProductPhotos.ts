import { useMutation, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import { ProductPhotoDto, UpdateProductPhotoDisplayOrderRequestDto } from '../types';

export const useProductPhotos = () => {
  const queryClient = useQueryClient();

  // Add a photo to a product
  const addProductPhoto = useMutation({
    mutationFn: async ({
      productId,
      file,
    }: {
      productId: number;
      file: File;
    }) => {
      const formData = new FormData();
      formData.append('file', file);

      return await customFetch<ProductPhotoDto>(`/products/${productId}/photos`, {
        method: 'POST',
        body: formData,
        // Don't set content type; browser will set it with proper boundary
        headers: {},
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
    },
  });

  // Delete a photo from a product
  const deleteProductPhoto = useMutation({
    mutationFn: async ({
      productId,
      photoKey,
    }: {
      productId: number;
      photoKey: string;
    }) => {
      await customFetch(`/products/${productId}/photos?photoKey=${encodeURIComponent(photoKey)}`, {
        method: 'DELETE',
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
    },
  });

  // Update the display order of product photos
  const updatePhotoDisplayOrder = useMutation({
    mutationFn: async ({
      productId,
      photoOrders,
    }: {
      productId: number;
      photoOrders: UpdateProductPhotoDisplayOrderRequestDto[];
    }) => {
      await customFetch(`/products/${productId}/photos/order`, {
        method: 'PUT',
        body: JSON.stringify(photoOrders),
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['product', variables.productId],
      });
    },
  });

  return {
    addProductPhoto,
    deleteProductPhoto,
    updatePhotoDisplayOrder,
  };
};
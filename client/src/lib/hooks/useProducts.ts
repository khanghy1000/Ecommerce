import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  CreateProductRequestDto,
  EditProductRequestDto,
  ListProductsRequest,
  PagedList,
  PopularProductResponseDto,
  ProductResponseDto,
} from '../types';
import queryString from 'query-string';

export const useProducts = (
  productId?: number,
  listProductsRequest?: ListProductsRequest
) => {
  const queryClient = useQueryClient();
  
  const { data: products, isLoading: loadingProducts } = useQuery({
    queryKey: ['products', listProductsRequest],
    queryFn: async () => {
      let url = `/products`;

      if (listProductsRequest) {
        const stringifiedParams = queryString.stringify(listProductsRequest, {
          arrayFormat: 'none',
        });
        url = `${url}?${stringifiedParams}`;
      }

      return await customFetch<PagedList<ProductResponseDto>>(url);
    },
    enabled: !productId,
  });

  const { data: product, isLoading: loadingProduct } = useQuery({
    queryKey: ['product', productId],
    queryFn: async () => {
      if (!productId) return null;
      return await customFetch<ProductResponseDto>(`/products/${productId}`);
    },
    enabled: !!productId,
  });

  const { data: popularProducts, isLoading: loadingPopularProducts } = useQuery(
    {
      queryKey: ['popularProducts'],
      queryFn: async () => {
        return await customFetch<PopularProductResponseDto[]>(
          '/products/popular'
        );
      },
      enabled: !productId,
    }
  );

  // Create a new product
  const createProduct = useMutation({
    mutationFn: async (productData: CreateProductRequestDto) => {
      return await customFetch<ProductResponseDto>('/products', {
        method: 'POST',
        body: JSON.stringify(productData),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['products'],
      });
    },
  });

  // Edit an existing product
  const editProduct = useMutation({
    mutationFn: async ({
      id,
      productData,
    }: {
      id: number;
      productData: EditProductRequestDto;
    }) => {
      return await customFetch<ProductResponseDto>(`/products/${id}`, {
        method: 'PUT',
        body: JSON.stringify(productData),
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['products'],
      });
      queryClient.invalidateQueries({
        queryKey: ['product', variables.id],
      });
    },
  });

  // Set product active state
  const setProductActiveState = useMutation({
    mutationFn: async ({ id, isActive }: { id: number; isActive: boolean }) => {
      return await customFetch(`/products/${id}/active?isActive=${isActive}`, {
        method: 'PUT',
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['products'],
      });
      queryClient.invalidateQueries({
        queryKey: ['product', variables.id],
      });
    },
  });

  // Delete a product
  const deleteProduct = useMutation({
    mutationFn: async (id: number) => {
      await customFetch(`/products/${id}`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['products'],
      });
    },
  });

  return {
    // List products
    products,
    loadingProducts,

    // Single product
    product,
    loadingProduct,

    // Popular products
    popularProducts,
    loadingPopularProducts,

    // Mutations
    createProduct,
    editProduct,
    setProductActiveState,
    deleteProduct,
  };
};

import { useQuery } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
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
  const { data: products, isLoading: loadingProducts } = useQuery({
    queryKey: ['products', listProductsRequest],
    queryFn: async () => {
      let url = `/products`;

      if (listProductsRequest) {
        const stringifiedParams = queryString.stringify(listProductsRequest, {
          arrayFormat: 'comma',
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
  };
};

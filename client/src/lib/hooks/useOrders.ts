import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  CheckoutPricePreviewRequestDto,
  CheckoutPricePreviewResponseDto,
  CheckoutRequestDto,
  CheckoutResponseDto,
  ListOrdersRequest,
  PagedList,
  SalesOrderResponseDto,
} from '../types';
import queryString from 'query-string';

export const useOrders = (
  orderId?: number,
  listOrdersRequest?: ListOrdersRequest
) => {
  const queryClient = useQueryClient();

  // Get list of orders with filtering
  const { data: orders, isLoading: loadingOrders } = useQuery({
    queryKey: ['orders', listOrdersRequest],
    queryFn: async () => {
      let url = '/orders';

      if (listOrdersRequest) {
        const stringifiedParams = queryString.stringify(listOrdersRequest, {
          arrayFormat: 'none',
        });
        url = `${url}?${stringifiedParams}`;
      }

      return await customFetch<PagedList<SalesOrderResponseDto>>(url);
    },
    enabled: !orderId,
  });

  // Get single order by ID
  const { data: order, isLoading: loadingOrder } = useQuery({
    queryKey: ['order', orderId],
    queryFn: async () => {
      if (!orderId) return null;
      return await customFetch<SalesOrderResponseDto>(`/orders/${orderId}`);
    },
    enabled: !!orderId,
  });

  // Checkout process
  const checkout = useMutation({
    mutationFn: async (checkoutData: CheckoutRequestDto) => {
      return await customFetch<CheckoutResponseDto>('/orders/checkout', {
        method: 'POST',
        body: JSON.stringify(checkoutData),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['cart'],
      });
      queryClient.invalidateQueries({
        queryKey: ['orders'],
      });
    },
  });

  // Preview checkout prices
  const checkoutPreview = useMutation({
    mutationFn: async (previewData: CheckoutPricePreviewRequestDto) => {
      return await customFetch<CheckoutPricePreviewResponseDto>(
        '/orders/checkout-preview',
        {
          method: 'POST',
          body: JSON.stringify(previewData),
        }
      );
    },
  });

  // Confirm order
  const confirmOrder = useMutation({
    mutationFn: async (id: number) => {
      return await customFetch<SalesOrderResponseDto>(`/orders/${id}/confirm`, {
        method: 'POST',
      });
    },
    onSuccess: (_, orderId) => {
      queryClient.invalidateQueries({
        queryKey: ['order', orderId],
      });
      queryClient.invalidateQueries({
        queryKey: ['orders'],
      });
    },
  });

  // Cancel order
  const cancelOrder = useMutation({
    mutationFn: async (id: number) => {
      return await customFetch<SalesOrderResponseDto>(`/orders/${id}/cancel`, {
        method: 'POST',
      });
    },
    onSuccess: (_, orderId) => {
      queryClient.invalidateQueries({
        queryKey: ['order', orderId],
      });
      queryClient.invalidateQueries({
        queryKey: ['orders'],
      });
    },
  });

  return {
    // List orders
    orders,
    loadingOrders,

    // Single order
    order,
    loadingOrder,

    // Mutations
    checkout,
    checkoutPreview,
    confirmOrder,
    cancelOrder,
  };
};

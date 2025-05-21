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
  listOrdersRequest?: ListOrdersRequest,
  checkoutPreviewRequest?: CheckoutPricePreviewRequestDto
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
    enabled: !orderId && !checkoutPreviewRequest,
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

  const {
    data: checkoutPreview,
    isLoading: loadingCheckoutPreview,
    isFetching: fetchingCheckoutPreview,
  } = useQuery({
    queryKey: ['checkoutPreview', checkoutPreviewRequest],
    retry: false,
    queryFn: async () => {
      if (!checkoutPreviewRequest) return null;
      return await customFetch<CheckoutPricePreviewResponseDto>(
        '/orders/checkout-preview',
        {
          method: 'POST',
          body: JSON.stringify(checkoutPreviewRequest),
        }
      );
    },
    enabled: !!checkoutPreviewRequest,
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

    confirmOrder,
    cancelOrder,

    // Checkout
    checkoutPreview,
    loadingCheckoutPreview,
    fetchingCheckoutPreview,
    checkout,
  };
};

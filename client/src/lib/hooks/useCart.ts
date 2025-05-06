import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AddToCartRequestDto,
  CartItemResponseDto,
  RemoveFromCartRequestDto,
  UpdateCartItemRequestDto,
} from '../types';
import { customFetch } from '../customFetch';

export const useCart = () => {
  const queryClient = useQueryClient();

  // Get cart items
  const { data: cartItems, isLoading: isLoadingCart } = useQuery({
    queryKey: ['cart'],
    queryFn: async () => {
      const data = await customFetch<CartItemResponseDto[]>('/cart');
      return data;
    },
  });

  // Add item to cart
  const addToCart = useMutation({
    mutationFn: async (item: AddToCartRequestDto) => {
      await customFetch('/cart', {
        method: 'POST',
        body: JSON.stringify(item),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['cart'],
      });
    },
  });

  // Update cart item quantity
  const updateCartItem = useMutation({
    mutationFn: async (item: UpdateCartItemRequestDto) => {
      await customFetch('/cart', {
        method: 'PUT',
        body: JSON.stringify(item),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['cart'],
      });
    },
  });

  // Remove item from cart
  const removeFromCart = useMutation({
    mutationFn: async (item: RemoveFromCartRequestDto) => {
      await customFetch('/cart', {
        method: 'DELETE',
        body: JSON.stringify(item),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['cart'],
      });
    },
  });

  // Calculate cart summary
  const cartSummary = cartItems
    ? {
        totalItems: cartItems.reduce((sum, item) => sum + item.quantity, 0),
        subtotal: cartItems.reduce((sum, item) => sum + item.subtotal, 0),
        totalProducts: cartItems.length,
      }
    : { totalItems: 0, subtotal: 0, totalProducts: 0 };

  return {
    cartItems,
    isLoadingCart,
    addToCart,
    updateCartItem,
    removeFromCart,
    cartSummary,
  };
};

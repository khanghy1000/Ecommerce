import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AddToCartRequestDto,
  CartItemResponseDto,
  RemoveFromCartRequestDto,
  UpdateCartItemRequestDto,
} from '../types';
import { customFetch } from '../customFetch';
import { useAccount } from './useAccount';

export const useCart = () => {
  const queryClient = useQueryClient();
  const { currentUserInfo } = useAccount();

  // Get cart items
  const { data: cartItems, isLoading: isLoadingCart } = useQuery({
    queryKey: ['cart'],
    queryFn: async () => {
      const data = await customFetch<CartItemResponseDto[]>('/cart');
      return data;
    },
    enabled: !!currentUserInfo && currentUserInfo.role === 'Buyer',
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
    onMutate: async (newItem) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['cart'] });

      // Snapshot the previous value
      const previousCartItems = queryClient.getQueryData<CartItemResponseDto[]>(
        ['cart']
      );

      // Optimistically update the cache with the new value
      if (previousCartItems) {
        const updatedCartItems = previousCartItems.map((cartItem) => {
          if (cartItem.productId === newItem.productId) {
            // Calculate new subtotal
            const unitPrice = cartItem.discountPrice ?? cartItem.unitPrice;
            return {
              ...cartItem,
              quantity: newItem.quantity,
              subtotal: unitPrice * newItem.quantity,
            };
          }
          return cartItem;
        });

        queryClient.setQueryData(['cart'], updatedCartItems);
      }

      // Return the snapshot to rollback if the mutation fails
      return { previousCartItems };
    },
    onError: (_err, _newItem, context) => {
      // If the mutation fails, use the context returned above
      if (context?.previousCartItems) {
        queryClient.setQueryData(['cart'], context.previousCartItems);
      }
    },
    onSettled: () => {
      // Always refetch after error or success
      queryClient.invalidateQueries({ queryKey: ['cart'] });
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

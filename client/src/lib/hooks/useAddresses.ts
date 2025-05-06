import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  AddUserAddressRequestDto,
  EditUserAddressRequestDto,
  UserAddressResponseDto,
} from '../types';

export const useAddresses = (addressId?: number) => {
  const queryClient = useQueryClient();

  // Get all user addresses
  const { data: addresses, isLoading: loadingAddresses } = useQuery({
    queryKey: ['addresses'],
    queryFn: async () => {
      return await customFetch<UserAddressResponseDto[]>('/users/addresses');
    },
  });

  // Get a single address by ID
  const { data: address, isLoading: loadingAddress } = useQuery({
    queryKey: ['address', addressId],
    queryFn: async () => {
      if (!addressId) return null;
      return await customFetch<UserAddressResponseDto>(`/users/addresses/${addressId}`);
    },
    enabled: !!addressId,
  });

  // Add a new address
  const addAddress = useMutation({
    mutationFn: async (addressData: AddUserAddressRequestDto) => {
      return await customFetch<UserAddressResponseDto>('/users/addresses', {
        method: 'POST',
        body: JSON.stringify(addressData),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['addresses'],
      });
    },
  });

  // Set an address as default
  const setDefaultAddress = useMutation({
    mutationFn: async (id: number) => {
      return await customFetch<UserAddressResponseDto>(`/users/addresses/default/${id}`, {
        method: 'PUT',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['addresses'],
      });
    },
  });

  // Edit an existing address
  const editAddress = useMutation({
    mutationFn: async ({
      id,
      addressData,
    }: {
      id: number;
      addressData: EditUserAddressRequestDto;
    }) => {
      return await customFetch<UserAddressResponseDto>(`/users/addresses/${id}`, {
        method: 'PUT',
        body: JSON.stringify(addressData),
      });
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['addresses'],
      });
      queryClient.invalidateQueries({
        queryKey: ['address', variables.id],
      });
    },
  });

  // Delete an address
  const deleteAddress = useMutation({
    mutationFn: async (id: number) => {
      return await customFetch<void>(`/users/addresses/${id}`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['addresses'],
      });
    },
  });

  return {
    addresses,
    loadingAddresses,
    address,
    loadingAddress,

    addAddress,
    setDefaultAddress,
    editAddress,
    deleteAddress,
  };
};
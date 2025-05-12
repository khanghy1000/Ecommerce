import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  CouponResponseDto,
  CreateCouponRequestDto,
  EditCouponRequestDto,
} from '../types';

export const useCoupons = () => {
  const queryClient = useQueryClient();

  const { data: coupons, isLoading: loadingCoupons } = useQuery({
    queryKey: ['coupons'],
    queryFn: async () => {
      return await customFetch<CouponResponseDto[]>('/coupons');
    },
  });

  const createCoupon = useMutation({
    mutationFn: async (couponRequest: CreateCouponRequestDto) => {
      return await customFetch<CouponResponseDto>('/coupons', {
        method: 'POST',
        body: JSON.stringify(couponRequest),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['coupons'],
      });
    },
  });

  const editCoupon = useMutation({
    mutationFn: async ({
      code,
      couponRequest,
    }: {
      code: string;
      couponRequest: EditCouponRequestDto;
    }) => {
      return await customFetch<CouponResponseDto>(`/coupons/${code}`, {
        method: 'PUT',
        body: JSON.stringify(couponRequest),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['coupons'],
      });
    },
  });

  const deleteCoupon = useMutation({
    mutationFn: async (code: string) => {
      await customFetch(`/coupons/${code}`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['coupons'],
      });
    },
  });

  return {
    coupons,
    loadingCoupons,
    createCoupon,
    editCoupon,
    deleteCoupon,
  };
};

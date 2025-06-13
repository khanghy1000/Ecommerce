import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  CouponResponseDto,
  CreateCouponRequestDto,
  EditCouponRequestDto,
} from '../types';
import queryString from 'query-string';

export const useCoupons = (
  orderSubtotal?: number,
  productCategoryIds?: number[]
) => {
  const queryClient = useQueryClient();

  const { data: coupons, isLoading: loadingCoupons } = useQuery({
    queryKey: ['coupons'],
    queryFn: async () => {
      return await customFetch<CouponResponseDto[]>('/coupons');
    },
  });

  const { data: applicableCoupons, isLoading: loadingApplicableCoupons } =
    useQuery({
      queryKey: ['coupons', 'applicable', orderSubtotal, productCategoryIds],
      queryFn: async () => {
        if (orderSubtotal === undefined || !productCategoryIds?.length)
          return [];

        let url = '/coupons/valid';
        const stringifiedParams = queryString.stringify(
          { orderSubtotal, productCategoryIds },
          {
            arrayFormat: 'none',
          }
        );
        url = `${url}?${stringifiedParams}`;

        return await customFetch<CouponResponseDto[]>(url);
      },
      enabled:
        !!orderSubtotal &&
        productCategoryIds != null &&
        productCategoryIds.length > 0,
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

    applicableCoupons,
    loadingApplicableCoupons,

    createCoupon,
    editCoupon,
    deleteCoupon,
  };
};

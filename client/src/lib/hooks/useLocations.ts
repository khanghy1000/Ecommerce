import { useQuery } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import { Province, District, Ward } from '../types';

export const useLocations = (provinceId?: number, districtId?: number) => {
  const { data: provinces, isLoading: loadingProvinces } = useQuery({
    queryKey: ['provinces'],
    queryFn: async () => {
      return await customFetch<Province[]>('/locations/provinces');
    },
  });

  const { data: districts = [], isLoading: loadingDistricts } = useQuery({
    queryKey: ['districts', provinceId],
    queryFn: async () => {
      if (!provinceId) return [];
      return await customFetch<District[]>(
        `/locations/districts?provinceId=${provinceId}`
      );
    },
    enabled: !!provinceId,
  });

  const { data: wards = [], isLoading: loadingWards } = useQuery({
    queryKey: ['wards', districtId],
    queryFn: async () => {
      if (!districtId) return [];
      return await customFetch<Ward[]>(
        `/locations/wards?districtId=${districtId}`
      );
    },
    enabled: !!districtId,
  });

  return {
    provinces,
    districts,
    wards,
    loadingProvinces,
    loadingDistricts,
    loadingWards,
  };
};

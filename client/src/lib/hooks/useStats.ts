import { useQuery } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import {
  ShopOrderStatsResponseDto,
  ShopPerformanceRequest,
  ShopPerformanceResponseDto,
} from '../types';
import queryString from 'query-string';

export const useStats = (
  shopPerformanceRequest?: ShopPerformanceRequest,
  shopSummaryRequestId?: string
) => {
  const { data: shopPerformance } = useQuery({
    queryKey: ['shopPerformance', shopPerformanceRequest],
    queryFn: async () => {
      const stringifiedParams = queryString.stringify(shopPerformanceRequest!);
      const url = `/stats/shop-performance?${stringifiedParams}`;

      return await customFetch<ShopPerformanceResponseDto[]>(url);
    },
    enabled: !!shopPerformanceRequest,
  });

  const { data: shopSummary } = useQuery({
    queryKey: ['shopSummary', shopSummaryRequestId],
    queryFn: async () => {
      const url = `/stats/shop-summary?shopId=${shopSummaryRequestId}`;
      return await customFetch<ShopOrderStatsResponseDto>(url);
    },
    enabled: !!shopSummaryRequestId,
  });

  return {
    shopPerformance,
    shopSummary,
  };
};

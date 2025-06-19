import { useQuery } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import { PaymentResponse } from '../types';

export const usePayment = (paymentId?: string) => {
  const { data: paymentInfo, isLoading: loadingPaymentInfo } = useQuery({
    queryKey: ['payment', paymentId],
    queryFn: async () => {
      if (!paymentId || paymentId === '0') {
        return null;
      }
      const data = await customFetch<PaymentResponse>(`/payment/${paymentId}`);
      return data;
    },
    enabled: !!paymentId,
  });

  return {
    paymentInfo,
    loadingPaymentInfo,
  };
};

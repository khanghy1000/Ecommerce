import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import { LoginRequest, UserInfoResponse } from '../types';

const useAccount = () => {
  const queryClient = useQueryClient();

  const { data: userInfo, isLoading: isLoadingUserInfo } = useQuery({
    queryKey: ['user'],
    queryFn: async () => {
      const data = await customFetch<UserInfoResponse>('/user-info');
      return data;
    },
    enabled: !queryClient.getQueryData(['user']),
  });

  const loginUser = useMutation({
    mutationFn: async (creds: LoginRequest) => {
      await customFetch('/login?useCookies=true', {
        body: JSON.stringify(creds),
        method: 'POST',
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['user'],
      });
    },
  });

  return {
    userInfo,
    isLoadingUserInfo,
    loginUser,
  };
};

export default useAccount;

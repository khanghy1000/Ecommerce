import { useQueryClient, useQuery, useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import { UserInfoResponse } from '../types';
import { customFetch } from '../customFetch';
import { LoginRequest } from '../types';
import { RegisterRequest } from '../types';
import { ChangePasswordRequest } from '../types';

export const useAccount = () => {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data: currentUserInfo, isLoading: loadingUserInfo } = useQuery({
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
        method: 'POST',
        body: JSON.stringify(creds),
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['user'],
      });
    },
  });

  const registerUser = useMutation({
    mutationFn: async (creds: RegisterRequest) => {
      await customFetch('/register', {
        method: 'POST',
        body: JSON.stringify(creds),
      });
    },
  });

  const logoutUser = useMutation({
    mutationFn: async () => {
      await customFetch('/logout', {
        method: 'POST',
      });
    },
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: ['user'] });
      navigate('/');
    },
  });

  const changePassword = useMutation({
    mutationFn: async (data: ChangePasswordRequest) => {
      await customFetch('/change-password', {
        method: 'POST',
        body: JSON.stringify(data),
      });
    },
  });

  const updateProfile = useMutation({
    mutationFn: async (data: { displayName: string }) => {
      await customFetch('/users/profile', {
        method: 'PUT',
        body: JSON.stringify(data),
      });
    },
    onSuccess: async () => {
      queryClient.removeQueries({ queryKey: ['user'] });
      navigate('/profile');
    },
  });

  const updateUserImage = useMutation({
    mutationFn: async (file: File) => {
      const formData = new FormData();
      formData.append('file', file);

      await customFetch('/users/image', {
        method: 'PUT',
        body: formData,
        headers: {},
      });
    },
    onSuccess: async () => {
      queryClient.removeQueries({ queryKey: ['user'] });
      navigate('/profile');
    },
  });

  return {
    currentUserInfo,
    loadingUserInfo,

    loginUser,
    logoutUser,
    registerUser,
    changePassword,

    updateProfile,
    updateUserImage,
  };
};

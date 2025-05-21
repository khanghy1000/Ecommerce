import { router } from '../router';
import { useAppStore } from './hooks/useAppStore';
import { ErrorResponse } from './types';
import { notifications } from '@mantine/notifications';

const baseUrl =
  import.meta.env.VITE_BASE_API_URL || 'http://localhost:5213/api';

const getBody = <T>(c: Response | Request): Promise<T> => {
  const contentType = c.headers.get('content-type');

  if (contentType && contentType.includes('application/json')) {
    return c.json() as Promise<T>;
  }

  if (contentType && contentType.includes('application/problem+json')) {
    return c.json() as Promise<T>;
  }

  if (contentType && contentType.includes('application/pdf')) {
    return c.blob() as Promise<T>;
  }

  return c.text() as Promise<T>;
};

async function sleep(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function customFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const defaultOptions: RequestInit = {
    method: 'GET',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
  };
  const finalOptions = { ...defaultOptions, ...options };

  const { addLoading, removeLoading } = useAppStore.getState();
  addLoading();

  if (import.meta.env.DEV) {
    // await sleep(1000);
  }

  try {
    const response = await fetch(baseUrl + path, finalOptions);
    let data = await getBody<T | ErrorResponse>(response);

    if (!response.ok) {
      const status = response.status;
      switch (status) {
        case 400:
          data = data as ErrorResponse;
          if (data.errors) {
            const modalStateErrors = [];
            for (const key in data.errors) {
              if (data.errors[key]) {
                modalStateErrors.push(data.errors[key]);
              }
            }
            throw modalStateErrors.flat();
          } else {
            notifications.show({
              color: 'red',
              title: 'Error',
              message: data.detail ?? JSON.stringify(data),
            });
          }
          break;
        case 401:
          data = data as ErrorResponse;
          if (data.detail === 'NotAllowed' || data.detail === 'Failed') {
            throw new Error(data.detail);
          } else {
            notifications.show({
              color: 'red',
              title: 'Error',
              message: 'Unauthorized access. Please log in.',
            });
          }
          break;
        case 403:
          notifications.show({
            color: 'red',
            title: 'Error',
            message:
              'Forbidden access. You do not have permission to perform this action.',
          });
          break;
        case 404:
          router.navigate('/not-found');
          break;
        case 500:
          router.navigate('/server-error', { state: { error: data } });
          break;
        default:
          break;
      }

      return Promise.reject(data);
    }

    return data as T;
  } catch (error) {
    // handle network errors
    if (error instanceof Error) {
      notifications.show({
        color: 'red',
        title: 'Error',
        message: error.message,
      });
    } else {
      notifications.show({
        color: 'red',
        title: 'Error',
        message: JSON.stringify(error),
      });
    }
    return Promise.reject(error);
  } finally {
    removeLoading();
  }
}
export { customFetch };

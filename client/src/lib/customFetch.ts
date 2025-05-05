import { router } from '../router';
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

  await sleep(1000);

  const finalOptions = { ...defaultOptions, ...options };

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
            message: data.title ?? JSON.stringify(data),
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
}
export { customFetch };

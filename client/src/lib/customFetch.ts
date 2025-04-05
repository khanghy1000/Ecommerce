import { toast } from 'react-toastify';
import { ErrorResponse } from './types';

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

  try {
    const response = await fetch(baseUrl + path, finalOptions);
    const data = await getBody<T>(response);

    if (!response.ok) {
      console.log(typeof data);
      throw new Error(
        (data as ErrorResponse).detail ??
          `HTTP error! status: ${response.status}`
      );
    }

    return data;
  } catch (error) {
    if (error instanceof Error) {
      toast.error(error.message);
    }
    console.error('Fetch error:', error);
    throw error;
  }
}
export { customFetch };

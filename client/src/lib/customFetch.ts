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
): Promise<{ data: T; status: number }> {
  const defaultOptions: RequestInit = {
    method: 'GET',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const finalOptions = { ...defaultOptions, ...options };

  const response = await fetch(baseUrl + path, finalOptions);
  const data = await getBody<T>(response);
  const status = response.status;

  return { data, status };
}
export { customFetch };

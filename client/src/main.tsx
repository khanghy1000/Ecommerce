import { createTheme, MantineProvider } from '@mantine/core';
import '@mantine/core/styles.css';
import '@mantine/notifications/styles.css';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router';
import { router } from './router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Notifications } from '@mantine/notifications';
import './main.css';
import '@mantine/carousel/styles.css';
import { NavigationProgress } from '@mantine/nprogress';
import '@mantine/nprogress/styles.css';
import Loading from './lib/components/Loading';
import '@mantine/charts/styles.css';
import '@mantine/dates/styles.css';
import '@mantine/dropzone/styles.css';
import '@mantine/tiptap/styles.css';

const queryClient = new QueryClient();

const theme = createTheme({
  cursorType: 'pointer',
  primaryColor: 'shopee',
  colors: {
    shopee: [
      '#ffeae7', // 0 - very light
      '#ffd3c9', // 1
      '#ffb3a3', // 2
      '#ff8e77', // 3
      '#ff674d', // 4
      '#ee4d2d', // 5 - main
      '#ee4d2d', // 6
      '#ba3a23', // 7
      '#9e311e', // 8
      '#802818', // 9 - very dark
    ],
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider theme={theme}>
      <Loading />
      <NavigationProgress />
      <Notifications />
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </MantineProvider>
  </StrictMode>
);

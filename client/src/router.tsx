import { createBrowserRouter } from 'react-router';
import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';
import NotFound from './features/errors/NotFound';
import ServerError from './features/errors/ServerError';
import Homepage from './features/homepage/Homepage';
import { BuyerLayout } from './features/layout/BuyerLayout';
import ProductSearchPage from './features/products/ProductSearchPage';
import ProductPage from './features/products/ProductPage';

const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },
  {
    path: '/',
    element: <BuyerLayout />,
    children: [
      {
        index: true,
        element: <Homepage />,
      },
      {
        path: '/products/search',
        element: <ProductSearchPage />,
      },
      {
        path: '/products/:productId',
        element: <ProductPage />,
      },
      {
        path: '/cart',
        element: <div>Cart Page</div>,
      },
      {
        path: '/profile',
        element: <div>Profile Page</div>,
      },
      {
        path: '/orders',
        element: <div>Orders Page</div>,
      },
    ],
  },
  {
    path: '/not-found',
    element: <NotFound />,
  },
  {
    path: '/server-error',
    element: <ServerError />,
  },
  {
    path: '*',
    element: <NotFound />,
  },
]);

export { router };

import { createBrowserRouter } from 'react-router';
import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';
import Homepage from './features/homepage/Homepage';
import { BuyerLayout } from './features/layout/BuyerLayout';

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
        element: <div>Search Results Page</div>,
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
]);

export { router };

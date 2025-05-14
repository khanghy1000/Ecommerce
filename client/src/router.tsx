import { createBrowserRouter } from 'react-router';
import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';
import NotFound from './features/errors/NotFound';
import ServerError from './features/errors/ServerError';
import Homepage from './features/homepage/Homepage';
import { BuyerLayout } from './features/layout/BuyerLayout';
import ProductSearchPage from './features/products/ProductSearchPage';
import ProductPage from './features/products/ProductPage';
import CartPage from './features/cart/CartPage';
import CheckoutPage from './features/checkout/CheckoutPage';
import PaymentSuccessPage from './features/payments/PaymentSuccessPage';
import PaymentFailurePage from './features/payments/PaymentFailurePage';

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
        element: <CartPage />,
      },
      {
        path: '/checkout',
        element: <CheckoutPage />,
      },
      {
        path: '/profile',
        element: <div>Profile Page</div>,
      },
      {
        path: '/orders',
        element: <div>Orders Page</div>,
      },
      {
        path: '/payment/success',
        element: <PaymentSuccessPage />,
      },
      {
        path: '/payment/failure',
        element: <PaymentFailurePage />,
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

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
import OrdersPage from './features/orders/OrdersPage';
import OrderDetailPage from './features/orders/OrderDetailPage';
import ManagementPage from './features/management/ManagementPage';
import ManagementLayout from './features/layout/ManagementLayout';
import ShopPage from './features/shop/ShopPage';
import Unauthorized from './features/errors/Unauthorized';
import RequireShopOrAdminRole from './lib/components/RequireShopOrAdminRole';
import RequireBuyerRole from './lib/components/RequireBuyerRole';
import RequireLogin from './lib/components/RequireLogin';
import ProfilePage from './features/profile/ProfilePage';
import PerformancePage from './features/management/statistics/PerformancePage';
import OrdersManagementPage from './features/management/orders/OrdersManagementPage';
import ProductsManagementPage from './features/management/products/ProductsManagementPage';
import ProductCreatePage from './features/management/products/ProductCreatePage';
import ProductEditPage from './features/management/products/ProductEditPage';
import CouponsManagementPage from './features/management/coupons/CouponsManagementPage';
import CategoriesManagementPage from './features/management/categories/CategoriesManagementPage';
import OrderDetailsManagementPage from './features/management/orders/OrderDetailsManagementPage';

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
    path: '/management',
    element: (
      <RequireShopOrAdminRole>
        <ManagementLayout />
      </RequireShopOrAdminRole>
    ),
    children: [
      {
        index: true,
        element: <ManagementPage />,
      },
      {
        path: 'performance',
        element: <PerformancePage />,
      },
      {
        path: 'orders',
        children: [
          {
            index: true,
            element: <OrdersManagementPage />,
          },
          {
            path: ':id',
            element: <OrderDetailsManagementPage />,
          },
        ],
      },
      {
        path: 'products',
        children: [
          {
            index: true,
            element: <ProductsManagementPage />,
          },
          {
            path: 'create',
            element: <ProductCreatePage />,
          },
          {
            path: 'edit/:productId',
            element: <ProductEditPage />,
          },
        ],
      },
      {
        path: 'coupons',
        element: <CouponsManagementPage />,
      },
      {
        path: 'categories',
        element: <CategoriesManagementPage />,
      },
      {
        path: '*',
        element: <div>Placeholder</div>,
      },
    ],
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
        path: '/shop/:shopId',
        element: <ShopPage />,
      },
      {
        path: '/cart',
        element: (
          <RequireBuyerRole>
            <CartPage />
          </RequireBuyerRole>
        ),
      },
      {
        path: '/checkout',
        element: (
          <RequireBuyerRole>
            <CheckoutPage />
          </RequireBuyerRole>
        ),
      },
      {
        path: '/profile',
        element: (
          <RequireLogin>
            <ProfilePage />
          </RequireLogin>
        ),
      },
      {
        path: '/orders',
        element: (
          <RequireBuyerRole>
            <OrdersPage />
          </RequireBuyerRole>
        ),
      },
      {
        path: '/order/:orderId',
        element: (
          <RequireBuyerRole>
            <OrderDetailPage />
          </RequireBuyerRole>
        ),
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
    path: '/unauthorized',
    element: <Unauthorized />,
  },
  {
    path: '*',
    element: <NotFound />,
  },
]);

export { router };

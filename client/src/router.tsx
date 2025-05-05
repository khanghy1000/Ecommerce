import { createBrowserRouter } from 'react-router';
import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';

const router = createBrowserRouter([
  {
    path: '/',
    element: <>Homepage</>,
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage></RegisterPage>,
  }
]);

export { router };

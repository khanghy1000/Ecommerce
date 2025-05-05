import { createBrowserRouter } from 'react-router';
import LoginPage from './features/auth/LoginPage';

const router = createBrowserRouter([
  {
    path: '/',
    element: <>Homepage</>,
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
]);

export { router };

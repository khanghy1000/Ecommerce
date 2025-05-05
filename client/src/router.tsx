import { createBrowserRouter } from 'react-router';
import App from './App';
import LoginPage from './features/auth/LoginPage';

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
]);

export { router };

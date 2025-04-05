import { createBrowserRouter } from 'react-router';
import App from './layouts/App';
import Homepage from './lib/components/Homepage';
import Login from './lib/components/Login';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      {
        path: '/',
        element: <Homepage />,
      },
      {
        path: '/login',
        element: <Login />,
      },
    ],
  },
]);

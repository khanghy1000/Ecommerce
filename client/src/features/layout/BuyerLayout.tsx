import { Outlet } from 'react-router';
import { BuyerNavbar } from './BuyerNavbar';

export function BuyerLayout() {
  return (
    <>
      <BuyerNavbar />
      <Outlet />
    </>
  );
}

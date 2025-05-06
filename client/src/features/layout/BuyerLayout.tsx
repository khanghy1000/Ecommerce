import { Outlet } from 'react-router';
import { BuyerNavbar } from './BuyerNavbar';
import ScrollToTop from '../../lib/components/ScrollToTop';

export function BuyerLayout() {
  return (
    <>
      <ScrollToTop />
      <BuyerNavbar />
      <Outlet />
    </>
  );
}

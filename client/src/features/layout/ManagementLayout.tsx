import { Outlet } from 'react-router';
import ManagementSideBar from './ManagementSideBar';
import ScrollToTop from '../../lib/components/ScrollToTop';
import ManagementNavbar from './ManagementNavbar';
import { AppShell } from '@mantine/core';

export default function ManagementLayout() {
  return (
    <>
      <ScrollToTop />
      <AppShell
        header={{ height: 60 }}
        navbar={{
          width: 300,
          breakpoint: 'sm',
        }}
        padding="md"
      >
        <AppShell.Header>
          <ManagementNavbar />
        </AppShell.Header>
        <AppShell.Navbar p="md">
          <ManagementSideBar />
        </AppShell.Navbar>

        <AppShell.Main>
          <Outlet />
        </AppShell.Main>
      </AppShell>
    </>
  );
}

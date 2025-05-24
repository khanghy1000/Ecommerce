import { NavLink, Stack, rem, Text } from '@mantine/core';
import { Link, useLocation } from 'react-router';
import {
  IoStatsChartSharp,
  IoGrid,
  IoLayersSharp,
  IoStorefront,
  IoCartSharp,
  IoTicketSharp,
} from 'react-icons/io5';
import { useAccount } from '../../lib/hooks/useAccount';
import { useState, useEffect } from 'react';

const adminNavItems = [
  {
    icon: IoStatsChartSharp,
    label: 'Performance',
    to: '/management/performance',
  },
  { icon: IoCartSharp, label: 'Orders', to: '/management/orders' },
  { icon: IoStorefront, label: 'Products', to: '/management/products' },
  { icon: IoTicketSharp, label: 'Coupons', to: '/management/coupons' },
  { icon: IoGrid, label: 'Categories', to: '/management/categories' },
  {
    icon: IoLayersSharp,
    label: 'Subcategories',
    to: '/management/subcategories',
  },
];

const shopNavItems = [
  {
    icon: IoStatsChartSharp,
    label: 'Statistics',
    to: '/management/statistics',
  },
  { icon: IoCartSharp, label: 'Orders', to: '/management/orders' },
  { icon: IoStorefront, label: 'Products', to: '/management/products' },
];

export default function ManagementSideBar() {
  const location = useLocation();
  const { currentUserInfo } = useAccount();
  const [navItems, setNavItems] = useState(shopNavItems);

  useEffect(() => {
    if (currentUserInfo?.role === 'Admin') {
      setNavItems(adminNavItems);
    }
    if (currentUserInfo?.role === 'Shop') {
      setNavItems(shopNavItems);
    }
  }, [currentUserInfo]);

  const links = navItems.map((item) => (
    <NavLink
      key={item.label}
      component={Link}
      to={item.to}
      active={location.pathname === item.to}
      label={item.label}
      leftSection={<item.icon style={{ width: rem(20), height: rem(20) }} />}
      variant="light"
    />
  ));

  return (
    <div>
      <Text fw={600} mb="md" c="gray">
        SHOP MANAGEMENT
      </Text>
      <Stack gap="xs">{links}</Stack>
    </div>
  );
}

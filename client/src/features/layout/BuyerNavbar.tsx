import {
  ActionIcon,
  Anchor,
  Avatar,
  Button,
  Group,
  Input,
  Menu,
  Text,
  rem,
  Container,
  Box,
  Flex,
  Image,
} from '@mantine/core';
import {
  FiSearch,
  FiShoppingCart,
  FiUser,
  FiShoppingBag,
} from 'react-icons/fi';
import { useRef, useState } from 'react';
import { useAccount } from '../../lib/hooks/useAccount';
import { Link, useNavigate } from 'react-router';

export function BuyerNavbar() {
  const { currentUserInfo, logoutUser, loadingUserInfo } = useAccount();
  const [searchValue, setSearchValue] = useState('');
  const navigate = useNavigate();
  const searchInputRef = useRef<HTMLInputElement | null>(null);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchValue.trim()) {
      navigate(`/products/search?keyword=${encodeURIComponent(searchValue)}`);
      setSearchValue('');
      if (searchInputRef.current) {
        searchInputRef.current.blur();
      }
    }
  };

  return (
    <>
      <Box
        component="header"
        py="md"
        style={{
          height: 60,
          position: 'fixed',
          width: '100%',
          zIndex: 1000,
          borderBottom: `1px solid #eaeaea`,
          backgroundColor: 'var(--mantine-color-body)',
        }}
      >
        <Container size="xl" h="100%">
          <Flex align="center" justify="space-between" h="100%">
            {/* Logo / Homepage link */}
            <Anchor component={Link} to="/" underline="never" fw={700}>
              <Image src="/shopee.svg" alt="Shopee" height={30} width="auto" />
            </Anchor>

            {/* Search bar */}
            <Box
              component="form"
              onSubmit={handleSearch}
              style={{ flex: 1, maxWidth: 480, margin: '0 auto' }}
            >
              <Input
                ref={searchInputRef}
                placeholder="Search products..."
                value={searchValue}
                onChange={(e) => setSearchValue(e.target.value)}
                rightSectionPointerEvents="all"
                rightSection={
                  <ActionIcon
                    onClick={handleSearch}
                    variant="transparent"
                    color="gray"
                  >
                    <FiSearch size={16} />
                  </ActionIcon>
                }
              />
            </Box>

            {/* Navigation links */}
            <Group gap="sm">
              {currentUserInfo && (
                <>
                  {/* Cart button */}
                  <ActionIcon
                    component={Link}
                    to="/cart"
                    variant="subtle"
                    aria-label="Cart"
                    size="lg"
                    radius="xl"
                    color="gray"
                  >
                    <FiShoppingCart size={20} />
                  </ActionIcon>

                  {/* User profile dropdown */}
                  <Menu shadow="md" width={200} position="bottom-end">
                    <Menu.Target>
                      <Button variant="subtle" px="xs">
                        <Avatar
                          src={currentUserInfo.imageUrl}
                          alt={currentUserInfo.displayName}
                          size="sm"
                          style={{ marginRight: rem(5) }}
                        />
                        <Text size="sm" fw={500} c="gray">
                          {currentUserInfo.displayName}
                        </Text>
                      </Button>
                    </Menu.Target>

                    <Menu.Dropdown>
                      <Menu.Item
                        leftSection={<FiUser style={{ width: rem(14) }} />}
                        component={Link}
                        to="/profile"
                      >
                        My Profile
                      </Menu.Item>
                      <Menu.Item
                        leftSection={
                          <FiShoppingBag style={{ width: rem(14) }} />
                        }
                        component={Link}
                        to="/orders"
                      >
                        Orders
                      </Menu.Item>
                      <Menu.Divider />
                      <Menu.Item
                        color="red"
                        onClick={() => logoutUser.mutate()}
                        disabled={logoutUser.isPending}
                      >
                        Log out
                      </Menu.Item>
                    </Menu.Dropdown>
                  </Menu>
                </>
              )}
              {!currentUserInfo && !loadingUserInfo && (
                <Group gap={'xs'}>
                  <Button
                    c={'dimmed'}
                    component={Link}
                    to="/login"
                    variant="subtle"
                    size="compact-sm"
                  >
                    Log in
                  </Button>
                  <span>|</span>
                  <Button
                    component={Link}
                    to="/register"
                    variant="subtle"
                    size="compact-sm"
                  >
                    Sign up
                  </Button>
                </Group>
              )}
            </Group>
          </Flex>
        </Container>
      </Box>
      <Box
        style={{
          height: 60,
          backgroundColor: '#ffffff',
        }}
      />
    </>
  );
}

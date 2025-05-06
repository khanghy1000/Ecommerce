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
  useMantineTheme,
} from '@mantine/core';
import {
  FiSearch,
  FiShoppingCart,
  FiUser,
  FiShoppingBag,
} from 'react-icons/fi';
import { useState } from 'react';
import { useAccount } from '../../lib/hooks/useAccount';
import { Link, useNavigate } from 'react-router';

export function BuyerNavbar() {
  const { currentUserInfo, logoutUser, loadingUserInfo } = useAccount();
  const [searchValue, setSearchValue] = useState('');
  const navigate = useNavigate();
  const theme = useMantineTheme();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchValue.trim()) {
      navigate(`/products/search?keyword=${encodeURIComponent(searchValue)}`);
    }
  };

  return (
    <>
      <Box
        component="header"
        py="md"
        style={{
          height: 60,
          backgroundColor: theme.colors[theme.primaryColor][5],
          position: 'fixed',
          width: '100%',
          zIndex: 1000,
        }}
      >
        <Container size="xl" h="100%">
          <Flex align="center" justify="space-between" h="100%">
            {/* Logo / Homepage link */}
            <Anchor component={Link} to="/" underline="never" fw={700}>
              <Image
                src="/shopee-white.svg"
                alt="Shopee"
                height={30}
                width="auto"
              />
            </Anchor>

            {/* Search bar */}
            <Box
              component="form"
              onSubmit={handleSearch}
              style={{ flex: 1, maxWidth: 480, margin: '0 auto' }}
            >
              <Input
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
                    color="white"
                    aria-label="Cart"
                    size="lg"
                    radius="xl"
                  >
                    <FiShoppingCart size={20} />
                  </ActionIcon>

                  {/* User profile dropdown */}
                  <Menu shadow="md" width={200} position="bottom-end">
                    <Menu.Target>
                      <Button variant="subtle" color="white" px="xs">
                        <Avatar
                          src={currentUserInfo.imageUrl}
                          alt={currentUserInfo.displayName}
                          size="sm"
                          style={{ marginRight: rem(5) }}
                          color="white"
                        />
                        <Text size="sm" fw={500} c="white">
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
                    component={Link}
                    to="/login"
                    variant="subtle"
                    color="white"
                    size="compact-sm"
                  >
                    Log in
                  </Button>
                  <span style={{ color: 'white' }}>|</span>
                  <Button
                    component={Link}
                    to="/register"
                    variant="subtle"
                    color="white"
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

import {
  Container,
  Flex,
  Anchor,
  Group,
  Menu,
  Button,
  Avatar,
  rem,
  Image,
  Text,
} from '@mantine/core';
import { FiUser } from 'react-icons/fi';
import { Link } from 'react-router';
import { useAccount } from '../../lib/hooks/useAccount';

export default function ManagementNavbar() {
  const { currentUserInfo, logoutUser, loadingUserInfo } = useAccount();

  return (
    <Container h="100%">
      <Flex align="center" justify="space-between" h="100%">
        <Anchor component={Link} to="/" underline="never" fw={700}>
          <Group gap={8}>
            <Image src="/shopee.svg" alt="Shopee" height={30} width="auto" />
          </Group>
        </Anchor>
        <Group gap="sm">
          {currentUserInfo && (
            <>
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
                    My Shop
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
  );
}

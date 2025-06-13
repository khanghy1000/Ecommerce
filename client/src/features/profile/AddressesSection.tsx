import {
  Stack,
  Title,
  Text,
  Button,
  Card,
  Group,
  Badge,
  ActionIcon,
  Box,
  LoadingOverlay,
  Alert,
} from '@mantine/core';
import {
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiStar,
  FiMapPin,
  FiPhone,
  FiUser,
} from 'react-icons/fi';
import { UserAddressResponseDto } from '../../lib/types';

interface AddressesSectionProps {
  addresses?: UserAddressResponseDto[];
  loadingAddresses: boolean;
  onDeleteAddress: (addressId: number) => void;
  onSetDefaultAddress: (addressId: number) => void;
  onAddNewAddress: () => void;
  onEditAddressClick: (address: UserAddressResponseDto) => void;
}

export const AddressesSection = ({
  addresses,
  loadingAddresses,
  onDeleteAddress,
  onSetDefaultAddress,
  onAddNewAddress,
  onEditAddressClick,
}: AddressesSectionProps) => {
  if (loadingAddresses) {
    return (
      <Box pos="relative" h={200}>
        <LoadingOverlay visible={true} />
      </Box>
    );
  }

  return (
    <Stack gap="lg">
      <Box>
        <Title order={3} mb="md">
          Manage Addresses
        </Title>
        <Text size="sm" c="dimmed" mb="lg">
          Add and manage your shipping addresses for faster checkout.
        </Text>
      </Box>

      <Group justify="space-between" align="center">
        <Text size="sm" c="dimmed">
          {addresses?.length || 0} address
          {(addresses?.length || 0) !== 1 ? 'es' : ''}
        </Text>
        <Button
          leftSection={<FiPlus />}
          onClick={onAddNewAddress}
          variant="light"
        >
          Add New Address
        </Button>
      </Group>

      {!addresses || addresses.length === 0 ? (
        <Alert
          icon={<FiMapPin />}
          title="No addresses found"
          color="blue"
          variant="light"
        >
          You haven't added any addresses yet. Add your first address to make
          checkout faster and easier.
        </Alert>
      ) : (
        <Stack gap="md">
          {addresses.map((address) => (
            <Card key={address.id} withBorder p="lg">
              <Stack gap="sm">
                <Group justify="space-between" align="flex-start">
                  <Stack gap="xs" style={{ flex: 1 }}>
                    <Group align="center" gap="sm">
                      <FiUser size={14} />
                      <Text fw={500}>{address.name}</Text>
                      {address.isDefault && (
                        <Badge
                          leftSection={<FiStar size={10} />}
                          color="yellow"
                          variant="light"
                          size="sm"
                        >
                          Default
                        </Badge>
                      )}
                    </Group>

                    <Group align="center" gap="sm">
                      <FiPhone size={14} />
                      <Text size="sm" c="dimmed">
                        {address.phoneNumber}
                      </Text>
                    </Group>

                    <Group align="center" gap="sm">
                      <FiMapPin size={14} />
                      <Text size="sm" c="dimmed">
                        {address.address}, {address.wardName},{' '}
                        {address.districtName}, {address.provinceName}
                      </Text>
                    </Group>
                  </Stack>

                  <Group gap="xs">
                    {!address.isDefault && (
                      <Button
                        size="xs"
                        variant="light"
                        color="yellow"
                        leftSection={<FiStar size={12} />}
                        onClick={() => onSetDefaultAddress(address.id)}
                      >
                        Set Default
                      </Button>
                    )}
                    <ActionIcon
                      variant="light"
                      color="blue"
                      onClick={() => onEditAddressClick(address)}
                    >
                      <FiEdit2 size={14} />
                    </ActionIcon>
                    <ActionIcon
                      variant="light"
                      color="red"
                      onClick={() => onDeleteAddress(address.id)}
                      disabled={address.isDefault}
                    >
                      <FiTrash2 size={14} />
                    </ActionIcon>
                  </Group>
                </Group>

                {address.isDefault && (
                  <Text size="xs" c="dimmed" style={{ fontStyle: 'italic' }}>
                    This is your default shipping address. You cannot delete the
                    default address.
                  </Text>
                )}
              </Stack>
            </Card>
          ))}
        </Stack>
      )}
    </Stack>
  );
};

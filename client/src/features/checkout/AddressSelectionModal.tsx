import {
  Modal,
  Stack,
  Button,
  Card,
  Group,
  Radio,
  Box,
  Text,
  Badge,
  ActionIcon,
} from '@mantine/core';
import { FiPlus, FiEdit2, FiTrash2 } from 'react-icons/fi';
import { UserAddressResponseDto } from '../../lib/types';

// AddressSelectionModal component
type AddressSelectionModalProps = {
  opened: boolean;
  onClose: () => void;
  addresses: UserAddressResponseDto[];
  selectedAddressId: number | null;
  onAddressSelect: (addressId: number) => void;
  onSetDefault: (addressId: number) => void;
  onEdit: (address: UserAddressResponseDto) => void;
  onDelete: (addressId: number) => void;
  onAdd: () => void;
};
export const AddressSelectionModal = ({
  opened,
  onClose,
  addresses,
  selectedAddressId,
  onAddressSelect,
  onSetDefault,
  onEdit,
  onDelete,
  onAdd,
}: AddressSelectionModalProps) => {
  return (
    <Modal opened={opened} onClose={onClose} title="Shipping Address" size="lg">
      <Stack>
        <Button
          leftSection={<FiPlus size={16} />}
          variant="outline"
          onClick={onAdd}
          mb="sm"
        >
          Add New Address
        </Button>

        {addresses?.map((address) => (
          <Card
            key={address.id}
            withBorder
            p="sm"
            style={{
              cursor: 'pointer',
              backgroundColor:
                selectedAddressId === address.id
                  ? 'var(--mantine-color-gray-0)'
                  : undefined,
              borderColor:
                selectedAddressId === address.id
                  ? 'var(--mantine-color-blue-5)'
                  : undefined,
            }}
            onClick={() => onAddressSelect(address.id)}
          >
            <Stack>
              {/* address details */}
              <Group align="flex-start">
                <Radio
                  checked={selectedAddressId === address.id}
                  onChange={() => onAddressSelect(address.id)}
                  onClick={(e) => e.stopPropagation()}
                />
                <Box>
                  <Group mb={4}>
                    <Text fw={500}>{address.name}</Text>
                    <Text size="sm" c="dimmed">
                      {address.phoneNumber}
                    </Text>
                    {address.isDefault && (
                      <Badge size="sm" color="blue">
                        DEFAULT
                      </Badge>
                    )}
                  </Group>
                  <Group align="center">
                    <Text size="sm">
                      {address.address}, {address.wardName},{' '}
                      {address.districtName}, {address.provinceName}
                    </Text>
                  </Group>
                </Box>
              </Group>

              {/* edit, delete, set default buttons */}
              <Group justify="flex-end">
                <ActionIcon
                  variant="subtle"
                  color="blue"
                  onClick={(e) => {
                    e.stopPropagation();
                    onEdit(address);
                  }}
                >
                  <FiEdit2 size={16} />
                </ActionIcon>
                {!address.isDefault && (
                  <ActionIcon
                    variant="subtle"
                    color="red"
                    onClick={(e) => {
                      e.stopPropagation();
                      onDelete(address.id);
                    }}
                  >
                    <FiTrash2 size={16} />
                  </ActionIcon>
                )}
                {!address.isDefault && (
                  <Button
                    variant="outline"
                    size="xs"
                    onClick={(e) => {
                      e.stopPropagation();
                      onSetDefault(address.id);
                    }}
                  >
                    Set as default
                  </Button>
                )}
              </Group>
            </Stack>
          </Card>
        ))}
      </Stack>

      <Group justify="flex-end" mt="xl">
        <Button onClick={onClose}>Confirm Selection</Button>
      </Group>
    </Modal>
  );
};

import { Paper, Group, Title, Button, Alert } from '@mantine/core';
import { FiEdit2 } from 'react-icons/fi';
import { UserAddressResponseDto } from '../../lib/types';
import { AddressSelectionModal } from './AddressSelectionModal';
import { ShippingAddressCard } from './ShippingAddressCard';

// ShippingAddressSection component
type ShippingAddressSectionProps = {
  addresses: UserAddressResponseDto[];
  selectedAddress: UserAddressResponseDto | undefined;
  onOpenAddressModal: () => void;
  addressModalOpened: boolean;
  onAddressModalClose: () => void;
  selectedAddressId: number | null;
  onAddressSelect: (addressId: number) => void;
  onSetDefault: (addressId: number) => void;
  onEdit: (address: UserAddressResponseDto) => void;
  onDelete: (addressId: number) => void;
  onAdd: () => void;
};
export const ShippingAddressSection = ({
  addresses,
  selectedAddress,
  onOpenAddressModal,
  addressModalOpened,
  onAddressModalClose,
  selectedAddressId,
  onAddressSelect,
  onSetDefault,
  onEdit,
  onDelete,
  onAdd,
}: ShippingAddressSectionProps) => {
  return (
    <Paper shadow="xs" p="md" withBorder>
      <Group justify="space-between" mb="md">
        <Title order={4}>Shipping Address</Title>
        <Button
          size="xs"
          variant="outline"
          leftSection={<FiEdit2 size={16} />}
          onClick={onOpenAddressModal}
        >
          Change
        </Button>
      </Group>

      {addresses?.length === 0 ? (
        <Alert color="yellow">
          You don't have any saved addresses. Please add an address to continue.
        </Alert>
      ) : selectedAddress ? (
        <ShippingAddressCard
          address={selectedAddress}
          isDefault={selectedAddress.isDefault}
        />
      ) : (
        <Alert color="yellow">
          Please select a shipping address to continue.
        </Alert>
      )}

      <AddressSelectionModal
        opened={addressModalOpened}
        onClose={onAddressModalClose}
        addresses={addresses || []}
        selectedAddressId={selectedAddressId}
        onAddressSelect={onAddressSelect}
        onSetDefault={onSetDefault}
        onEdit={onEdit}
        onDelete={onDelete}
        onAdd={onAdd}
      />
    </Paper>
  );
};

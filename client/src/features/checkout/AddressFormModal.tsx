import { Modal } from '@mantine/core';
import {
  UserAddressResponseDto,
  AddUserAddressRequestDto,
  EditUserAddressRequestDto,
} from '../../lib/types';
import { AddressForm } from './AddressForm';

// Modal for adding/editing addresses
type AddressFormModalProps = {
  opened: boolean;
  onClose: () => void;
  title: string;
  editingAddress?: UserAddressResponseDto;
  onSubmit: (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => void;
  isSubmitting: boolean;
};
export const AddressFormModal = ({
  opened,
  onClose,
  title,
  editingAddress,
  onSubmit,
  isSubmitting,
}: AddressFormModalProps) => {
  return (
    <Modal opened={opened} onClose={onClose} title={title} size="lg">
      <AddressForm
        initialValues={editingAddress}
        onSubmit={onSubmit}
        onCancel={onClose}
        isSubmitting={isSubmitting}
      />
    </Modal>
  );
};

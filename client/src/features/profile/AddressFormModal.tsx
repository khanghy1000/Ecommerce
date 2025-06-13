import {
  Modal,
  Stack,
  TextInput,
  Select,
  Button,
  Group,
  Checkbox,
} from '@mantine/core';
import { useForm, zodResolver } from '@mantine/form';
import { useEffect, useState } from 'react';
import { z } from 'zod';
import { useLocations } from '../../lib/hooks/useLocations';
import { UserAddressResponseDto } from '../../lib/types';

const addressSchema = z.object({
  name: z.string().min(2, 'Name must have at least 2 characters'),
  phoneNumber: z.string().min(10, 'Please enter a valid phone number'),
  address: z.string().min(5, 'Address must have at least 5 characters'),
  provinceId: z.number().min(1, 'Please select a province'),
  districtId: z.number().min(1, 'Please select a district'),
  wardId: z.number().min(1, 'Please select a ward'),
  isDefault: z.boolean(),
});

type AddressFormValues = z.infer<typeof addressSchema>;

interface AddressFormModalProps {
  opened: boolean;
  onClose: () => void;
  editingAddress?: UserAddressResponseDto;
  onSubmit: (values: AddressFormValues, isEdit: boolean) => void;
  isSubmitting?: boolean;
}

export const AddressFormModal = ({
  opened,
  onClose,
  editingAddress,
  onSubmit,
  isSubmitting = false,
}: AddressFormModalProps) => {
  const [selectedProvinceId, setSelectedProvinceId] = useState<
    number | undefined
  >(editingAddress?.provinceId);
  const [selectedDistrictId, setSelectedDistrictId] = useState<
    number | undefined
  >(editingAddress?.districtId);
  const [selectedWardId, setSelectedWardId] = useState<number | undefined>(
    editingAddress?.wardId
  );

  const { provinces, districts, wards } = useLocations(
    selectedProvinceId,
    selectedDistrictId
  );

  const form = useForm<AddressFormValues>({
    initialValues: {
      name: editingAddress?.name || '',
      phoneNumber: editingAddress?.phoneNumber || '',
      address: editingAddress?.address || '',
      provinceId: editingAddress?.provinceId || 0,
      districtId: editingAddress?.districtId || 0,
      wardId: editingAddress?.wardId || 0,
      isDefault: editingAddress?.isDefault || false,
    },
    validate: zodResolver(addressSchema),
  });

  // Reset form when modal opens/closes or editing address changes
  useEffect(() => {
    if (opened) {
      if (editingAddress) {
        form.setValues({
          name: editingAddress.name,
          phoneNumber: editingAddress.phoneNumber,
          address: editingAddress.address,
          provinceId: editingAddress.provinceId,
          districtId: editingAddress.districtId,
          wardId: editingAddress.wardId,
          isDefault: editingAddress.isDefault,
        });
        setSelectedProvinceId(editingAddress.provinceId);
        setSelectedDistrictId(editingAddress.districtId);
        setSelectedWardId(editingAddress.wardId);
      } else {
        form.reset();
        setSelectedProvinceId(undefined);
        setSelectedDistrictId(undefined);
        setSelectedWardId(undefined);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opened, editingAddress]);

  const handleProvinceChange = (value: string | null) => {
    if (value) {
      const provinceId = parseInt(value);
      setSelectedProvinceId(provinceId);
      form.setFieldValue('provinceId', provinceId);

      // Reset district and ward selections
      setSelectedDistrictId(undefined);
      setSelectedWardId(undefined);
      form.setFieldValue('districtId', 0);
      form.setFieldValue('wardId', 0);
    }
  };

  const handleDistrictChange = (value: string | null) => {
    if (value) {
      const districtId = parseInt(value);
      setSelectedDistrictId(districtId);
      form.setFieldValue('districtId', districtId);

      // Reset ward selection
      setSelectedWardId(undefined);
      form.setFieldValue('wardId', 0);
    }
  };

  const handleWardChange = (value: string | null) => {
    if (value) {
      const wardId = parseInt(value);
      setSelectedWardId(wardId);
      form.setFieldValue('wardId', wardId);
    }
  };

  const handleSubmit = (values: AddressFormValues) => {
    onSubmit(values, !!editingAddress);
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={editingAddress ? 'Edit Address' : 'Add New Address'}
      size="md"
    >
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="md">
          <TextInput
            label="Full Name"
            placeholder="Enter full name"
            required
            {...form.getInputProps('name')}
          />

          <TextInput
            label="Phone Number"
            placeholder="Enter phone number"
            required
            {...form.getInputProps('phoneNumber')}
          />

          <TextInput
            label="Address"
            placeholder="Enter detailed address"
            required
            {...form.getInputProps('address')}
          />

          <Select
            label="Province"
            placeholder="Select province"
            required
            data={
              provinces?.map((province) => ({
                value: province.id.toString(),
                label: province.name,
              })) || []
            }
            value={selectedProvinceId?.toString()}
            onChange={handleProvinceChange}
            searchable
          />

          <Select
            label="District"
            placeholder="Select district"
            required
            data={
              districts?.map((district) => ({
                value: district.id.toString(),
                label: district.name,
              })) || []
            }
            value={selectedDistrictId?.toString()}
            onChange={handleDistrictChange}
            disabled={!selectedProvinceId}
            searchable
          />

          <Select
            label="Ward"
            placeholder="Select ward"
            required
            data={
              wards?.map((ward) => ({
                value: ward.id.toString(),
                label: ward.name,
              })) || []
            }
            value={selectedWardId?.toString()}
            onChange={handleWardChange}
            disabled={!selectedDistrictId}
            searchable
          />

          <Checkbox
            label="Set as default address"
            {...form.getInputProps('isDefault', { type: 'checkbox' })}
          />

          <Group justify="flex-end" mt="md">
            <Button variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" loading={isSubmitting}>
              {editingAddress ? 'Update Address' : 'Add Address'}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
};

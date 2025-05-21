import { Checkbox, Modal } from '@mantine/core';
import {
  UserAddressResponseDto,
  AddUserAddressRequestDto,
  EditUserAddressRequestDto,
} from '../../lib/types';
import { Stack, TextInput, Select, Flex, Group, Button } from '@mantine/core';
import { useForm, zodResolver } from '@mantine/form';
import { useEffect, useState } from 'react';
import { useLocations } from '../../lib/hooks/useLocations';
import { z } from 'zod';
import { useAddresses } from '../../lib/hooks/useAddresses';

const schema = z.object({
  address: z.string().min(5, 'Address must have at least 5 characters'),
  provinceId: z.number().min(1, 'Please select a province'),
  districtId: z.number().min(1, 'Please select a district'),
  wardId: z.number().min(1, 'Please select a ward'),
  phoneNumber: z.string().min(10, 'Please enter a valid phone number'),
  name: z.string().min(2, 'Name must have at least 2 characters'),
  isDefault: z.boolean(),
});

type AddressFormValues = z.infer<typeof schema>;

type AddressFormModalProps = {
  opened: boolean;
  onClose: () => void;
  title: string;
  editingAddress?: UserAddressResponseDto;
  onSubmitSuccess: () => void;
  isSubmitting: boolean;
};

export const AddressFormModal = ({
  opened,
  onClose,
  title,
  editingAddress,
  onSubmitSuccess,
  isSubmitting,
}: AddressFormModalProps) => {
  const [selectedProvinceId, setSelectedProvinceId] = useState<
    number | undefined
  >(editingAddress?.provinceId ?? undefined);
  const [selectedDistrictId, setSelectedDistrictId] = useState<
    number | undefined
  >(editingAddress?.districtId ?? undefined);
  const [selectedWardId, setSelectedWardId] = useState<number | undefined>(
    editingAddress?.wardId ?? undefined
  );

  const {
    provinces,
    districts,
    wards,
    loadingProvinces,
    loadingDistricts,
    loadingWards,
  } = useLocations(selectedProvinceId, selectedDistrictId);

  const { addAddress, editAddress } = useAddresses();

  const form = useForm<AddressFormValues>({
    mode: 'uncontrolled',
    initialValues: editingAddress
      ? {
          name: editingAddress.name,
          phoneNumber: editingAddress.phoneNumber,
          address: editingAddress.address,
          provinceId: editingAddress.provinceId,
          districtId: editingAddress.districtId,
          wardId: editingAddress.wardId,
          isDefault: editingAddress.isDefault,
        }
      : {
          name: '',
          phoneNumber: '',
          address: '',
          provinceId: 0,
          districtId: 0,
          wardId: 0,
          isDefault: false,
        },
    validate: zodResolver(schema),
  });

  useEffect(() => {
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
    // Disable exhaustive-deps rule for this effect
    // because if we put form as a dependency, it will cause an infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [editingAddress]);

  const handleProvinceChange = (value: string | null) => {
    if (value) {
      const provinceId = parseInt(value);
      setSelectedProvinceId(provinceId);
      form.setFieldValue('provinceId', provinceId);

      // Reset district and ward selections
      setSelectedDistrictId(undefined);
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

  const handleSubmit = (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => {
    if (editingAddress) {
      // Edit existing address
      editAddress.mutate(
        {
          id: editingAddress.id,
          addressData: values as EditUserAddressRequestDto,
        },
        {
          onSuccess: () => {
            onSubmitSuccess();
          },
        }
      );
    } else {
      // Add new address
      addAddress.mutate(values as AddUserAddressRequestDto, {
        onSuccess: () => {
          onSubmitSuccess();
        },
      });
    }
  };

  return (
    <Modal opened={opened} onClose={onClose} title={title} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            required
            label="Full Name"
            placeholder="Enter your full name"
            {...form.getInputProps('name')}
          />

          <TextInput
            required
            label="Phone Number"
            placeholder="Enter your phone number"
            {...form.getInputProps('phoneNumber')}
          />

          <TextInput
            required
            label="Address"
            placeholder="Street address, apartment, etc."
            {...form.getInputProps('address')}
          />

          <Select
            required
            label="Province"
            placeholder="Select province"
            data={
              provinces?.map((province) => ({
                value: province.id.toString(),
                label: province.name,
              })) || []
            }
            value={selectedProvinceId?.toString()}
            onChange={handleProvinceChange}
            disabled={loadingProvinces}
            error={form.errors.provinceId}
          />

          <Select
            required
            label="District"
            placeholder="Select district"
            data={
              districts?.map((district) => ({
                value: district.id.toString(),
                label: district.name,
              })) || []
            }
            value={selectedDistrictId?.toString()}
            onChange={handleDistrictChange}
            disabled={loadingDistricts || !selectedProvinceId}
            error={form.errors.districtId}
          />

          <Select
            required
            label="Ward"
            placeholder="Select ward"
            data={
              wards?.map((ward) => ({
                value: ward.id.toString(),
                label: ward.name,
              })) || []
            }
            onChange={handleWardChange}
            value={selectedWardId?.toString()}
            disabled={loadingWards || !selectedDistrictId}
            error={form.errors.wardId}
          />

          <Flex align="center" gap="md">
            <Checkbox
              label="Set as default address"
              {...form.getInputProps('isDefault', { type: 'checkbox' })}
            />
          </Flex>

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

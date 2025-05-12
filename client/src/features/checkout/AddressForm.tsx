import {
  Stack,
  TextInput,
  Select,
  Flex,
  Radio,
  Group,
  Button,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { useState, useEffect } from 'react';
import { useLocations } from '../../lib/hooks/useLocations';
import {
  UserAddressResponseDto,
  AddUserAddressRequestDto,
  EditUserAddressRequestDto,
} from '../../lib/types';

// AddressForm component for add/edit address
type AddressFormProps = {
  initialValues?: UserAddressResponseDto;
  onSubmit: (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => void;
  onCancel: () => void;
  isSubmitting: boolean;
};
export const AddressForm = ({
  initialValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: AddressFormProps) => {
  const [selectedProvinceId, setSelectedProvinceId] = useState<
    number | undefined
  >(initialValues?.wardId ? undefined : undefined);
  const [selectedDistrictId, setSelectedDistrictId] = useState<
    number | undefined
  >(initialValues?.wardId ? undefined : undefined);

  // Use the useLocations hook to get provinces, districts, and wards
  const {
    provinces,
    districts,
    wards,
    loadingProvinces,
    loadingDistricts,
    loadingWards,
  } = useLocations(selectedProvinceId, selectedDistrictId);

  const form = useForm<AddUserAddressRequestDto | EditUserAddressRequestDto>({
    mode: 'uncontrolled',
    initialValues: initialValues
      ? {
          name: initialValues.name,
          phoneNumber: initialValues.phoneNumber,
          address: initialValues.address,
          wardId: initialValues.wardId,
          isDefault: initialValues.isDefault,
        }
      : {
          name: '',
          phoneNumber: '',
          address: '',
          wardId: 0,
          isDefault: false,
        },
  });

  // Find the ward, district, and province for the selected address
  useEffect(() => {
    if (initialValues?.wardId && wards.length > 0) {
      const ward = wards.find((w) => w.id === initialValues.wardId);
      if (ward) {
        setSelectedDistrictId(ward.districtId);
      }
    }
  }, [initialValues?.wardId, wards]);

  useEffect(() => {
    if (selectedDistrictId && districts.length > 0) {
      const district = districts.find((d) => d.id === selectedDistrictId);
      if (district) {
        setSelectedProvinceId(district.provinceId);
      }
    }
  }, [selectedDistrictId, districts]);

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

  const handleSubmit = (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => {
    onSubmit(values);
  };

  return (
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
          onChange={(value) => {
            if (value) {
              form.setFieldValue('wardId', parseInt(value));
            }
          }}
          disabled={loadingWards || !selectedDistrictId}
        />

        <Flex align="center" gap="md">
          <Radio
            label="Set as default address"
            checked={form.values.isDefault}
            onChange={(event) =>
              form.setFieldValue('isDefault', event.currentTarget.checked)
            }
          />
        </Flex>

        <Group justify="flex-end" mt="md">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" loading={isSubmitting}>
            {initialValues ? 'Update Address' : 'Add Address'}
          </Button>
        </Group>
      </Stack>
    </form>
  );
};

import { Card, Group, Text, Badge } from '@mantine/core';
import { FiMapPin } from 'react-icons/fi';
import { UserAddressResponseDto } from '../../lib/types';

// ShippingAddressCard component
type ShippingAddressCardProps = {
  address: UserAddressResponseDto;
  isDefault: boolean;
};
export const ShippingAddressCard = ({
  address,
  isDefault,
}: ShippingAddressCardProps) => {
  return (
    <Card withBorder p="md">
      <Group mb={4}>
        <FiMapPin />
        <Text fw={500}>{address.name}</Text>
        <Text size="sm" c="dimmed">
          {address.phoneNumber}
        </Text>
        {isDefault && (
          <Badge size="sm" color="blue">
            DEFAULT
          </Badge>
        )}
      </Group>
      <Text size="sm" ml={24}>
        {address.address}, {address.wardName}, {address.districtName},{' '}
        {address.provinceName}
      </Text>
    </Card>
  );
};

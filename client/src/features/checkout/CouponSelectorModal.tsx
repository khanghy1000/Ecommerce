import {
  Modal,
  Stack,
  Button,
  Table,
  Group,
  Text,
  Badge,
  Center,
  Alert,
} from '@mantine/core';
import { format } from 'date-fns';
import { CouponResponseDto } from '../../lib/types';
import { formatPrice } from '../../lib/utils';

// Format coupon value for display
const formatCouponValue = (coupon: CouponResponseDto) => {
  if (coupon.discountType === 'Percent') {
    return `${coupon.value}%`;
  } else {
    return formatPrice(coupon.value);
  }
};
// CouponSelectorModal component
type CouponSelectorModalProps = {
  opened: boolean;
  onClose: () => void;
  title: string;
  coupons: CouponResponseDto[];
  selectedCoupon: string | undefined;
  onCouponSelect: (code: string | undefined) => void;
  iconComponent: React.ReactNode;
  badgeColor: string;
};
export const CouponSelectorModal = ({
  opened,
  onClose,
  title,
  coupons,
  selectedCoupon,
  onCouponSelect,
  iconComponent,
  badgeColor,
}: CouponSelectorModalProps) => {
  return (
    <Modal opened={opened} onClose={onClose} title={title} size="lg">
      {coupons.length > 0 ? (
        <Stack>
          <Button
            variant="subtle"
            color="red"
            onClick={() => onCouponSelect(undefined)}
          >
            Remove Coupon
          </Button>
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Code</Table.Th>
                <Table.Th>Discount</Table.Th>
                <Table.Th>Min Order</Table.Th>
                <Table.Th>Valid Until</Table.Th>
                <Table.Th>Action</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {coupons.map((coupon) => (
                <Table.Tr key={coupon.code}>
                  <Table.Td>
                    <Group>
                      {iconComponent}
                      <Text fw={500}>{coupon.code}</Text>
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={badgeColor}>
                      {formatCouponValue(coupon)}
                      {coupon.maxDiscountAmount > 0 &&
                        coupon.discountType === 'Percent' &&
                        ` (max ${formatPrice(coupon.maxDiscountAmount)})`}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    {coupon.minOrderValue > 0
                      ? formatPrice(coupon.minOrderValue)
                      : 'None'}
                  </Table.Td>
                  <Table.Td>
                    {format(new Date(coupon.endTime), 'dd/MM/yyyy')}
                  </Table.Td>
                  <Table.Td>
                    <Button
                      size="xs"
                      onClick={() => onCouponSelect(coupon.code)}
                      variant={
                        selectedCoupon === coupon.code ? 'filled' : 'outline'
                      }
                    >
                      {selectedCoupon === coupon.code ? 'Selected' : 'Select'}
                    </Button>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Stack>
      ) : (
        <Center py="xl">
          <Alert color="yellow">
            No {title.toLowerCase()} available at the moment.
          </Alert>
        </Center>
      )}
      <Group justify="flex-end" mt="xl">
        <Button onClick={onClose}>Close</Button>
      </Group>
    </Modal>
  );
};

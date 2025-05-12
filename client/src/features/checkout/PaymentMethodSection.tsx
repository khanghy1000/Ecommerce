import { Paper, Title, SegmentedControl, Group, Text } from '@mantine/core';
import { FiTruck, FiCreditCard } from 'react-icons/fi';
import { PaymentMethod } from '../../lib/types';

// PaymentMethodSection component
type PaymentMethodSectionProps = {
  paymentMethod: PaymentMethod;
  setPaymentMethod: (method: PaymentMethod) => void;
};
export const PaymentMethodSection = ({
  paymentMethod,
  setPaymentMethod,
}: PaymentMethodSectionProps) => {
  return (
    <Paper shadow="xs" p="md" withBorder>
      <Title order={4} mb="md">
        Payment Method
      </Title>

      <SegmentedControl
        value={paymentMethod}
        onChange={(value) => setPaymentMethod(value as PaymentMethod)}
        data={[
          {
            value: 'Cod',
            label: (
              <Group gap="xs">
                <FiTruck size={16} />
                <Text>Cash on Delivery</Text>
              </Group>
            ),
          },
          {
            value: 'Vnpay',
            label: (
              <Group gap="xs">
                <FiCreditCard size={16} />
                <Text>Online Payment</Text>
              </Group>
            ),
          },
        ]}
        fullWidth
      />
    </Paper>
  );
};

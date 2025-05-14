import { Link, useSearchParams } from 'react-router';
import { usePayment } from '../../lib/hooks/usePayment';
import {
  Alert,
  Button,
  Container,
  Group,
  Loader,
  Paper,
  Text,
  Title,
  Stack,
  ThemeIcon,
} from '@mantine/core';
import { FaCheckCircle, FaHome, FaBoxOpen } from 'react-icons/fa';
import { format } from 'date-fns';

export default function PaymentSuccessPage() {
  const [searchParams] = useSearchParams();
  const paymentId = searchParams.get('paymentId') || '0';
  const { paymentInfo, loadingPaymentInfo } = usePayment(paymentId);

  if (loadingPaymentInfo) {
    return (
      <Container size="sm" py="xl">
        <Paper p="xl" radius="md" withBorder>
          <Group justify="center" py="xl">
            <Loader size="lg" />
            <Text>Loading payment information...</Text>
          </Group>
        </Paper>
      </Container>
    );
  }

  if (!paymentInfo) {
    return (
      <Container size="sm" py="xl">
        <Paper p="xl" radius="md" withBorder>
          <Alert title="Payment Not Found" color="red">
            We couldn't find the payment information.
          </Alert>
          <Group justify="center" mt="xl">
            <Button leftSection={<FaHome />} component={Link} to="/">
              Return to Homepage
            </Button>
          </Group>
        </Paper>
      </Container>
    );
  }

  return (
    <Container size="sm" py="xl">
      <Paper p="xl" radius="md" withBorder>
        <Stack align="center" gap="md">
          <ThemeIcon size={80} radius={100} color="green">
            <FaCheckCircle size={40} />
          </ThemeIcon>

          <Title order={1} ta="center">
            Payment Successful!
          </Title>

          <Text ta="center" size="lg">
            Thank you for your purchase. Your payment has been processed
            successfully.
          </Text>

          <Paper withBorder p="md" radius="md" w="100%">
            <Stack gap="xs">
              <Group>
                <Text fw={500}>Payment ID:</Text>
                <Text>{paymentInfo.paymentId}</Text>
              </Group>

              <Group>
                <Text fw={500}>Transaction Date:</Text>
                <Text>{format(new Date(paymentInfo.timestamp), 'PPpp')}</Text>
              </Group>

              <Group>
                <Text fw={500}>Payment Method:</Text>
                <Text>{paymentInfo.paymentMethod || 'VNPAY'}</Text>
              </Group>

              {paymentInfo.bankCode && (
                <Group>
                  <Text fw={500}>Bank:</Text>
                  <Text>{paymentInfo.bankCode}</Text>
                </Group>
              )}

              {paymentInfo.transactionCode && (
                <Group>
                  <Text fw={500}>Transaction Code:</Text>
                  <Text>{paymentInfo.transactionCode}</Text>
                </Group>
              )}
            </Stack>
          </Paper>

          <Group mt="lg">
            <Button
              leftSection={<FaBoxOpen />}
              variant="filled"
              component={Link}
              to={`/orders`}
            >
              View My Orders
            </Button>
            <Button
              leftSection={<FaHome />}
              variant="light"
              component={Link}
              to={`/`}
            >
              Continue Shopping
            </Button>
          </Group>
        </Stack>
      </Paper>
    </Container>
  );
}

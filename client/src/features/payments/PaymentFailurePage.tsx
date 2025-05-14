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
import { FaExclamationTriangle, FaHome, FaBoxOpen } from 'react-icons/fa';
import { format } from 'date-fns';

export default function PaymentFailurePage() {
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

  return (
    <Container size="sm" py="xl">
      <Paper p="xl" radius="md" withBorder>
        <Stack align="center" gap="md">
          <ThemeIcon size={80} radius={100} color="red">
            <FaExclamationTriangle size={40} />
          </ThemeIcon>

          <Title order={1} ta="center">
            Payment Failed
          </Title>

          <Text ta="center" size="lg" c="dimmed">
            Unfortunately, there was an issue processing your payment.
          </Text>

          {paymentInfo ? (
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

                {paymentInfo.responseDescription && (
                  <Group>
                    <Text fw={500}>Error:</Text>
                    <Text color="red">{paymentInfo.responseDescription}</Text>
                  </Group>
                )}

                {paymentInfo.transactionDescription && (
                  <Group>
                    <Text fw={500}>Transaction Details:</Text>
                    <Text>{paymentInfo.transactionDescription}</Text>
                  </Group>
                )}
              </Stack>
            </Paper>
          ) : (
            <Alert title="Payment Details Unavailable" color="yellow" w="100%">
              We couldn't retrieve the payment details.
            </Alert>
          )}

          <Text mt="md" size="sm" c="dimmed" ta="center">
            Your order has not been confirmed. Please try again or choose a
            different payment method.
          </Text>

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
              to="/"
            >
              Return to Homepage
            </Button>
          </Group>
        </Stack>
      </Paper>
    </Container>
  );
}

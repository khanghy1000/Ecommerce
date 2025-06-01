import {
  Box,
  Button,
  Container,
  Title,
  Text,
  Group,
  Stack,
  Paper,
  Badge,
  Divider,
  Image,
  Anchor,
  Loader,
  Modal,
  Alert,
} from '@mantine/core';
import { useParams, useNavigate, Link } from 'react-router';
import { useDisclosure } from '@mantine/hooks';
import { format } from 'date-fns';
import { FiChevronLeft, FiCheck, FiX, FiAlertTriangle } from 'react-icons/fi';
import { useOrders } from '../../../lib/hooks/useOrders';
import { formatPrice } from '../../../lib/utils';
import { SalesOrderStatus } from '../../../lib/types';

const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

function OrderDetailsManagementPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const orderId = Number(id);

  const { order, loadingOrder, confirmOrder, cancelOrder } = useOrders(orderId);

  const [
    confirmModalOpened,
    { open: openConfirmModal, close: closeConfirmModal },
  ] = useDisclosure(false);
  const [
    cancelModalOpened,
    { open: openCancelModal, close: closeCancelModal },
  ] = useDisclosure(false);

  if (loadingOrder) {
    return (
      <Box p="md">
        <Group>
          <Button
            variant="subtle"
            leftSection={<FiChevronLeft size={18} />}
            onClick={() => navigate('/management/orders')}
          >
            Back to Orders
          </Button>
        </Group>
        <Container size="md" py="xl">
          <Group justify="center">
            <Loader />
            <Text>Loading order details...</Text>
          </Group>
        </Container>
      </Box>
    );
  }

  if (!order) {
    return (
      <Box p="md">
        <Group>
          <Button
            variant="subtle"
            leftSection={<FiChevronLeft size={18} />}
            onClick={() => navigate('/management/orders')}
          >
            Back to Orders
          </Button>
        </Group>
        <Container size="md" py="xl">
          <Alert variant="light" color="red" icon={<FiAlertTriangle />}>
            Order not found
          </Alert>
        </Container>
      </Box>
    );
  }

  const getStatusColor = (status: SalesOrderStatus) => {
    switch (status) {
      case 'PendingPayment':
        return 'yellow';
      case 'PendingConfirmation':
        return 'blue';
      case 'Tracking':
        return 'cyan';
      case 'Delivered':
        return 'green';
      case 'Cancelled':
        return 'red';
      default:
        return 'gray';
    }
  };

  const getStatusLabel = (status: SalesOrderStatus) => {
    switch (status) {
      case 'PendingPayment':
        return 'Pending Payment';
      case 'PendingConfirmation':
        return 'Pending Confirmation';
      case 'Tracking':
        return 'Tracking';
      case 'Delivered':
        return 'Delivered';
      case 'Cancelled':
        return 'Cancelled';
      default:
        return status;
    }
  };

  const canConfirm = order.status === 'PendingConfirmation';
  const canCancel =
    order.status === 'PendingPayment' || order.status === 'PendingConfirmation';

  const handleConfirm = () => {
    openConfirmModal();
  };

  const handleCancel = () => {
    openCancelModal();
  };

  const executeConfirm = () => {
    confirmOrder.mutate(orderId, {
      onSuccess: () => {
        closeConfirmModal();
      },
    });
  };

  const executeCancel = () => {
    cancelOrder.mutate(orderId, {
      onSuccess: () => {
        closeCancelModal();
      },
    });
  };

  return (
    <Box p="md">
      <Group justify="space-between" mb="lg">
        <Group>
          <Button
            variant="subtle"
            leftSection={<FiChevronLeft size={18} />}
            onClick={() => navigate('/management/orders')}
          >
            Back to Orders
          </Button>
          <Title order={2}>Order #{order.id}</Title>
        </Group>

        <Group>
          {canConfirm && (
            <Button
              color="green"
              leftSection={<FiCheck size={16} />}
              onClick={handleConfirm}
              loading={confirmOrder.isPending}
            >
              Confirm Order
            </Button>
          )}
          {canCancel && (
            <Button
              color="red"
              variant="outline"
              leftSection={<FiX size={16} />}
              onClick={handleCancel}
              loading={cancelOrder.isPending}
            >
              Cancel Order
            </Button>
          )}
        </Group>
      </Group>

      <Stack gap="lg">
        {/* Order Summary */}
        <Paper p="md" withBorder>
          <Title order={3} mb="md">
            Order Information
          </Title>
          <Group justify="space-between" wrap="wrap">
            <Box>
              <Text size="sm" c="dimmed">
                Order Date
              </Text>
              <Text fw={500}>{format(new Date(order.orderTime), 'PPpp')}</Text>
            </Box>
            <Box>
              <Text size="sm" c="dimmed">
                Status
              </Text>
              <Badge color={getStatusColor(order.status)} size="lg">
                {getStatusLabel(order.status)}
              </Badge>
            </Box>
            <Box>
              <Text size="sm" c="dimmed">
                Payment Method
              </Text>
              <Text fw={500}>
                {order.paymentMethod === 'Cod' ? 'Cash on Delivery' : 'VNPay'}
              </Text>
            </Box>
            <Box>
              <Text size="sm" c="dimmed">
                Total Amount
              </Text>
              <Text fw={700} size="lg" c="blue">
                {formatPrice(order.total)}
              </Text>
            </Box>
          </Group>
        </Paper>

        {/* Customer Information */}
        <Paper p="md" withBorder>
          <Title order={3} mb="md">
            Customer Information
          </Title>
          <Stack gap="xs">
            <Group>
              <Text size="sm" c="dimmed" w={100}>
                Customer:
              </Text>
              <Text fw={500}>{order.buyerName}</Text>
            </Group>
            <Group>
              <Text size="sm" c="dimmed" w={100}>
                Customer ID:
              </Text>
              <Text>{order.buyerId}</Text>
            </Group>
          </Stack>
        </Paper>

        {/* Shipping Information */}
        <Paper p="md" withBorder>
          <Title order={3} mb="md">
            Shipping Information
          </Title>
          <Stack gap="xs">
            <Group>
              <Text size="sm" c="dimmed" w={100}>
                Recipient:
              </Text>
              <Text fw={500}>{order.shippingName}</Text>
            </Group>
            <Group>
              <Text size="sm" c="dimmed" w={100}>
                Phone:
              </Text>
              <Text>{order.shippingPhone}</Text>
            </Group>
            <Group>
              <Text size="sm" c="dimmed" w={100}>
                Address:
              </Text>
              <Text>
                {order.shippingAddress}, {order.shippingWardName},{' '}
                {order.shippingDistrictName}, {order.shippingProvinceName}
              </Text>
            </Group>
            {order.shippingOrderCode && (
              <Group>
                <Text size="sm" c="dimmed" w={100}>
                  Tracking Code:
                </Text>
                <Text fw={500}>{order.shippingOrderCode}</Text>
              </Group>
            )}
          </Stack>
        </Paper>

        {/* Order Products */}
        <Paper p="md" withBorder>
          <Title order={3} mb="md">
            Order Products
          </Title>
          <Stack gap="md">
            {order.orderProducts.map((product) => {
              const mainPhoto = product.photos?.find(
                (photo) => photo.displayOrder === 1
              );
              return (
                <Group key={product.id} align="flex-start" gap="md">
                  <Box style={{ flexShrink: 0 }}>
                    <Image
                      src={
                        mainPhoto
                          ? `${baseImageUrl}${mainPhoto.key}`
                          : '/placeholder.svg'
                      }
                      alt={product.name}
                      w={80}
                      h={80}
                      fit="cover"
                      radius="md"
                    />
                  </Box>

                  <Box style={{ flex: 1, minWidth: 0 }}>
                    <Anchor
                      component={Link}
                      to={`/products/${product.productId}`}
                      fw={500}
                      color="blue"
                      style={{
                        wordBreak: 'break-word',
                        display: 'block',
                        marginBottom: 4,
                      }}
                    >
                      {product.name}
                    </Anchor>
                    <Group gap="lg">
                      <Text size="sm" c="dimmed">
                        Price: {formatPrice(product.price)}
                      </Text>
                      <Text size="sm" c="dimmed">
                        Quantity: {product.quantity}
                      </Text>
                      <Text size="sm" fw={500}>
                        Subtotal: {formatPrice(product.subtotal)}
                      </Text>
                    </Group>
                  </Box>
                </Group>
              );
            })}
          </Stack>
        </Paper>

        {/* Order Summary */}
        <Paper p="md" withBorder>
          <Title order={3} mb="md">
            Order Summary
          </Title>
          <Stack gap="xs">
            <Group justify="space-between">
              <Text>Subtotal:</Text>
              <Text>{formatPrice(order.subtotal)}</Text>
            </Group>
            <Group justify="space-between">
              <Text>Shipping Fee:</Text>
              <Text>{formatPrice(order.shippingFee)}</Text>
            </Group>
            {order.productDiscountAmount > 0 && (
              <Group justify="space-between">
                <Text c="green">Product Discount:</Text>
                <Text c="green">
                  -{formatPrice(order.productDiscountAmount)}
                </Text>
              </Group>
            )}
            {order.shippingDiscountAmount > 0 && (
              <Group justify="space-between">
                <Text c="green">Shipping Discount:</Text>
                <Text c="green">
                  -{formatPrice(order.shippingDiscountAmount)}
                </Text>
              </Group>
            )}
            <Divider />
            <Group justify="space-between">
              <Text fw={700} size="lg">
                Total:
              </Text>
              <Text fw={700} size="lg" c="blue">
                {formatPrice(order.total)}
              </Text>
            </Group>
          </Stack>
        </Paper>
      </Stack>

      {/* Confirm Order Modal */}
      <Modal
        opened={confirmModalOpened}
        onClose={closeConfirmModal}
        title="Confirm Order"
        centered
      >
        <Stack gap="md">
          <Text>
            Are you sure you want to confirm this order? This action will update
            the order status and create a shipping order.
          </Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeConfirmModal}>
              Cancel
            </Button>
            <Button
              color="green"
              onClick={executeConfirm}
              loading={confirmOrder.isPending}
            >
              Confirm Order
            </Button>
          </Group>
        </Stack>
      </Modal>

      {/* Cancel Order Modal */}
      <Modal
        opened={cancelModalOpened}
        onClose={closeCancelModal}
        title="Cancel Order"
        centered
      >
        <Stack gap="md">
          <Text>
            Are you sure you want to cancel this order? This action cannot be
            undone.
          </Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeCancelModal}>
              No, Keep Order
            </Button>
            <Button
              color="red"
              onClick={executeCancel}
              loading={cancelOrder.isPending}
            >
              Yes, Cancel Order
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Box>
  );
}

export default OrderDetailsManagementPage;

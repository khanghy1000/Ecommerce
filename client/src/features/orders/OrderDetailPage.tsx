import { useOrders } from '../../lib/hooks/useOrders';
import {
  Box,
  Group,
  Button,
  Container,
  Anchor,
  Image,
  Modal,
  Divider,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useNavigate, Link, useParams } from 'react-router';
import { FiChevronLeft } from 'react-icons/fi';
import { } from '../../features/orders/OrdersPage';
import { getOrderStatusText } from '../../lib/utils';

const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

function OrderDetailPage() {
  const navigate = useNavigate();
  const { orderId } = useParams();

  const { order, loadingOrder, cancelOrder } = useOrders(Number(orderId));
  const [opened, { open, close }] = useDisclosure(false);
  console.log(order);

  if (loadingOrder) {
    return (
      <Container size="sm" py="xl">
        <div>Loading...</div>
      </Container>
    );
  }

  if (!order) {
    return (
      <Container size="sm" py="xl">
        <div>Order not found.</div>
      </Container>
    );
  }

  return (
    <Container size="sm" py="xl">
      <Button
        variant="subtle"
        leftSection={<FiChevronLeft size={18} />}
        mb="md"
        onClick={() => navigate(-1)}
      >
        Back
      </Button>

      <Box mb="md" p="md" style={{ border: '1px solid #eee', borderRadius: 8 }}>
        <Group justify="space-between" mb="xs">
          <div>
            <b>Order id:</b> {order.id}
          </div>
          <div>
            <b>Order time:</b> {new Date(order.orderTime).toLocaleString()}
          </div>
          <div>
            <b>Status:</b> {getOrderStatusText(order.status)}
          </div>
          {(order.status === 'PendingConfirmation' ||
            order.status === 'PendingPayment') && (
            <Button
              color="red"
              variant="outline"
              size="xs"
              loading={cancelOrder.isPending}
              onClick={open}
            >
              Cancel Order
            </Button>
          )}
        </Group>

        <Divider my="sm" />
        <Box mb="md" p="md" style={{ background: '#f8fafc', borderRadius: 6 }}>
          <div style={{ marginBottom: 4 }}>
            <b>Recipient:</b> {order.shippingName}
          </div>
          <div style={{ marginBottom: 4 }}>
            <b>Phone:</b> {order.shippingPhone}
          </div>
          <div>
            <b>Address:</b> {order.shippingAddress}, {order.shippingWardName},{' '}
            {order.shippingDistrictName}, {order.shippingProvinceName}
          </div>
        </Box>
        <Divider my="sm" />
        <Box mb="md" p="md" style={{ background: '#f8fafc', borderRadius: 6 }}>
          <b>Products:</b>
          <ul style={{ margin: 0, paddingLeft: 0, listStyle: 'none' }}>
            {order.orderProducts.map((product) => {
              const mainPhoto = product.photos?.find(
                (photo) => photo.displayOrder === 1
              );
              return (
                <li
                  key={product.id}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 12,
                    marginBottom: 8,
                  }}
                >
                  {mainPhoto ? (
                    <Box
                      component="span"
                      style={{
                        display: 'inline-block',
                        width: 80,
                        height: 80,
                        borderRadius: 4,
                        overflow: 'hidden',
                        background: '#f6f6f6',
                        border: '1px solid #eee',
                      }}
                    >
                      <Image
                        src={`${baseImageUrl}${mainPhoto.key}`}
                        alt={product.name}
                        fit="contain"
                        h={80}
                        w={80}
                      />
                    </Box>
                  ) : (
                    <Box
                      component="span"
                      style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        width: 80,
                        height: 80,
                        borderRadius: 4,
                        background: '#f6f6f6',
                        border: '1px solid #eee',
                        color: '#bbb',
                        fontSize: 32,
                        fontWeight: 700,
                      }}
                    >
                      <Image
                        src="/placeholder.svg"
                        alt="No image available"
                        fit="contain"
                        h={80}
                        w={80}
                      />
                    </Box>
                  )}
                  <div
                    style={{
                      flex: 1,
                      minWidth: 0,
                      display: 'flex',
                      alignItems: 'center',
                    }}
                  >
                    <Anchor
                      component={Link}
                      to={`/products/${product.productId}`}
                      underline="always"
                      color="blue"
                      fw={500}
                      style={{
                        wordBreak: 'break-word',
                        maxWidth: '100%',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap',
                        display: 'block',
                      }}
                      title={product.name}
                    >
                      {product.name}
                    </Anchor>
                  </div>
                  <span
                    style={{
                      color: '#888',
                      whiteSpace: 'nowrap',
                      marginLeft: 'auto',
                      fontWeight: 500,
                    }}
                  >
                    x {product.quantity} - {product.price.toLocaleString()}₫
                  </span>
                </li>
              );
            })}
          </ul>
        </Box>

        <Group
          justify="flex-end"
          mt="md"
          gap={4}
          style={{ flexDirection: 'column', alignItems: 'flex-end' }}
        >
          <Box
            style={{
              fontSize: 14,
              color: '#888',
              fontWeight: 400,
              marginBottom: 2,
            }}
          >
            Subtotal: {order.subtotal?.toLocaleString() ?? 0}₫
          </Box>
          <Box
            style={{
              fontSize: 14,
              color: '#888',
              fontWeight: 400,
              marginBottom: 2,
            }}
          >
            Shipping fee: {order.shippingFee?.toLocaleString() ?? 0}₫
          </Box>
          {order.productCouponCode && (
            <Box
              style={{
                fontSize: 14,
                color: '#888',
                fontWeight: 400,
                marginBottom: 2,
              }}
            >
              Product coupon
              <span> ({order.productCouponCode}): </span>
              {order.productDiscountAmount ? (
                <span>−{order.productDiscountAmount.toLocaleString()}₫</span>
              ) : null}
            </Box>
          )}

          {order.shippingCouponCode && (
            <Box
              style={{
                fontSize: 14,
                color: '#888',
                fontWeight: 400,
                marginBottom: 2,
              }}
            >
              Shipping coupon
              <span> ({order.shippingCouponCode}): </span>
              {order.shippingDiscountAmount ? (
                <span>−{order.shippingDiscountAmount.toLocaleString()}₫</span>
              ) : null}
            </Box>
          )}

          <Box
            style={{
              fontSize: 14,
              color: '#888',
              fontWeight: 400,
              marginBottom: 2,
            }}
          >
            Payment method: {order.paymentMethod ?? ''}
          </Box>

          <Box
            style={{
              fontSize: 22,
              fontWeight: 700,
              color: 'var(--mantine-primary-color-filled)',
            }}
          >
            Total: {order.total.toLocaleString()}₫
          </Box>
        </Group>
      </Box>
      <Modal opened={opened} onClose={close} title="Cancel Order" centered>
        <div>Are you sure you want to cancel this order?</div>
        <Group mt="md" justify="flex-end">
          <Button variant="default" onClick={close}>
            No
          </Button>
          <Button
            color="red"
            loading={cancelOrder.isPending}
            onClick={() => {
              cancelOrder.mutate(order.id, {
                onSuccess: () => {
                  close();
                  navigate('/orders');
                },
              });
            }}
          >
            Confirm
          </Button>
        </Group>
      </Modal>
    </Container>
  );
}

export default OrderDetailPage;

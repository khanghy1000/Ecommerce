import { useOrders } from '../../lib/hooks/useOrders';
import {
  Box,
  Group,
  Button,
  Container,
  Pagination,
  Image,
  Modal,
  Divider,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useSearchParams, useNavigate, Link } from 'react-router';
import { useState } from 'react';
import { SalesOrderStatus } from '../../lib/types';

const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

const navOptions = [
  { label: 'All', value: 'All' },
  { label: 'Pending Payment', value: 'PendingPayment' },
  { label: 'Pending Confirmation', value: 'PendingConfirmation' },
  { label: 'Tracking', value: 'Tracking' },
  { label: 'Delivered', value: 'Delivered' },
  { label: 'Cancelled', value: 'Cancelled' },
];

function OrdersPage() {
  const navigate = useNavigate();

  const [searchParams] = useSearchParams();
  const currentStatus = searchParams.get('status');

  const { orders, loadingOrders, cancelOrder } = useOrders(undefined, {
    status:
      currentStatus == 'All' ? undefined : (currentStatus as SalesOrderStatus),
    pageSize: 3, //test paging
    pageNumber: Number(searchParams.get('page')) || 1,
  });
  const [opened, { open, close }] = useDisclosure(false);
  const [selectedOrderId, setSelectedOrderId] = useState<number | null>(null);
  console.log(orders);

  return (
    <Box component="nav" style={{ borderBottom: '1px solid #ccc' }}>
      <Container size="md" py="sm">
        <Group>
          {navOptions.map((option) => (
            <Button
              component={Link}
              to={`/orders?status=${option.value}`}
              key={option.value}
              variant={currentStatus === option.value ? 'filled' : 'subtle'}
              color="blue"
            >
              {option.label}
            </Button>
          ))}
        </Group>
      </Container>
      <Container size="md" py="sm">
        {loadingOrders ? (
          <div>Loading...</div>
        ) : (
          <>
            {orders?.items && orders.items.length > 0 ? (
              <>
                {orders.items.map((order) => (
                  <Box
                    key={order.id}
                    mb="xl"
                    p={0}
                    style={{
                      border:
                        '2px solid var(--mantine-primary-color-light, #a5d8ff)',
                      borderRadius: 10,
                      boxShadow: '0 4px 24px 0 rgba(80, 120, 200, 0.10)',
                      background: '#fff',
                      overflow: 'hidden',
                      transition: 'box-shadow 0.2s, border 0.2s',
                      position: 'relative',
                    }}
                  >
                    {/* Order header */}
                    <Box
                      px="md"
                      py="sm"
                      style={{
                        background:
                          'var(--mantine-primary-color-light, #e7f5ff)',
                        borderBottom: '1px solid #e3e8f0',
                        borderTopLeftRadius: 10,
                        borderTopRightRadius: 10,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                      }}
                    >
                      <Group gap={16}>
                        <b
                          style={{
                            color: 'var(--mantine-primary-color-filled)',
                          }}
                        >
                          {order.status}
                        </b>
                        <span style={{ color: '#666', fontSize: 14 }}>
                          {new Date(order.orderTime).toLocaleString()}
                        </span>
                      </Group>
                      {(order.status === 'PendingConfirmation' ||
                        order.status === 'PendingPayment') && (
                        <Button
                          color="red"
                          variant="outline"
                          size="xs"
                          loading={cancelOrder.isPending}
                          onClick={() => {
                            setSelectedOrderId(order.id);
                            open();
                          }}
                        >
                          Cancel Order
                        </Button>
                      )}
                    </Box>

                    {/* Products */}
                    <Box px="md" py="sm">
                      <Box
                        component={Link}
                        to={`/order/${order.id}`}
                        style={{
                          textDecoration: 'none',
                          color: 'inherit',
                        }}
                      >
                        <b>Products:</b>
                        <ul
                          style={{
                            margin: 0,
                            paddingLeft: 0,
                            listStyle: 'none',
                            marginTop: 8,
                            paddingRight: 0,
                          }}
                        >
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
                                  <span
                                    style={{
                                      wordBreak: 'break-word',
                                      maxWidth: '100%',
                                      overflow: 'hidden',
                                      textOverflow: 'ellipsis',
                                      whiteSpace: 'nowrap',
                                      display: 'block',
                                      fontWeight: 500,
                                      color: '#222',
                                      fontSize: 16,
                                    }}
                                    title={product.name}
                                  >
                                    {product.name}
                                  </span>
                                </div>
                                <span
                                  style={{
                                    color: '#888',
                                    whiteSpace: 'nowrap',
                                    marginLeft: 'auto',
                                    fontWeight: 500,
                                  }}
                                >
                                  x {product.quantity} -{' '}
                                  {product.price.toLocaleString()}₫
                                </span>
                              </li>
                            );
                          })}
                        </ul>
                      </Box>
                    </Box>
                    {/* Shipping fee and total */}
                    <Divider my="sm" />
                    <Group
                      justify="flex-end"
                      mt="md"
                      gap={4}
                      style={{
                        flexDirection: 'column',
                        alignItems: 'flex-end',
                        padding: '0 24px 18px 0',
                      }}
                    >
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
                ))}

                <Group justify="center" mt="md">
                  <Pagination
                    total={Math.ceil(
                      (orders.totalCount || 0) / (orders.pageSize || 10)
                    )}
                    value={orders.pageNumber || 1}
                    onChange={(page) => {
                      const params = new URLSearchParams(searchParams);
                      params.set('page', page.toString());
                      navigate(`/orders?${params.toString()}`);
                    }}
                  />
                </Group>

                <Modal
                  opened={opened}
                  onClose={close}
                  title="Cancel Order"
                  centered
                >
                  <div>Are you sure you want to cancel this order?</div>
                  <Group mt="md" justify="flex-end">
                    <Button variant="default" onClick={close}>
                      No
                    </Button>
                    <Button
                      color="red"
                      loading={cancelOrder.isPending}
                      onClick={() => {
                        if (selectedOrderId) {
                          cancelOrder.mutate(selectedOrderId, {
                            onSuccess: () => {
                              close();
                            },
                          });
                        }
                      }}
                    >
                      Confirm
                    </Button>
                  </Group>
                </Modal>
              </>
            ) : (
              <div>No orders found.</div>
            )}
          </>
        )}
      </Container>
    </Box>
  );
}

export default OrdersPage;

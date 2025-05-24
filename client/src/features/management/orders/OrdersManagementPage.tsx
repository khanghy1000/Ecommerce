import { useState } from 'react';
import {
  Paper,
  Title,
  TextInput,
  Select,
  Button,
  Group,
  Box,
  Table,
  Text,
  Badge,
  Pagination,
  Loader,
  Flex,
  Stack,
  NumberInput,
} from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { ListOrdersRequest, SalesOrderStatus } from '../../../lib/types';
import { useOrders } from '../../../lib/hooks/useOrders';
import { startOfDay, endOfDay, format } from 'date-fns';
import { useAccount } from '../../../lib/hooks/useAccount';
import { formatPrice } from '../../../lib/utils';

function OrdersManagementPage() {
  const { currentUserInfo } = useAccount();
  const isAdmin = currentUserInfo?.role === 'Admin';

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const form = useForm<ListOrdersRequest>({
    initialValues: {
      pageNumber: page,
      pageSize: pageSize,
      orderId: undefined,
      fromDate: undefined,
      toDate: undefined,
      status: undefined,
      buyerId: undefined,
      shopId: isAdmin ? undefined : currentUserInfo?.id,
    },
  });

  const { orders, loadingOrders } = useOrders(undefined, form.values);

  const resetForm = () => {
    form.reset();
    // If shop role, keep the shopId filter
    if (!isAdmin && currentUserInfo?.id) {
      form.setValues({
        shopId: currentUserInfo.id,
      });
    }
  };

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

  const statusOptions = [
    { value: 'PendingPayment', label: 'Pending Payment' },
    { value: 'PendingConfirmation', label: 'Pending Confirmation' },
    { value: 'Tracking', label: 'Tracking' },
    { value: 'Delivered', label: 'Delivered' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  const pageSizeOptions = [
    { value: '5', label: '5' },
    { value: '10', label: '10' },
    { value: '20', label: '20' },
    { value: '50', label: '50' },
  ];

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
    form.setValues({ ...form.values, pageNumber: newPage });
  };

  const handlePageSizeChange = (newSize: string | null) => {
    if (newSize) {
      const size = parseInt(newSize, 10);
      setPageSize(size);
      form.setValues({ ...form.values, pageSize: size, pageNumber: 1 });
      setPage(1); // Reset to first page when changing page size
    }
  };

  return (
    <Box p="md">
      <Title order={2} mb="lg">
        Orders Management
      </Title>

      <Paper p="md" mb="lg" withBorder>
          <Stack gap="md">
            <Group>
              <NumberInput
                label="Order ID"
                placeholder="Search by Order ID"
                min={1}
                {...form.getInputProps('orderId')}
                w={{ base: '100%', sm: '48%', md: '32%' }}
                hideControls
                // rightSection={
                //   form.values.orderId ? (
                //     <ActionIcon
                //       size="sm"
                //       variant="transparent"
                //       onClick={() => form.setFieldValue('orderId', undefined)}
                //     >
                //       <FiX size={16} />
                //     </ActionIcon>
                //   ) : null
                // }
              />

              <TextInput
                label="Buyer"
                placeholder="Search by Buyer ID"
                {...form.getInputProps('buyerId')}
                w={{ base: '100%', sm: '48%', md: '32%' }}
                // rightSection={
                //   form.values.buyerId ? (
                //     <ActionIcon
                //       size="sm"
                //       variant="transparent"
                //       onClick={() => form.setFieldValue('buyerId', '')}
                //     >
                //       <FiX size={16} />
                //     </ActionIcon>
                //   ) : null
                // }
              />

              {isAdmin && (
                <TextInput
                  label="Shop"
                  placeholder="Search by Shop ID"
                  {...form.getInputProps('shopId')}
                  w={{ base: '100%', sm: '48%', md: '32%' }}
                  // rightSection={
                  //   form.values.shopId ? (
                  //     <ActionIcon
                  //       size="sm"
                  //       variant="transparent"
                  //       onClick={() => form.setFieldValue('shopId', '')}
                  //     >
                  //       <FiX size={16} />
                  //     </ActionIcon>
                  //   ) : null
                  // }
                />
              )}
            </Group>

            <Group>
              <DatePickerInput
                label="From Date"
                placeholder="Select start date"
                valueFormat="DD/MM/YYYY"
                onChange={(date) => {
                  if (date) {
                    // Format the date in ISO string with proper UTC time for start of day
                    form.setFieldValue(
                      'fromDate',
                      startOfDay(new Date(date)).toISOString()
                    );
                  } else {
                    form.setFieldValue('fromDate', undefined);
                  }
                }}
                w={{ base: '100%', sm: '48%', md: '32%' }}
                clearable
              />

              <DatePickerInput
                label="To Date"
                placeholder="Select end date"
                valueFormat="DD/MM/YYYY"
                onChange={(date) => {
                  if (date) {
                    // Format the date in ISO string with proper UTC time for end of day
                    form.setFieldValue(
                      'toDate',
                      endOfDay(new Date(date)).toISOString()
                    );
                  } else {
                    form.setFieldValue('toDate', undefined);
                  }
                }}
                w={{ base: '100%', sm: '48%', md: '32%' }}
                clearable
              />

              <Select
                label="Order Status"
                placeholder="Select status"
                data={statusOptions}
                {...form.getInputProps('status')}
                w={{ base: '100%', sm: '48%', md: '32%' }}
                clearable
              />
            </Group>

            <Group justify="flex-end">
              <Button variant="outline" onClick={resetForm}>
                Reset Filters
              </Button>
            </Group>
          </Stack>
      </Paper>

      <Paper p="md" withBorder>
        <Box mb="md">
          <Group justify="space-between">
            <Text size="sm">
              {orders ? `Total: ${orders.totalCount} orders` : 'Loading...'}
            </Text>

            <Group>
              <Text size="sm">Items per page:</Text>
              <Select
                data={pageSizeOptions}
                value={pageSize.toString()}
                onChange={handlePageSizeChange}
                w={80}
                size="xs"
              />
            </Group>
          </Group>
        </Box>

        {loadingOrders ? (
          <Flex justify="center" align="center" h={200}>
            <Loader />
          </Flex>
        ) : (
          <>
            <Box style={{ overflowX: 'auto' }}>
              <Table striped>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>ID</Table.Th>
                    <Table.Th>Time</Table.Th>
                    <Table.Th>Shipping Info</Table.Th>
                    <Table.Th>Buyer</Table.Th>
                    {isAdmin && <Table.Th>Shop</Table.Th>}
                    <Table.Th>Status</Table.Th>
                    <Table.Th>Payment</Table.Th>
                    <Table.Th style={{ textAlign: 'right' }}>Amount</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {orders && orders.items.length > 0 ? (
                    orders.items.map((order) => (
                      <Table.Tr key={order.id}>
                        <Table.Td>{order.id}</Table.Td>
                        <Table.Td>
                          {format(new Date(order.orderTime), 'PPpp')}
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm" fw={500}>
                            {order.shippingName}
                          </Text>
                          <Text size="xs" c="dimmed">
                            {order.shippingPhone}
                          </Text>
                        </Table.Td>
                        <Table.Td>
                          <Text
                            size="sm"
                            title={`${order.buyerName} (ID: ${order.buyerId})`}
                            style={{
                              maxWidth: '150px',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                              whiteSpace: 'nowrap',
                            }}
                          >
                            {order.buyerName}
                          </Text>
                        </Table.Td>
                        {isAdmin && (
                          <Table.Td>
                            <Text
                              size="sm"
                              title={`${order.shopName} (ID: ${order.shopId})`}
                              style={{
                                maxWidth: '150px',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                              }}
                            >
                              {order.shopName}
                            </Text>
                          </Table.Td>
                        )}
                        <Table.Td>
                          <Badge color={getStatusColor(order.status)}>
                            {
                              statusOptions.find(
                                ({ value }) => value === order.status
                              )?.label
                            }
                          </Badge>
                        </Table.Td>
                        <Table.Td>
                          {order.paymentMethod === 'Cod'
                            ? 'Cash on Delivery'
                            : 'VNPay'}
                        </Table.Td>
                        <Table.Td style={{ textAlign: 'right' }}>
                          {formatPrice(order.total)}
                        </Table.Td>
                      </Table.Tr>
                    ))
                  ) : (
                    <Table.Tr>
                      <Table.Td
                        colSpan={isAdmin ? 8 : 6}
                        style={{ textAlign: 'center' }}
                      >
                        <Text py="md">No orders found</Text>
                      </Table.Td>
                    </Table.Tr>
                  )}
                </Table.Tbody>
              </Table>
            </Box>

            {orders && orders.totalCount > 0 && (
              <Group justify="center" mt="md">
                <Pagination
                  total={Math.ceil(orders.totalCount / pageSize)}
                  value={page}
                  onChange={handlePageChange}
                />
              </Group>
            )}
          </>
        )}
      </Paper>
    </Box>
  );
}

export default OrdersManagementPage;

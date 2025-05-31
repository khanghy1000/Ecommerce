import { useState, useEffect } from 'react';
import {
  Container,
  Title,
  Paper,
  Table,
  Button,
  Text,
  Group,
  ActionIcon,
  Badge,
  Box,
  TextInput,
  Pagination,
  Modal,
  Select,
  Flex,
  Loader,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useForm } from '@mantine/form';
import { useProducts } from '../../../lib/hooks/useProducts';
import { useAccount } from '../../../lib/hooks/useAccount';
import { ListProductsRequest } from '../../../lib/types';
import { Link } from 'react-router';
import { formatPrice } from '../../../lib/utils';
import {
  FiEdit,
  FiPlus,
  FiTrash2,
  FiSearch,
  FiEye,
  FiEyeOff,
} from 'react-icons/fi';
import { notifications } from '@mantine/notifications';

function ProductsManagementPage() {
  const { currentUserInfo } = useAccount();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const isAdmin = currentUserInfo?.role === 'Admin';

  // Modal for delete confirmation
  const [deleteProductId, setDeleteProductId] = useState<number | null>(null);
  const [opened, { open, close }] = useDisclosure(false);

  // Form for filtering (user input state)
  const form = useForm<ListProductsRequest>({
    mode: 'uncontrolled',
    initialValues: {
      pageNumber: page,
      pageSize: pageSize,
      keyword: '',
      shopId: isAdmin ? '' : currentUserInfo?.id,
      includeInactive: true,
    },
  });

  // Query parameters state (separate from form state)
  const [queryParams, setQueryParams] = useState<ListProductsRequest>({
    pageNumber: page,
    pageSize: pageSize,
    keyword: '',
    shopId: isAdmin ? '' : currentUserInfo?.id,
    includeInactive: true,
  });

  // Query products with filter
  const { products, loadingProducts, setProductActiveState, deleteProduct } =
    useProducts(undefined, queryParams);

  // Initialize query parameters when component mounts or when user info changes
  useEffect(() => {
    const initialParams = {
      pageNumber: 1,
      pageSize: pageSize,
      keyword: '',
      shopId: isAdmin ? '' : currentUserInfo?.id,
      includeInactive: true,
    };
    setQueryParams(initialParams);
  }, [currentUserInfo?.id, isAdmin, pageSize]);

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
    form.setFieldValue('pageNumber', newPage);
    setQueryParams((prev) => ({ ...prev, pageNumber: newPage }));
  };

  const handlePageSizeChange = (newSize: string | null) => {
    if (newSize) {
      const size = parseInt(newSize, 10);
      setPageSize(size);
      form.setFieldValue('pageSize', size);
      setPage(1); // Reset to first page when changing page size
      form.setFieldValue('pageNumber', 1);
      setQueryParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
    }
  };

  const handleSearch = () => {
    setPage(1); // Reset page on new search
    form.setFieldValue('pageNumber', 1);
    // Update query params with current form values to trigger API call
    const currentValues = form.getValues();
    setQueryParams({
      ...currentValues,
      pageNumber: 1,
    });
  };

  const handleResetFilters = () => {
    form.reset();
    setPage(1);
    setQueryParams(form.getValues());
  };

  const handleActiveStateChange = (id: number, currentState: boolean) => {
    setProductActiveState.mutate(
      { id, isActive: !currentState },
      {
        onSuccess: () => {
          notifications.show({
            title: 'Product updated',
            message: `Product has been ${!currentState ? 'activated' : 'deactivated'} successfully`,
            color: !currentState ? 'green' : 'yellow',
          });
        },
      }
    );
  };

  const handleDeleteClick = (id: number) => {
    setDeleteProductId(id);
    open();
  };

  const confirmDelete = () => {
    if (deleteProductId) {
      deleteProduct.mutate(deleteProductId, {
        onSuccess: () => {
          notifications.show({
            title: 'Product deleted',
            message: 'Product has been deleted successfully',
            color: 'red',
          });
          close();
          setDeleteProductId(null);
        },
      });
    }
  };

  const pageSizeOptions = [
    { value: '5', label: '5' },
    { value: '10', label: '10' },
    { value: '20', label: '20' },
    { value: '50', label: '50' },
  ];

  return (
    <Container size="xl" py="xl">
      <Group justify="space-between" mb="md">
        <Title order={2}>Products Management</Title>
        <Button
          component={Link}
          to="/management/products/create"
          leftSection={<FiPlus size={16} />}
        >
          Add New Product
        </Button>
      </Group>

      <Paper p="md" mb="lg" withBorder>
        <form onSubmit={form.onSubmit(handleSearch)}>
          <Group mb="md">
            <TextInput
              label="Search Products"
              placeholder="Enter keyword"
              {...form.getInputProps('keyword')}
              key={form.key('keyword')}
              style={{ flex: 1 }}
            />

            {isAdmin && (
              <TextInput
                label="Shop ID"
                placeholder="Filter by shop ID"
                {...form.getInputProps('shopId')}
                key={form.key('shopId')}
                style={{ width: '200px' }}
              />
            )}

            <Button
              type="submit"
              leftSection={<FiSearch size={16} />}
              style={{ marginTop: 'auto' }}
            >
              Search
            </Button>

            <Button
              variant="outline"
              onClick={handleResetFilters}
              style={{ marginTop: 'auto' }}
            >
              Reset Filters
            </Button>
          </Group>
        </form>
      </Paper>

      <Paper p="md" withBorder>
        <Box mb="md">
          <Group justify="space-between">
            <Text size="sm">
              {products
                ? `Total: ${products.totalCount} products`
                : 'Loading...'}
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

        {loadingProducts ? (
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
                    <Table.Th>Name</Table.Th>
                    <Table.Th>Regular Price</Table.Th>
                    <Table.Th>Discount Price</Table.Th>
                    <Table.Th>Stock</Table.Th>
                    {isAdmin && <Table.Th>Shop</Table.Th>}
                    <Table.Th>Status</Table.Th>
                    <Table.Th>Actions</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {products && products.items.length > 0 ? (
                    products.items.map((product) => (
                      <Table.Tr key={product.id}>
                        <Table.Td>{product.id}</Table.Td>
                        <Table.Td style={{ maxWidth: '200px' }}>
                          <div
                            style={{
                              width: '100%',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                              whiteSpace: 'nowrap',
                            }}
                          >
                            <Text
                              component={Link}
                              to={`/products/${product.id}`}
                              style={{
                                textDecoration: 'none',
                                color: 'inherit',
                                fontWeight: 500,
                              }}
                              title={product.name}
                            >
                              {product.name}
                            </Text>
                          </div>
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm" fw={500}>
                            {formatPrice(product.regularPrice)}
                          </Text>
                        </Table.Td>
                        <Table.Td>
                          {product.discountPrice ? (
                            <Text style={{ color: 'red' }} size="sm" fw={500}>
                              {formatPrice(product.discountPrice)}
                            </Text>
                          ) : (
                            <Text size="xs" c="dimmed">
                              -
                            </Text>
                          )}
                        </Table.Td>
                        <Table.Td>{product.quantity}</Table.Td>
                        {isAdmin && (
                          <Table.Td>
                            <Text
                              size="sm"
                              title={`${product.shopName} (ID: ${product.shopId})`}
                            >
                              {product.shopName}
                            </Text>
                          </Table.Td>
                        )}
                        <Table.Td>
                          <Badge color={product.active ? 'green' : 'gray'}>
                            {product.active ? 'Active' : 'Inactive'}
                          </Badge>
                        </Table.Td>
                        <Table.Td>
                          <Group>
                            <ActionIcon
                              color={product.active ? 'red' : 'green'}
                              variant="subtle"
                              onClick={() =>
                                handleActiveStateChange(
                                  product.id,
                                  product.active
                                )
                              }
                              title={
                                product.active
                                  ? 'Deactivate product'
                                  : 'Activate product'
                              }
                            >
                              {product.active ? <FiEyeOff /> : <FiEye />}
                            </ActionIcon>
                            <ActionIcon
                              color="blue"
                              variant="subtle"
                              component={Link}
                              to={`/management/products/edit/${product.id}`}
                              title="Edit product"
                            >
                              <FiEdit />
                            </ActionIcon>
                            <ActionIcon
                              color="red"
                              variant="subtle"
                              onClick={() => handleDeleteClick(product.id)}
                              title="Delete product"
                            >
                              <FiTrash2 />
                            </ActionIcon>
                          </Group>
                        </Table.Td>
                      </Table.Tr>
                    ))
                  ) : (
                    <Table.Tr>
                      <Table.Td
                        colSpan={isAdmin ? 8 : 7}
                        style={{ textAlign: 'center' }}
                      >
                        <Text py="md">No products found</Text>
                      </Table.Td>
                    </Table.Tr>
                  )}
                </Table.Tbody>
              </Table>
            </Box>

            {products && products.totalCount > 0 && (
              <Group justify="center" mt="md">
                <Pagination
                  total={Math.ceil(products.totalCount / pageSize)}
                  value={page}
                  onChange={handlePageChange}
                />
              </Group>
            )}
          </>
        )}
      </Paper>

      {/* Delete Confirmation Modal */}
      <Modal opened={opened} onClose={close} title="Delete Product" centered>
        <Text>
          Are you sure you want to delete this product? This action cannot be
          undone.
        </Text>
        <Group justify="flex-end" mt="xl">
          <Button variant="default" onClick={close}>
            Cancel
          </Button>
          <Button
            color="red"
            onClick={confirmDelete}
            loading={deleteProduct.isPending}
          >
            Delete
          </Button>
        </Group>
      </Modal>
    </Container>
  );
}

export default ProductsManagementPage;

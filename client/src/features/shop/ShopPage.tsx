import { useState, useMemo } from 'react';
import { useParams } from 'react-router';
import { useProducts } from '../../lib/hooks/useProducts';
import { useStats } from '../../lib/hooks/useStats';
import {
  Container,
  Title,
  Grid,
  Card,
  Text,
  Loader,
  Box,
  Flex,
  Skeleton,
  Pagination,
  Group,
  Select,
  Paper,
  Badge,
  Stack,
  Center,
  Alert,
} from '@mantine/core';
import { ProductCard } from '../products/ProductCard';
import { ListProductsRequest } from '../../lib/types';
import { FiStar, FiShoppingBag, FiAlertCircle } from 'react-icons/fi';
import { formatPrice } from '../../lib/utils';

function ShopPage() {
  const { shopId } = useParams();
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // Get shop summary statistics
  const { shopSummary, loadingShopSummary } = useStats(undefined, shopId);

  // Prepare products request
  const productsRequest = useMemo<ListProductsRequest>(
    () => ({
      shopId: shopId,
      pageNumber: currentPage,
      pageSize: pageSize,
      sortBy: 'name',
      sortDirection: 'asc',
      includeInactive: false,
    }),
    [shopId, currentPage, pageSize]
  );

  // Get products for this shop
  const { products, loadingProducts } = useProducts(undefined, productsRequest);

  // Calculate total pages for pagination
  const totalPages = useMemo(() => {
    if (!products) return 0;
    return Math.ceil(products.totalCount / products.pageSize);
  }, [products]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handlePageSizeChange = (newSize: string | null) => {
    if (newSize) {
      const size = parseInt(newSize, 10);
      setPageSize(size);
      setCurrentPage(1); // Reset to first page when changing page size
    }
  };

  const pageSizeOptions = [
    { value: '12', label: '12' },
    { value: '20', label: '20' },
    { value: '32', label: '32' },
    { value: '48', label: '48' },
  ];

  if (!shopId) {
    return (
      <Container size="xl" py="xl">
        <Alert
          icon={<FiAlertCircle size={16} />}
          title="Invalid Shop"
          color="red"
        >
          Shop ID is required to view this page.
        </Alert>
      </Container>
    );
  }

  return (
    <Container size="xl" py="xl">
      {/* Shop Summary Section */}
      <Box mb="xl">
        <Title order={1} mb="lg">
          Shop Statistics
        </Title>

        <Grid>
          <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
            <Card withBorder padding="lg" radius="md" h="100%">
              {loadingShopSummary ? (
                <Skeleton height={100} />
              ) : (
                <Stack justify="center" h="100%">
                  <Group align="center" gap="sm">
                    <FiShoppingBag
                      size={24}
                      color="var(--mantine-color-blue-6)"
                    />
                    <Box>
                      <Text size="xl" fw={700}>
                        {shopSummary?.totalOrders || 0}
                      </Text>
                      <Text c="dimmed" size="sm">
                        Total Orders
                      </Text>
                    </Box>
                  </Group>
                </Stack>
              )}
            </Card>
          </Grid.Col>

          <Grid.Col span={{ base: 12, sm: 6, md: 4 }}>
            <Card withBorder padding="lg" radius="md" h="100%">
              {loadingShopSummary ? (
                <Skeleton height={100} />
              ) : (
                <Stack justify="center" h="100%">
                  <Group align="center" gap="sm">
                    <FiStar size={24} color="var(--mantine-color-yellow-6)" />
                    <Box>
                      <Text size="xl" fw={700}>
                        {shopSummary?.averageRating
                          ? shopSummary.averageRating.toFixed(1)
                          : 'N/A'}
                      </Text>
                      <Text c="dimmed" size="sm">
                        Average Rating
                      </Text>
                    </Box>
                  </Group>
                </Stack>
              )}
            </Card>
          </Grid.Col>

          <Grid.Col span={{ base: 12, sm: 12, md: 4 }}>
            <Card withBorder padding="lg" radius="md" h="100%">
              {loadingProducts ? (
                <Skeleton height={100} />
              ) : (
                <Stack justify="center" h="100%">
                  <Group align="center" gap="sm">
                    <Box style={{ color: 'var(--mantine-color-green-6)' }}>
                      📦
                    </Box>
                    <Box>
                      <Text size="xl" fw={700}>
                        {products?.totalCount || 0}
                      </Text>
                      <Text c="dimmed" size="sm">
                        Total Products
                      </Text>
                    </Box>
                  </Group>
                </Stack>
              )}
            </Card>
          </Grid.Col>
        </Grid>
      </Box>

      {/* Products Section */}
      <Box>
        <Group justify="space-between" align="center" mb="lg">
          <Title order={2}>Shop Products</Title>

          <Group gap="sm">
            <Text size="sm" c="dimmed">
              Items per page:
            </Text>
            <Select
              data={pageSizeOptions}
              value={pageSize.toString()}
              onChange={handlePageSizeChange}
              w={80}
              size="sm"
            />
          </Group>
        </Group>

        {/* Products loading state */}
        {loadingProducts && (
          <Grid>
            {Array.from({ length: pageSize }, (_, i) => (
              <Grid.Col key={i} span={{ base: 12, xs: 6, sm: 4, md: 3, lg: 2 }}>
                <Card withBorder padding="md" radius="md">
                  <Skeleton height={150} mb="md" />
                  <Skeleton height={20} mb="xs" />
                  <Skeleton height={16} width="60%" />
                </Card>
              </Grid.Col>
            ))}
          </Grid>
        )}

        {/* Products content */}
        {!loadingProducts && products && (
          <>
            {products.items.length === 0 ? (
              <Paper withBorder p="xl">
                <Center>
                  <Stack align="center" gap="md">
                    <FiShoppingBag size={64} opacity={0.4} />
                    <Text size="lg" fw={500}>
                      No products available
                    </Text>
                    <Text c="dimmed" ta="center">
                      This shop doesn't have any active products at the moment.
                    </Text>
                  </Stack>
                </Center>
              </Paper>
            ) : (
              <>
                {/* Products count and pagination info */}
                <Group justify="space-between" mb="md">
                  <Text size="sm" c="dimmed">
                    Showing{' '}
                    {Math.min(
                      (currentPage - 1) * pageSize + 1,
                      products.totalCount
                    )}{' '}
                    - {Math.min(currentPage * pageSize, products.totalCount)} of{' '}
                    {products.totalCount} products
                  </Text>
                </Group>

                {/* Products grid */}
                <Grid mb="xl">
                  {products.items.map((product) => (
                    <Grid.Col
                      key={product.id}
                      span={{ base: 12, xs: 6, sm: 4, md: 3, lg: 2 }}
                    >
                      <ProductCard product={product} />
                    </Grid.Col>
                  ))}
                </Grid>

                {/* Pagination */}
                {totalPages > 1 && (
                  <Flex justify="center">
                    <Pagination
                      value={currentPage}
                      onChange={handlePageChange}
                      total={totalPages}
                      withEdges
                    />
                  </Flex>
                )}
              </>
            )}
          </>
        )}
      </Box>
    </Container>
  );
}

export default ShopPage;

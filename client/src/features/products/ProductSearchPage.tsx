import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router';
import {
  Container,
  Title,
  Grid,
  Box,
  Loader,
  Flex,
  Stack,
  Group,
  Checkbox,
  Radio,
  TextInput,
  NumberInput,
  Text,
  Button,
  Select,
  Paper,
  Pagination,
} from '@mantine/core';
import { useProducts } from '../../lib/hooks/useProducts';
import { useCategories } from '../../lib/hooks/useCategories';
import { ListProductsRequest } from '../../lib/types';
import { ProductCard } from './ProductCard';
import {
  FaSearch,
  FaFilter,
  FaSortAmountDown,
  FaSortAmountUp,
} from 'react-icons/fa';
import { useWindowScroll } from '@mantine/hooks';

function ProductSearchPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const scrollTo = useWindowScroll()[1];

  // Get filter values from URL or set defaults
  const currentKeyword = searchParams.get('keyword') || '';
  const currentPageSize = Number(searchParams.get('pageSize')) || 20;
  const currentPageNumber = Number(searchParams.get('pageNumber')) || 1;
  const currentSortBy = searchParams.get('sortBy') || 'name';
  const currentSortDirection =
    (searchParams.get('sortDirection') as 'asc' | 'desc') || 'asc';
  const currentCategory = searchParams.get('categoryId')
    ? Number(searchParams.get('categoryId'))
    : undefined;

  // Wrap currentSubcategories initialization in useMemo to prevent dependencies changing every render
  const currentSubcategories = useMemo(() => {
    return searchParams.get('subCategoryIds')
      ? searchParams
          .get('subCategoryIds')
          ?.split(',')
          .map((id) => Number(id))
      : [];
  }, [searchParams]);

  const currentMinPrice = searchParams.get('minPrice')
    ? Number(searchParams.get('minPrice'))
    : undefined;

  const currentMaxPrice = searchParams.get('maxPrice')
    ? Number(searchParams.get('maxPrice'))
    : undefined;

  // Local state for form values
  const [keyword, setKeyword] = useState(currentKeyword);
  const [minPrice, setMinPrice] = useState<number | undefined>(currentMinPrice);
  const [maxPrice, setMaxPrice] = useState<number | undefined>(currentMaxPrice);

  // Fetch categories for filter options
  const { categories, loadingCategories } = useCategories();

  // Prepare filter request based on URL search params
  const filterRequest = useMemo<ListProductsRequest>(
    () => ({
      keyword: currentKeyword || undefined,
      pageSize: currentPageSize,
      pageNumber: currentPageNumber,
      sortBy: currentSortBy,
      sortDirection: currentSortDirection as 'asc' | 'desc',
      categoryId: currentCategory,
      subCategoryIds: currentSubcategories,
      minPrice: currentMinPrice,
      maxPrice: currentMaxPrice,
    }),
    [
      currentKeyword,
      currentPageSize,
      currentPageNumber,
      currentSortBy,
      currentSortDirection,
      currentCategory,
      currentSubcategories,
      currentMinPrice,
      currentMaxPrice,
    ]
  );

  // Fetch filtered products
  const { products, loadingProducts } = useProducts(undefined, filterRequest);

  // Calculate total pages for pagination
  const totalPages = useMemo(() => {
    if (!products) return 0;
    return Math.ceil(products.totalCount / products.pageSize);
  }, [products]);

  const handlePageChange = (page: number) => {
    const params = new URLSearchParams(searchParams);
    params.set('pageNumber', page.toString());
    setSearchParams(params);
    scrollTo({ y: 0 });
  };

  // Reset pagination when filters change
  useEffect(() => {
    if (
      currentPageNumber > 1 &&
      products &&
      currentPageNumber > Math.ceil(products.totalCount / products.pageSize)
    ) {
      const params = new URLSearchParams(searchParams);
      params.set('pageNumber', '1');
      setSearchParams(params);
      scrollTo({ y: 0 });
    }
  }, [products, currentPageNumber, searchParams, setSearchParams, scrollTo]);

  const handleCategoryChange = (categoryId: number | null) => {
    const params = new URLSearchParams(searchParams);

    // Always remove subcategory filters when changing categories
    params.delete('subCategoryIds');

    if (categoryId) {
      params.set('categoryId', categoryId.toString());
    } else {
      params.delete('categoryId');
    }

    // Reset to first page
    params.set('pageNumber', '1');
    setSearchParams(params);
    scrollTo({ y: 0 });
  };

  const handleSubcategoryChange = (
    subcategoryId: number,
    isChecked: boolean
  ) => {
    const params = new URLSearchParams(searchParams);

    // Always remove category filter when selecting a subcategory
    params.delete('categoryId');

    let subcategoryIds = currentSubcategories || [];

    if (isChecked) {
      // Add the subcategory if it's not already in the list
      if (!subcategoryIds.includes(subcategoryId)) {
        subcategoryIds = [...subcategoryIds, subcategoryId];
      }
    } else {
      // Remove the subcategory if it's in the list
      subcategoryIds = subcategoryIds.filter((id) => id !== subcategoryId);
    }

    if (subcategoryIds.length > 0) {
      params.set('subCategoryIds', subcategoryIds.join(','));
    } else {
      params.delete('subCategoryIds');
    }

    // Reset to first page
    params.set('pageNumber', '1');
    setSearchParams(params);
    scrollTo({ y: 0 });
  };

  const handleSearch = () => {
    const params = new URLSearchParams(searchParams);

    if (keyword) {
      params.set('keyword', keyword);
    } else {
      params.delete('keyword');
    }

    // Reset to first page
    params.set('pageNumber', '1');
    setSearchParams(params);
    scrollTo({ y: 0 });
  };

  const handlePriceFilter = () => {
    const params = new URLSearchParams(searchParams);

    if (minPrice !== undefined) {
      params.set('minPrice', minPrice.toString());
    } else {
      params.delete('minPrice');
    }

    if (maxPrice !== undefined) {
      params.set('maxPrice', maxPrice.toString());
    } else {
      params.delete('maxPrice');
    }

    // Reset to first page
    params.set('pageNumber', '1');
    setSearchParams(params);
    scrollTo({ y: 0 });
  };

  const handleSortChange = (sortBy: string, sortDirection: 'asc' | 'desc') => {
    const params = new URLSearchParams(searchParams);
    params.set('sortBy', sortBy);
    params.set('sortDirection', sortDirection);
    setSearchParams(params);
    scrollTo({ y: 0 });
  };

  const resetFilters = () => {
    const params = new URLSearchParams();
    params.set('pageSize', '20');
    params.set('pageNumber', '1');
    params.set('sortBy', 'name');
    params.set('sortDirection', 'asc');
    setSearchParams(params);
    setKeyword('');
    setMinPrice(undefined);
    setMaxPrice(undefined);
    scrollTo({ y: 0 });
  };

  return (
    <Container size="xl" py="md">
      <Title order={1} mb="lg">
        Product Search
      </Title>

      <Grid gutter="md">
        {/* Filters sidebar */}
        <Grid.Col span={{ base: 12, md: 3 }}>
          <Paper withBorder p="md">
            <Stack>
              <Title order={4}>Filters</Title>

              {/* Keyword search */}
              <Box>
                <TextInput
                  label="Search"
                  placeholder="Search products..."
                  value={keyword}
                  onChange={(e) => setKeyword(e.currentTarget.value)}
                  rightSection={
                    <FaSearch
                      size={16}
                      style={{ cursor: 'pointer' }}
                      onClick={handleSearch}
                    />
                  }
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                />
              </Box>

              {/* Price range filter */}
              <Box>
                <Text fw={500} size="sm" mb={5}>
                  Price Range
                </Text>
                <Group grow mb="xs">
                  <NumberInput
                    placeholder="Min"
                    value={minPrice}
                    onChange={(val) =>
                      setMinPrice((val as number | undefined) || undefined)
                    }
                    min={0}
                  />
                  <NumberInput
                    placeholder="Max"
                    value={maxPrice}
                    onChange={(val) =>
                      setMaxPrice((val as number | undefined) || undefined)
                    }
                    min={minPrice || 0}
                  />
                </Group>
                <Button
                  variant="light"
                  leftSection={<FaFilter size={14} />}
                  onClick={handlePriceFilter}
                  fullWidth
                >
                  Apply Price Filter
                </Button>
              </Box>

              {/* Categories and subcategories filter */}
              <Box>
                <Text fw={500} size="sm" mb={5}>
                  Categories
                </Text>
                {loadingCategories ? (
                  <Flex justify="center" py="md">
                    <Loader size="sm" />
                  </Flex>
                ) : (
                  <Stack gap="xs">
                    <Radio.Group
                      value={
                        currentSubcategories != undefined &&
                        currentSubcategories.length > 0
                          ? 'subcategories' // Custom value when subcategories are selected
                          : currentCategory?.toString() || ''
                      }
                      onChange={(value) =>
                        handleCategoryChange(
                          value && value !== 'subcategories'
                            ? Number(value)
                            : null
                        )
                      }
                    >
                      <Stack gap="xs">
                        <Radio value="" label="All Categories" />
                        {categories?.map((category) => (
                          <Box key={category.id}>
                            <Radio
                              value={category.id.toString()}
                              label={category.name}
                            />

                            {/* Show subcategories regardless of selection status */}
                            <Box ml="md" mt="xs">
                              <Stack gap="xs">
                                {category.subcategories.map((subcategory) => (
                                  <Checkbox
                                    key={subcategory.id}
                                    label={subcategory.name}
                                    checked={currentSubcategories?.includes(
                                      subcategory.id
                                    )}
                                    onChange={(event) => {
                                      handleSubcategoryChange(
                                        subcategory.id,
                                        event.currentTarget.checked
                                      );
                                    }}
                                  />
                                ))}
                              </Stack>
                            </Box>
                          </Box>
                        ))}
                      </Stack>
                    </Radio.Group>
                  </Stack>
                )}
              </Box>

              {/* Reset filters button */}
              <Button variant="outline" color="gray" onClick={resetFilters}>
                Reset All Filters
              </Button>
            </Stack>
          </Paper>
        </Grid.Col>

        {/* Main content area */}
        <Grid.Col span={{ base: 12, md: 9 }}>
          <Box mb="md">
            <Group justify="space-between" wrap="nowrap">
              <Group align="flex-end" gap="xs">
                <Box>
                  <Text size="sm" mb={5}>
                    Sort by
                  </Text>
                  <Select
                    data={[
                      { value: 'name', label: 'Name' },
                      { value: 'price', label: 'Price' },
                    ]}
                    value={currentSortBy}
                    onChange={(value) =>
                      value && handleSortChange(value, currentSortDirection)
                    }
                    w={150}
                  />
                </Box>
                <Button.Group>
                  <Button
                    variant={
                      currentSortDirection === 'asc' ? 'filled' : 'outline'
                    }
                    onClick={() => handleSortChange(currentSortBy, 'asc')}
                    leftSection={<FaSortAmountUp size={14} />}
                  >
                    Asc
                  </Button>
                  <Button
                    variant={
                      currentSortDirection === 'desc' ? 'filled' : 'outline'
                    }
                    onClick={() => handleSortChange(currentSortBy, 'desc')}
                    leftSection={<FaSortAmountDown size={14} />}
                  >
                    Desc
                  </Button>
                </Button.Group>
              </Group>

              {products && products.totalCount > 0 && (
                <Text size="sm" c="dimmed">
                  Showing{' '}
                  {Math.min(
                    (currentPageNumber - 1) * currentPageSize + 1,
                    products.totalCount
                  )}{' '}
                  -{' '}
                  {Math.min(
                    currentPageNumber * currentPageSize,
                    products.totalCount
                  )}{' '}
                  of {products.totalCount} products
                </Text>
              )}
            </Group>
          </Box>

          {/* Loading indicator */}
          {loadingProducts && (
            <Flex justify="center" align="center" h={300}>
              <Loader size="lg" />
            </Flex>
          )}

          {/* Products grid */}
          {!loadingProducts && products && (
            <>
              {products.items.length === 0 ? (
                <Flex direction="column" align="center" py="xl">
                  <Text fw={500} size="lg" mb="md">
                    No products found
                  </Text>
                  <Text c="dimmed">
                    Try adjusting your filters or search term
                  </Text>
                </Flex>
              ) : (
                <>
                  <Grid>
                    {products.items.map((product) => (
                      <Grid.Col
                        key={product.id}
                        span={{ base: 12, xs: 6, sm: 4, md: 4, lg: 3 }}
                      >
                        <ProductCard product={product} />
                      </Grid.Col>
                    ))}
                  </Grid>

                  {/* Pagination */}
                  {totalPages > 1 && (
                    <Flex justify="center" mt="xl">
                      <Pagination
                        value={currentPageNumber}
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
        </Grid.Col>
      </Grid>
    </Container>
  );
}

export default ProductSearchPage;

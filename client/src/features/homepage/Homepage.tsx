import { useProducts } from '../../lib/hooks/useProducts';
import { useCategories } from '../../lib/hooks/useCategories';
import {
  Box,
  Container,
  Title,
  Group,
  Anchor,
  Skeleton,
  Card,
  Text,
  SimpleGrid,
  Center,
  Flex,
  Loader,
  Paper,
} from '@mantine/core';
import { Carousel } from '@mantine/carousel';
import ProductCard from '../products/ProductCard';
import { useEffect, useState } from 'react';
import { PopularProductResponseDto } from '../../lib/types';
import classes from './Homepage.module.css';
import { Link } from 'react-router';

interface GroupedProducts {
  [categoryId: number]: {
    categoryName: string;
    products: PopularProductResponseDto[];
  };
}

function Homepage() {
  const { popularProducts, loadingPopularProducts } = useProducts();
  const { categories, loadingCategories } = useCategories();
  const [groupedProducts, setGroupedProducts] = useState<GroupedProducts>({});

  useEffect(() => {
    if (!popularProducts) return;

    const grouped: GroupedProducts = {};
    popularProducts.forEach((product) => {
      if (!grouped[product.categoryId]) {
        grouped[product.categoryId] = {
          categoryName: product.categoryName,
          products: [],
        };
      }
      grouped[product.categoryId].products.push(product);
    });

    setGroupedProducts(grouped);
  }, [popularProducts]);

  return (
    <Container size="xl" py="xl">
      {/* Categories Grid */}
      <Box mb="lg">
        <Title order={2} mb="lg" style={{ textAlign: 'center' }}>
          Categories
        </Title>

        {loadingCategories ? (
          <Flex justify="center" py="md">
            <Loader size="md" />
          </Flex>
        ) : (
          <SimpleGrid
            cols={{ base: 2, sm: 3, md: 4, lg: 6 }}
            spacing={{ base: 10, sm: 'md' }}
          >
            {categories?.map((category) => (
              <Paper
                key={category.id}
                component={Link}
                to={`/products/search?categoryId=${category.id}`}
                shadow="sm"
                p="md"
                radius="md"
                withBorder
                className={classes.categoryCard}
              >
                <Center style={{ height: '100%' }}>
                  <Text fw={500} ta="center" c={'black'}>
                    {category.name}
                  </Text>
                </Center>
              </Paper>
            ))}
          </SimpleGrid>
        )}
      </Box>

      {/* Popular Products by Category */}
      {loadingPopularProducts
        ? // Loading skeleton
          Array.from({ length: 3 }).map((_, i) => (
            <Box key={i} mb="xl">
              <Skeleton height={30} width={200} mb="md" />
              <Skeleton height={220} radius="md" />
            </Box>
          ))
        : Object.entries(groupedProducts).map(([categoryId, category]) => (
            <Box key={categoryId} mb="xl">
              <Group mb="md" justify="space-between">
                <Title order={3}>
                  <Anchor
                    to={`/products/search?categoryId=${categoryId}`}
                    component={Link}
                    underline="hover"
                    fw={600}
                  >
                    {category.categoryName}
                  </Anchor>
                </Title>
              </Group>

              <Carousel
                slideSize="0%"
                slideGap="md"
                align="start"
                slidesToScroll={3}
                containScroll="trimSnaps"
                classNames={classes}
                className="homepage-product-carousel"
              >
                {category.products.map((product: PopularProductResponseDto) => (
                  <Carousel.Slide key={product.productId}>
                    <ProductCard product={product} />
                  </Carousel.Slide>
                ))}
                <Carousel.Slide>
                  <Card
                    component={Link}
                    to={`/products/search?categoryId=${categoryId}`}
                    shadow="sm"
                    padding="md"
                    radius="md"
                    withBorder
                    style={{
                      width: 180,
                      height: 255,
                      textDecoration: 'none',
                      display: 'flex',
                      flexDirection: 'column',
                      justifyContent: 'center',
                      alignItems: 'center',
                      backgroundColor: '#f8f9fa',
                      transition: 'transform 0.2s, box-shadow 0.2s',
                      cursor: 'pointer',
                    }}
                    classNames={classes}
                  >
                    <Box
                      style={{
                        textAlign: 'center',
                        padding: '20px 0',
                      }}
                    >
                      <Text size="xl" fw={500} mb="xs">
                        See All
                      </Text>
                      <Text size="sm" c="dimmed">
                        More in {category.categoryName}
                      </Text>
                    </Box>
                  </Card>
                </Carousel.Slide>
              </Carousel>
            </Box>
          ))}
    </Container>
  );
}

export default Homepage;

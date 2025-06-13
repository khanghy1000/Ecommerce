import { useParams, useNavigate } from 'react-router';
import { useProducts } from '../../lib/hooks/useProducts';
import { useCart } from '../../lib/hooks/useCart';
import { useAppStore } from '../../lib/hooks/useAppStore';
import {
  Container,
  Group,
  Text,
  Title,
  Badge,
  Grid,
  Divider,
  Box,
  Image,
  Skeleton,
  Button,
  NumberInput,
  Anchor,
} from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { Carousel } from '@mantine/carousel';
import { formatPrice } from '../../lib/utils';
import { FiShoppingCart, FiShoppingBag, FiCheckCircle } from 'react-icons/fi';
import { useState } from 'react';
import { Link } from 'react-router';
import ProductReviews from '../reviews/ProductReviews';

function ProductPage() {
  const { productId } = useParams();
  const navigate = useNavigate();
  const { product, loadingProduct } = useProducts(
    productId ? parseInt(productId) : undefined
  );
  const { addToCart } = useCart();
  const { disableAllCartItems, enableCartItem } = useAppStore();
  const [quantity, setQuantity] = useState(1);

  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

  const discountPercent = product?.discountPrice
    ? Math.round(
        ((product.regularPrice - product.discountPrice) /
          product.regularPrice) *
          100
      )
    : 0;

  const handleAddToCart = () => {
    if (product && productId) {
      const productIdNumber = parseInt(productId);

      addToCart.mutate(
        { productId: productIdNumber, quantity },
        {
          onSuccess: () => {
            notifications.show({
              title: 'Product added to cart',
              message: `${quantity} x ${product.name} has been added to your cart`,
              color: 'green',
              icon: <FiCheckCircle />,
            });
          },
        }
      );
    }
  };

  const handleBuyNow = () => {
    if (product && productId) {
      const productIdNumber = parseInt(productId);

      addToCart.mutate(
        { productId: productIdNumber, quantity },
        {
          onSuccess: () => {
            disableAllCartItems();
            enableCartItem(productIdNumber);
            navigate('/cart');
          },
        }
      );
    }
  };

  if (loadingProduct) {
    return (
      <Container size="xl" py="xl">
        <Grid>
          <Grid.Col span={{ base: 12, md: 6 }}>
            <Skeleton height={400} radius="md" mb="md" />
            <Group gap="xs" mb="md">
              <Skeleton height={80} width={80} radius="md" />
              <Skeleton height={80} width={80} radius="md" />
              <Skeleton height={80} width={80} radius="md" />
            </Group>
          </Grid.Col>
          <Grid.Col span={{ base: 12, md: 6 }}>
            <Skeleton height={40} width="70%" mb="md" />
            <Skeleton height={30} width="40%" mb="md" />
            <Skeleton height={20} width="30%" mb="xl" />
            <Skeleton height={36} width={320} />
          </Grid.Col>
          <Grid.Col span={12}>
            <Skeleton height={30} width="200" mt="xl" mb="md" />
            <Skeleton height={120} mb="md" />
          </Grid.Col>
        </Grid>
      </Container>
    );
  }

  if (!product) {
    return (
      <Container size="xl" py="xl">
        <Text>Product not found</Text>
      </Container>
    );
  }

  return (
    <Container size="xl" py="xl">
      <Grid gutter="xl">
        {/* Product images carousel - Left column */}
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Carousel
            withIndicators
            loop
            height={400}
            styles={{
              indicators: {
                bottom: -30,
              },
              control: {
                '&[data-inactive]': {
                  opacity: 0,
                  cursor: 'default',
                },
              },
            }}
          >
            {product.photos.map((photo) => (
              <Carousel.Slide key={photo.key}>
                <Box
                  style={{
                    height: 400,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: '#f8f9fa',
                  }}
                >
                  <Image
                    src={`${baseImageUrl}${photo.key}`}
                    alt={product.name}
                    fit="contain"
                    h={400}
                  />
                </Box>
              </Carousel.Slide>
            ))}
            {product.photos.length === 0 && (
              <Carousel.Slide>
                <Box
                  style={{
                    height: 400,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: '#f8f9fa',
                  }}
                >
                  <Image
                    src="/placeholder.svg"
                    alt="No image available"
                    fit="contain"
                    h={400}
                  />
                </Box>
              </Carousel.Slide>
            )}
          </Carousel>

          {/* Thumbnail gallery */}
          {product.photos.length > 0 && (
            <Group mt="xl" gap="xs" justify="center">
              {product.photos.map((photo) => (
                <Box
                  key={photo.key}
                  style={{
                    height: 80,
                    width: 80,
                    cursor: 'pointer',
                    border: '1px solid #e0e0e0',
                    borderRadius: '4px',
                    padding: '4px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <Image
                    src={`${baseImageUrl}${photo.key}`}
                    alt={`${product.name} thumbnail`}
                    fit="contain"
                    h={60}
                  />
                </Box>
              ))}
            </Group>
          )}
        </Grid.Col>

        {/* Product details - Right column */}
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Title order={1} mb="sm">
            {product.name}
          </Title>

          <Group mb="md">
            {discountPercent > 0 ? (
              <>
                <Text size="xl" fw={700} style={{ color: 'red' }}>
                  {formatPrice(product.discountPrice || 0)}
                </Text>
                <Text size="lg" td="line-through" c="dimmed">
                  {formatPrice(product.regularPrice)}
                </Text>
                <Badge color="red" variant="filled">
                  -{discountPercent}% OFF
                </Badge>
              </>
            ) : (
              <Text size="xl" fw={700} style={{ color: 'red' }}>
                {formatPrice(product.regularPrice)}
              </Text>
            )}
          </Group>

          {/* Availability */}
          <Group mb="xl">
            <NumberInput
              value={quantity}
              onChange={(val) => setQuantity(Number(val))}
              min={1}
              max={product.quantity}
              step={1}
              clampBehavior="strict"
              style={{ width: 100 }}
              disabled={product.quantity <= 0}
            />
            {product.quantity > 0 ? (
              <Text size="sm" c="dimmed">
                {product.quantity} items available
              </Text>
            ) : (
              <Text size="sm" c="red" fw={500}>
                Out of stock
              </Text>
            )}
          </Group>

          {/* Add to cart and buy buttons with quantity */}
          <Group mt="lg" mb="xl">
            <Button
              leftSection={<FiShoppingCart />}
              radius="md"
              disabled={product.quantity <= 0 || addToCart.isPending}
              onClick={handleAddToCart}
              loading={addToCart.isPending}
            >
              Add to Cart
            </Button>
            <Button
              leftSection={<FiShoppingBag />}
              radius="md"
              variant="filled"
              color="red"
              disabled={product.quantity <= 0 || addToCart.isPending}
              onClick={handleBuyNow}
              loading={addToCart.isPending}
            >
              Buy Now
            </Button>
          </Group>
        </Grid.Col>

        {/* Shop info and description - Full width below */}
        <Grid.Col span={12}>
          <Divider my="md" />

          {/* Shop info */}
          <Group mb="xl">
            <Image
              src={
                product.shopImageUrl
                  ? `${baseImageUrl}${product.shopImageUrl}`
                  : '/placeholder.svg'
              }
              alt={product.shopName}
              width={30}
              height={30}
              radius="xl"
            />
            <Anchor component={Link} to={`/shop/${product.shopId}`} fw={500}>
              {product.shopName}
            </Anchor>
          </Group>

          {/* Description */}
          <Title order={3} size="h4" mb="xs">
            Description
          </Title>
          <Box
            dangerouslySetInnerHTML={{ __html: product.description }}
            mb="xl"
          />

          {/* Reviews Section */}
          <Divider my="xl" />
          <ProductReviews productId={product.id} productName={product.name} />
        </Grid.Col>
      </Grid>
    </Container>
  );
}

export default ProductPage;

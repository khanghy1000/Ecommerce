import { useEffect } from 'react';
import { useCart } from '../../lib/hooks/useCart';
import { useAppStore } from '../../lib/hooks/useAppStore';
import {
  Container,
  Title,
  Table,
  Group,
  Text,
  Button,
  Image,
  NumberInput,
  Box,
  Paper,
  ActionIcon,
  Skeleton,
  Stack,
  Center,
  Checkbox,
  rem,
  Avatar,
} from '@mantine/core';
import { Link, useNavigate } from 'react-router';
import { formatPrice } from '../../lib/utils';
import { FiTrash2, FiShoppingBag } from 'react-icons/fi';

function CartPage() {
  const navigate = useNavigate();
  const { cartItems, isLoadingCart, updateCartItem, removeFromCart } =
    useCart();
  const { selectedCartItems, selectCartItem } = useAppStore();
  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

  // Initialize selected state for new cart items
  useEffect(() => {
    if (cartItems) {
      cartItems.forEach((item) => {
        if (selectedCartItems[item.productId] === undefined) {
          selectCartItem(item.productId, false); // Default to unselected
        }
      });
    }
  }, [cartItems, selectedCartItems, selectCartItem]);

  const handleQuantityChange = (productId: number, quantity: number) => {
    updateCartItem.mutate({ productId, quantity });
  };

  const handleRemoveItem = (productId: number) => {
    removeFromCart.mutate({ productId });
  };

  const handleToggleSelect = (productId: number, isChecked: boolean) => {
    selectCartItem(productId, isChecked);
  };

  const handleCheckout = () => {
    const selectedProductIds = cartItems
      ?.filter((item) => selectedCartItems[item.productId])
      .map((item) => item.productId);

    if (selectedProductIds && selectedProductIds.length > 0) {
      const searchParams = new URLSearchParams();
      selectedProductIds.forEach((id) => {
        searchParams.append('productId', id.toString());
      });

      navigate(`/checkout?${searchParams.toString()}`);
    }
  };

  // Group cart items by shop
  const groupedCartItems = cartItems
    ? Object.values(
        cartItems.reduce<
          Record<
            string,
            {
              shopId: string;
              shopName: string;
              shopImageUrl: string | null;
              items: typeof cartItems;
            }
          >
        >((acc, item) => {
          const key = item.shopId.toString();
          if (!acc[key]) {
            acc[key] = {
              shopId: item.shopId.toString(),
              shopName: item.shopName,
              shopImageUrl: item.shopImageUrl,
              items: [],
            };
          }
          acc[key].items.push(item);
          return acc;
        }, {})
      )
    : [];

  // Calculate selected items total
  const selectedItemsTotal = cartItems
    ? cartItems
        .filter((item) => selectedCartItems[item.productId])
        .reduce((sum, item) => sum + item.subtotal, 0)
    : 0;

  const selectedItemsCount = cartItems
    ? cartItems
        .filter((item) => selectedCartItems[item.productId])
        .reduce((sum, item) => sum + item.quantity, 0)
    : 0;

  const hasSelectedItems = selectedItemsCount > 0;

  if (isLoadingCart) {
    return (
      <Container size="xl" py="xl">
        <Title order={2} mb="lg">
          Shopping Cart
        </Title>
        <Paper shadow="xs" p="md" withBorder>
          <Stack>
            {[1, 2, 3].map((i) => (
              <Group key={i} wrap="nowrap" align="center" gap="md" mb="md">
                <Skeleton height={80} width={80} />
                <Skeleton height={20} width="40%" />
                <Skeleton height={40} width={100} ml="auto" />
              </Group>
            ))}
          </Stack>
        </Paper>
      </Container>
    );
  }

  if (!cartItems || cartItems.length === 0) {
    return (
      <Container size="xl" py="xl">
        <Title order={2} mb="lg">
          Shopping Cart
        </Title>
        <Paper shadow="xs" p="xl" withBorder>
          <Center>
            <Stack align="center" gap="md">
              <FiShoppingBag size={64} opacity={0.4} />
              <Text size="lg" fw={500}>
                Your cart is empty
              </Text>
              <Text size="sm" c="dimmed" ta="center">
                Looks like you haven't added any products to your cart yet.
              </Text>
              <Button component={Link} to="/products" variant="filled" mt="md">
                Continue Shopping
              </Button>
            </Stack>
          </Center>
        </Paper>
      </Container>
    );
  }

  return (
    <Container size="xl" py="xl">
      <Title order={2} mb="lg">
        Shopping Cart
      </Title>

      {/* Cart items grouped by shop */}
      <Stack mb="xl">
        {groupedCartItems.map((group) => (
          <Paper shadow="xs" p="md" withBorder key={group.shopId}>
            {/* Shop header */}
            <Group mb="sm">
              <Button
                variant="subtle"
                px="xs"
                component={Link}
                to={`/users/${group.shopId}`}
              >
                <Avatar
                  src={group.shopImageUrl}
                  alt={group.shopName}
                  size="sm"
                  style={{ marginRight: rem(5) }}
                />
                <Text size="sm" fw={500}>
                  {group.shopName}
                </Text>
              </Button>
            </Group>

            <Table>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th style={{ width: 40 }}></Table.Th>
                  <Table.Th>Product</Table.Th>
                  <Table.Th>Price</Table.Th>
                  <Table.Th>Quantity</Table.Th>
                  <Table.Th>Subtotal</Table.Th>
                  <Table.Th></Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {group.items.map((item) => (
                  <Table.Tr key={item.productId}>
                    <Table.Td>
                      <Checkbox
                        checked={!!selectedCartItems[item.productId]}
                        onChange={(event) =>
                          handleToggleSelect(
                            item.productId,
                            event.currentTarget.checked
                          )
                        }
                      />
                    </Table.Td>
                    <Table.Td>
                      <Group gap="md" wrap="nowrap">
                        <Image
                          src={
                            item.productImageUrl
                              ? `${baseImageUrl}${item.productImageUrl}`
                              : '/placeholder.svg'
                          }
                          alt={item.productName}
                          fit="contain"
                          style={{
                            width: '80px',
                            height: '80px',
                            borderRadius: '4px',
                          }}
                        />
                        <Box>
                          <Text
                            component={Link}
                            to={`/products/${item.productId}`}
                            lineClamp={2}
                            size="sm"
                            fw={500}
                          >
                            {item.productName}
                          </Text>
                        </Box>
                      </Group>
                    </Table.Td>
                    <Table.Td>
                      {item.discountPrice ? (
                        <Group gap="xs" wrap="nowrap">
                          <Text style={{ color: 'red' }} size="sm" fw={500}>
                            {formatPrice(item.discountPrice)}
                          </Text>
                          <Text
                            size="xs"
                            c="dimmed"
                            style={{ textDecoration: 'line-through' }}
                          >
                            {formatPrice(item.unitPrice)}
                          </Text>
                        </Group>
                      ) : (
                        <Text size="sm" fw={500}>
                          {formatPrice(item.unitPrice)}
                        </Text>
                      )}
                    </Table.Td>
                    <Table.Td>
                      <NumberInput
                        value={item.quantity}
                        onChange={(value) =>
                          handleQuantityChange(item.productId, Number(value))
                        }
                        min={1}
                        max={item.maxQuantity}
                        style={{ width: '100px' }}
                      />
                      {item.quantity >= item.maxQuantity && (
                        <Text size="xs" c="red" mt={4}>
                          Max quantity
                        </Text>
                      )}
                    </Table.Td>
                    <Table.Td>
                      <Text fw={500} style={{ color: 'red' }}>
                        {formatPrice(item.subtotal)}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <ActionIcon
                        color="red"
                        variant="subtle"
                        onClick={() => handleRemoveItem(item.productId)}
                        aria-label="Remove item"
                      >
                        <FiTrash2 />
                      </ActionIcon>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Paper>
        ))}
      </Stack>

      {/* Order summary */}
      <Paper shadow="xs" p="md" withBorder>
        <Group justify="space-between">
          <div>
            <Title order={4} mb="md">
              Order Summary
            </Title>
            <Group mb="md">
              <Text>Selected Items: {selectedItemsCount}</Text>
            </Group>
          </div>

          <div style={{ width: '300px' }}>
            <Group justify="space-between" mb="lg">
              <Text fw={700}>Subtotal:</Text>
              <Text style={{ color: 'red', fontSize: '1.2rem' }} fw={700}>
                {formatPrice(selectedItemsTotal)}
              </Text>
            </Group>

            {hasSelectedItems ? (
              <Button onClick={handleCheckout} fullWidth size="lg">
                Proceed to Checkout
              </Button>
            ) : (
              <Button fullWidth size="lg" disabled>
                Proceed to Checkout
              </Button>
            )}

            <Button component={Link} to="/" fullWidth variant="outline" mt="md">
              Continue Shopping
            </Button>
          </div>
        </Group>
      </Paper>
    </Container>
  );
}

export default CartPage;

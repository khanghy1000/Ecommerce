import { useEffect, useState } from 'react';
import { useCart } from '../../lib/hooks/useCart';
import { useAddresses } from '../../lib/hooks/useAddresses';
import { useOrders } from '../../lib/hooks/useOrders';
import { useNavigate, useSearchParams } from 'react-router';
import {
  Container,
  Title,
  Group,
  Text,
  Button,
  Image,
  Box,
  Paper,
  Stack,
  Radio,
  rem,
  Avatar,
  Divider,
  LoadingOverlay,
  Alert,
  Card,
  SegmentedControl,
  Flex,
  Modal,
  Badge,
  Loader,
} from '@mantine/core';
import { formatPrice } from '../../lib/utils';
import {
  CheckoutPricePreviewRequestDto,
  CheckoutRequestDto,
  PaymentMethod,
} from '../../lib/types';
import { Link } from 'react-router';
import {
  FiAlertCircle,
  FiArrowLeft,
  FiCheckCircle,
  FiCreditCard,
  FiEdit2,
  FiMapPin,
  FiTruck,
} from 'react-icons/fi';

function CheckoutPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { cartItems, isLoadingCart } = useCart();
  const { addresses, loadingAddresses, setDefaultAddress } = useAddresses();
  const [selectedAddressId, setSelectedAddressId] = useState<number | null>(
    null
  );
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Cod');
  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;
  const [addressModalOpened, setAddressModalOpened] = useState(false);

  // Get product IDs from URL search params
  const selectedProductIds = searchParams.getAll('productId').map(Number);

  // Find default address and set it as selected initially
  useEffect(() => {
    if (addresses?.length) {
      const defaultAddress = addresses.find((addr) => addr.isDefault);
      if (defaultAddress) {
        setSelectedAddressId(defaultAddress.id);
      } else if (addresses.length > 0) {
        setSelectedAddressId(addresses[0].id);
      }
    }
  }, [addresses]);

  // Create checkout preview request
  const selectedAddress = addresses?.find(
    (addr) => addr.id === selectedAddressId
  );

  // Prepare checkout preview request when we have all required information
  const previewRequest: CheckoutPricePreviewRequestDto | undefined =
    selectedAddress && selectedProductIds.length > 0
      ? {
          shippingName: selectedAddress.name,
          shippingPhone: selectedAddress.phoneNumber,
          shippingAddress: selectedAddress.address,
          shippingWardId: selectedAddress.wardId,
          productIds: selectedProductIds,
        }
      : undefined;

  // Get checkout price preview
  const { checkoutPreview, fetchingCheckoutPreview, checkout } = useOrders(
    undefined,
    undefined,
    previewRequest
  );

  // Handle address selection
  const handleAddressSelect = (addressId: number) => {
    setSelectedAddressId(addressId);
  };

  // Handle setting address as default
  const handleSetDefault = (addressId: number) => {
    setDefaultAddress.mutate(addressId);
  };

  // Handle checkout submission
  const handleCheckout = () => {
    if (!selectedAddress || selectedProductIds.length === 0) return;

    const checkoutData: CheckoutRequestDto = {
      shippingName: selectedAddress.name,
      shippingPhone: selectedAddress.phoneNumber,
      shippingAddress: selectedAddress.address,
      shippingWardId: selectedAddress.wardId,
      productIds: selectedProductIds,
      paymentMethod: paymentMethod,
    };

    checkout.mutate(checkoutData, {
      onSuccess: (data) => {
        if (data.paymentUrl) {
          // For online payment, redirect to payment gateway
          window.location.href = data.paymentUrl;
        } else {
          // For COD, redirect to order confirmation page
          navigate('/orders');
        }
      },
    });
  };

  // Filter cart items to only include selected products
  const selectedItems = cartItems
    ? cartItems.filter((item) => selectedProductIds.includes(item.productId))
    : [];

  const groupedSelectedItems =
    selectedItems.length > 0
      ? Object.values(
          selectedItems.reduce<
            Record<
              string,
              {
                shopId: string;
                shopName: string;
                shopImageUrl: string | null;
                items: typeof selectedItems;
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

  // Calculate totals
  const selectedItemsTotal = selectedItems.reduce(
    (sum, item) => sum + item.subtotal,
    0
  );
  const selectedItemsCount = selectedItems.reduce(
    (sum, item) => sum + item.quantity,
    0
  );

  if (isLoadingCart || loadingAddresses) {
    return (
      <Container size="xl" py="xl" style={{ position: 'relative' }}>
        <LoadingOverlay visible={true} />
      </Container>
    );
  }

  if (!selectedProductIds.length) {
    return (
      <Container size="xl" py="xl">
        <Alert
          icon={<FiAlertCircle size={16} />}
          title="No items selected for checkout"
          color="yellow"
        >
          You haven't selected any items to checkout. Please go back to your
          cart and select some items.
        </Alert>
        <Button
          component={Link}
          to="/cart"
          leftSection={<FiArrowLeft />}
          mt="md"
        >
          Return to Cart
        </Button>
      </Container>
    );
  }

  if (!cartItems || selectedItems.length === 0) {
    return (
      <Container size="xl" py="xl">
        <Alert
          icon={<FiAlertCircle size={16} />}
          title="Products not found"
          color="yellow"
        >
          The selected products could not be found in your cart. They might have
          been removed or are no longer available.
        </Alert>
        <Button
          component={Link}
          to="/cart"
          leftSection={<FiArrowLeft />}
          mt="md"
        >
          Return to Cart
        </Button>
      </Container>
    );
  }

  return (
    <Container size="md" py="xl">
      <Title order={2} mb="lg">
        Checkout
      </Title>

      <Stack gap="xl">
        {/* Shipping Address Section */}
        <Paper shadow="xs" p="md" withBorder>
          <Group justify="space-between" mb="md">
            <Title order={4}>Shipping Address</Title>
            <Button
              size="xs"
              variant="outline"
              leftSection={<FiEdit2 size={16} />}
              onClick={() => setAddressModalOpened(true)}
            >
              Change
            </Button>
          </Group>

          {addresses?.length === 0 ? (
            <Alert color="yellow">
              You don't have any saved addresses. Please add an address to
              continue.
            </Alert>
          ) : selectedAddress ? (
            <Card withBorder p="md">
              <Group mb={4}>
                <FiMapPin />
                <Text fw={500}>{selectedAddress.name}</Text>
                <Text size="sm" c="dimmed">
                  {selectedAddress.phoneNumber}
                </Text>
                {selectedAddress.isDefault && (
                  <Badge size="sm" color="blue">
                    DEFAULT
                  </Badge>
                )}
              </Group>
              <Text size="sm" ml={24}>
                {selectedAddress.address}, {selectedAddress.wardName},{' '}
                {selectedAddress.districtName}, {selectedAddress.provinceName}
              </Text>
            </Card>
          ) : (
            <Alert color="yellow">
              Please select a shipping address to continue.
            </Alert>
          )}

          <Modal
            opened={addressModalOpened}
            onClose={() => setAddressModalOpened(false)}
            title="Select Shipping Address"
            size="auto"
          >
            <Stack>
              {addresses?.map((address) => (
                <Card
                  key={address.id}
                  withBorder
                  p="sm"
                  style={{
                    cursor: 'pointer',
                    backgroundColor:
                      selectedAddressId === address.id
                        ? 'var(--mantine-color-gray-0)'
                        : undefined,
                    borderColor:
                      selectedAddressId === address.id
                        ? 'var(--mantine-color-blue-5)'
                        : undefined,
                  }}
                  onClick={() => handleAddressSelect(address.id)}
                >
                  <Group align="flex-start" justify="space-between">
                    <Group align="flex-start">
                      <Radio
                        checked={selectedAddressId === address.id}
                        onChange={() => handleAddressSelect(address.id)}
                        onClick={(e) => e.stopPropagation()}
                      />
                      <Box>
                        <Group mb={4}>
                          <Text fw={500}>{address.name}</Text>
                          <Text size="sm" c="dimmed">
                            {address.phoneNumber}
                          </Text>
                          {address.isDefault && (
                            <Badge size="sm" color="blue">
                              DEFAULT
                            </Badge>
                          )}
                        </Group>
                        <Group align="center">
                          <Text size="sm">
                            {address.address}, {address.wardName},{' '}
                            {address.districtName}, {address.provinceName}
                          </Text>
                          {!address.isDefault && (
                            <Button
                              variant="outline"
                              size="xs"
                              onClick={(e) => {
                                e.stopPropagation();
                                handleSetDefault(address.id);
                              }}
                            >
                              Set as default
                            </Button>
                          )}
                        </Group>
                      </Box>
                    </Group>
                  </Group>
                </Card>
              ))}
            </Stack>

            <Group justify="space-between" mt="xl">
              <Button
                component={Link}
                to="/account/addresses/new"
                variant="outline"
              >
                Add New Address
              </Button>
              <Button onClick={() => setAddressModalOpened(false)}>
                Confirm Selection
              </Button>
            </Group>
          </Modal>
        </Paper>

        {/* Order Items Section */}
        <Paper shadow="xs" p="md" withBorder>
          <Title order={4} mb="md">
            Order Items ({selectedItemsCount} items)
          </Title>

          <Stack gap="md">
            {groupedSelectedItems.map((group) => (
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

                <Divider mb="sm" />

                <Stack>
                  {group.items.map((item) => (
                    <Group
                      key={item.productId}
                      wrap="nowrap"
                      align="flex-start"
                      gap="md"
                    >
                      <Image
                        src={
                          item.productImageUrl
                            ? `${baseImageUrl}${item.productImageUrl}`
                            : '/placeholder.svg'
                        }
                        alt={item.productName}
                        fit="contain"
                        style={{
                          width: '60px',
                          height: '60px',
                          borderRadius: '4px',
                        }}
                      />
                      <Box style={{ flex: 1 }}>
                        <Text
                          component={Link}
                          to={`/products/${item.productId}`}
                          lineClamp={2}
                          size="sm"
                          fw={500}
                        >
                          {item.productName}
                        </Text>
                        <Text size="xs" c="dimmed" mt={4}>
                          Quantity: {item.quantity}
                        </Text>
                      </Box>
                      <Box>
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
                        <Text
                          size="sm"
                          fw={500}
                          style={{ color: 'red' }}
                          mt={4}
                        >
                          {formatPrice(item.subtotal)}
                        </Text>
                      </Box>
                    </Group>
                  ))}
                </Stack>
              </Paper>
            ))}
          </Stack>
        </Paper>

        {/* Payment Method Section */}
        <Paper shadow="xs" p="md" withBorder>
          <Title order={4} mb="md">
            Payment Method
          </Title>

          <SegmentedControl
            value={paymentMethod}
            onChange={(value) => setPaymentMethod(value as PaymentMethod)}
            data={[
              {
                value: 'Cod',
                label: (
                  <Group gap="xs">
                    <FiTruck size={16} />
                    <Text>Cash on Delivery</Text>
                  </Group>
                ),
              },
              {
                value: 'Vnpay',
                label: (
                  <Group gap="xs">
                    <FiCreditCard size={16} />
                    <Text>Online Payment</Text>
                  </Group>
                ),
              },
            ]}
            fullWidth
          />
        </Paper>

        {/* Order Summary */}
        <Paper shadow="xs" p="md" withBorder>
          <Title order={4} mb="md">
            Order Summary
          </Title>

          <Stack>
            <Group justify="space-between">
              <Text>Subtotal ({selectedItemsCount} items)</Text>
              <Text fw={500}>{formatPrice(selectedItemsTotal)}</Text>
            </Group>

            <Group justify="space-between">
              <Text>Shipping Fee</Text>
              {fetchingCheckoutPreview ? (
                <Loader size="sm" />
              ) : (
                <Text fw={500}>
                  {checkoutPreview
                    ? formatPrice(checkoutPreview.shippingFee)
                    : 'Calculating...'}
                </Text>
              )}
            </Group>

            {checkoutPreview && checkoutPreview.productDiscountAmount > 0 && (
              <Group justify="space-between">
                <Text>Product Discount</Text>
                <Text fw={500} c="green">
                  -{formatPrice(checkoutPreview.productDiscountAmount)}
                </Text>
              </Group>
            )}

            {checkoutPreview && checkoutPreview.shippingDiscountAmount > 0 && (
              <Group justify="space-between">
                <Text>Shipping Discount</Text>
                <Text fw={500} c="green">
                  -{formatPrice(checkoutPreview.shippingDiscountAmount)}
                </Text>
              </Group>
            )}

            <Divider />

            <Group justify="space-between">
              <Text fw={700}>Total</Text>
              {fetchingCheckoutPreview ? (
                <Loader size="sm" />
              ) : (
                <Text style={{ color: 'red', fontSize: '1.2rem' }} fw={700}>
                  {checkoutPreview
                    ? formatPrice(checkoutPreview.total)
                    : formatPrice(selectedItemsTotal)}
                </Text>
              )}
            </Group>
          </Stack>

          <Flex direction="column" gap="md" mt="xl">
            <Button
              fullWidth
              size="lg"
              disabled={
                !selectedAddress ||
                fetchingCheckoutPreview ||
                checkout.isPending
              }
              onClick={handleCheckout}
              leftSection={<FiCheckCircle />}
              loading={fetchingCheckoutPreview}
            >
              {checkout.isPending
                ? 'Processing...'
                : `Place Order - ${formatPrice(checkoutPreview?.total || selectedItemsTotal)}`}
            </Button>

            <Button
              component={Link}
              to="/cart"
              fullWidth
              variant="outline"
              leftSection={<FiArrowLeft />}
            >
              Back to Cart
            </Button>
          </Flex>
        </Paper>
      </Stack>
    </Container>
  );
}

export default CheckoutPage;

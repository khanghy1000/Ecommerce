import { useEffect, useState } from 'react';
import { useCart } from '../../lib/hooks/useCart';
import { useAddresses } from '../../lib/hooks/useAddresses';
import { useOrders } from '../../lib/hooks/useOrders';
import { useCoupons } from '../../lib/hooks/useCoupons';
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
  Table,
  Center,
} from '@mantine/core';
import { formatPrice } from '../../lib/utils';
import {
  CheckoutPricePreviewRequestDto,
  CheckoutRequestDto,
  CouponResponseDto,
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
  FiTag,
  FiTruck,
} from 'react-icons/fi';
import { format } from 'date-fns';

function CheckoutPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { cartItems, isLoadingCart } = useCart();
  const { addresses, loadingAddresses, setDefaultAddress } = useAddresses();
  const { coupons, loadingCoupons } = useCoupons();
  const [selectedAddressId, setSelectedAddressId] = useState<number | null>(
    null
  );
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Cod');
  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;
  const [addressModalOpened, setAddressModalOpened] = useState(false);
  const [productCouponModalOpened, setProductCouponModalOpened] =
    useState(false);
  const [shippingCouponModalOpened, setShippingCouponModalOpened] =
    useState(false);
  const [selectedProductCoupon, setSelectedProductCoupon] = useState<
    string | undefined
  >(undefined);
  const [selectedShippingCoupon, setSelectedShippingCoupon] = useState<
    string | undefined
  >(undefined);

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

  // Filter coupons by type
  const productCoupons =
    coupons?.filter(
      (coupon) =>
        coupon.type === 'Product' &&
        coupon.active &&
        new Date(coupon.endTime) > new Date()
    ) || [];

  const shippingCoupons =
    coupons?.filter(
      (coupon) =>
        coupon.type === 'Shipping' &&
        coupon.active &&
        new Date(coupon.endTime) > new Date()
    ) || [];

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
          productCouponCode: selectedProductCoupon,
          shippingCouponCode: selectedShippingCoupon,
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

  // Handle coupon selection
  const handleProductCouponSelect = (code: string | undefined) => {
    setSelectedProductCoupon(code);
    setProductCouponModalOpened(false);
  };

  const handleShippingCouponSelect = (code: string | undefined) => {
    setSelectedShippingCoupon(code);
    setShippingCouponModalOpened(false);
  };

  // Format coupon value for display
  const formatCouponValue = (coupon: CouponResponseDto) => {
    if (coupon.discountType === 'Percent') {
      return `${coupon.value}%`;
    } else {
      return formatPrice(coupon.value);
    }
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
      productCouponCode: selectedProductCoupon,
      shippingCouponCode: selectedShippingCoupon,
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

  if (isLoadingCart || loadingAddresses || loadingCoupons) {
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

        {/* Coupons Section */}
        <Paper shadow="xs" p="md" withBorder>
          <Title order={4} mb="md">
            Coupons
          </Title>

          <Group justify='space-between'>
            {/* Product Coupon */}
            <Group>
              <Group>
                <FiTag size={18} />
                <Box>
                  <Text fw={500}>Product Coupon</Text>
                  {selectedProductCoupon ? (
                    <Text size="sm" c="green">
                      Coupon applied: {selectedProductCoupon}
                    </Text>
                  ) : (
                    <Text size="sm" c="dimmed">
                      No coupon applied
                    </Text>
                  )}
                </Box>
              </Group>
              <Button
                variant="outline"
                onClick={() => setProductCouponModalOpened(true)}
              >
                {selectedProductCoupon ? 'Change' : 'Select Coupon'}
              </Button>
            </Group>

            {/* Shipping Coupon */}
            <Group>
              <Group>
                <FiTruck size={18} />
                <Box>
                  <Text fw={500}>Shipping Coupon</Text>
                  {selectedShippingCoupon ? (
                    <Text size="sm" c="green">
                      Coupon applied: {selectedShippingCoupon}
                    </Text>
                  ) : (
                    <Text size="sm" c="dimmed">
                      No coupon applied
                    </Text>
                  )}
                </Box>
              </Group>
              <Button
                variant="outline"
                onClick={() => setShippingCouponModalOpened(true)}
              >
                {selectedShippingCoupon ? 'Change' : 'Select Coupon'}
              </Button>
            </Group>
          </Group>

          {/* Product Coupon Modal */}
          <Modal
            opened={productCouponModalOpened}
            onClose={() => setProductCouponModalOpened(false)}
            title="Select Product Coupon"
            size="lg"
          >
            {productCoupons.length > 0 ? (
              <Stack>
                <Button
                  variant="subtle"
                  color="red"
                  onClick={() => handleProductCouponSelect(undefined)}
                >
                  Remove Coupon
                </Button>
                <Table>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Code</Table.Th>
                      <Table.Th>Discount</Table.Th>
                      <Table.Th>Min Order</Table.Th>
                      <Table.Th>Valid Until</Table.Th>
                      <Table.Th>Action</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {productCoupons.map((coupon) => (
                      <Table.Tr key={coupon.code}>
                        <Table.Td>
                          <Group>
                            <FiTag size={16} />
                            <Text fw={500}>{coupon.code}</Text>
                          </Group>
                        </Table.Td>
                        <Table.Td>
                          <Badge color="green">
                            {formatCouponValue(coupon)}
                            {coupon.maxDiscountAmount > 0 &&
                              coupon.discountType === 'Percent' &&
                              ` (max ${formatPrice(coupon.maxDiscountAmount)})`}
                          </Badge>
                        </Table.Td>
                        <Table.Td>
                          {coupon.minOrderValue > 0
                            ? formatPrice(coupon.minOrderValue)
                            : 'None'}
                        </Table.Td>
                        <Table.Td>
                          {format(new Date(coupon.endTime), 'dd/MM/yyyy')}
                        </Table.Td>
                        <Table.Td>
                          <Button
                            size="xs"
                            onClick={() =>
                              handleProductCouponSelect(coupon.code)
                            }
                            variant={
                              selectedProductCoupon === coupon.code
                                ? 'filled'
                                : 'outline'
                            }
                          >
                            {selectedProductCoupon === coupon.code
                              ? 'Selected'
                              : 'Select'}
                          </Button>
                        </Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              </Stack>
            ) : (
              <Center py="xl">
                <Alert color="yellow">
                  No product coupons available at the moment.
                </Alert>
              </Center>
            )}
            <Group justify="flex-end" mt="xl">
              <Button onClick={() => setProductCouponModalOpened(false)}>
                Close
              </Button>
            </Group>
          </Modal>

          {/* Shipping Coupon Modal */}
          <Modal
            opened={shippingCouponModalOpened}
            onClose={() => setShippingCouponModalOpened(false)}
            title="Select Shipping Coupon"
            size="lg"
          >
            {shippingCoupons.length > 0 ? (
              <Stack>
                <Button
                  variant="subtle"
                  color="red"
                  onClick={() => handleShippingCouponSelect(undefined)}
                >
                  Remove Coupon
                </Button>
                <Table>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Code</Table.Th>
                      <Table.Th>Discount</Table.Th>
                      <Table.Th>Min Order</Table.Th>
                      <Table.Th>Valid Until</Table.Th>
                      <Table.Th>Action</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {shippingCoupons.map((coupon) => (
                      <Table.Tr key={coupon.code}>
                        <Table.Td>
                          <Group>
                            <FiTruck size={16} />
                            <Text fw={500}>{coupon.code}</Text>
                          </Group>
                        </Table.Td>
                        <Table.Td>
                          <Badge color="blue">
                            {formatCouponValue(coupon)}
                            {coupon.maxDiscountAmount > 0 &&
                              coupon.discountType === 'Percent' &&
                              ` (max ${formatPrice(coupon.maxDiscountAmount)})`}
                          </Badge>
                        </Table.Td>
                        <Table.Td>
                          {coupon.minOrderValue > 0
                            ? formatPrice(coupon.minOrderValue)
                            : 'None'}
                        </Table.Td>
                        <Table.Td>
                          {format(new Date(coupon.endTime), 'dd/MM/yyyy')}
                        </Table.Td>
                        <Table.Td>
                          <Button
                            size="xs"
                            onClick={() =>
                              handleShippingCouponSelect(coupon.code)
                            }
                            variant={
                              selectedShippingCoupon === coupon.code
                                ? 'filled'
                                : 'outline'
                            }
                          >
                            {selectedShippingCoupon === coupon.code
                              ? 'Selected'
                              : 'Select'}
                          </Button>
                        </Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              </Stack>
            ) : (
              <Center py="xl">
                <Alert color="yellow">
                  No shipping coupons available at the moment.
                </Alert>
              </Center>
            )}
            <Group justify="flex-end" mt="xl">
              <Button onClick={() => setShippingCouponModalOpened(false)}>
                Close
              </Button>
            </Group>
          </Modal>
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
                <Group gap={5}>
                  {checkoutPreview.appliedProductCoupon && (
                    <Badge size="xs" color="green">
                      {checkoutPreview.appliedProductCoupon}
                    </Badge>
                  )}
                  <Text fw={500} c="green">
                    -{formatPrice(checkoutPreview.productDiscountAmount)}
                  </Text>
                </Group>
              </Group>
            )}

            {checkoutPreview && checkoutPreview.shippingDiscountAmount > 0 && (
              <Group justify="space-between">
                <Text>Shipping Discount</Text>
                <Group gap={5}>
                  {checkoutPreview.appliedShippingCoupon && (
                    <Badge size="xs" color="blue">
                      {checkoutPreview.appliedShippingCoupon}
                    </Badge>
                  )}
                  <Text fw={500} c="green">
                    -{formatPrice(checkoutPreview.shippingDiscountAmount)}
                  </Text>
                </Group>
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

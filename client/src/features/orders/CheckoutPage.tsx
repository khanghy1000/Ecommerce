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
  TextInput,
  Select,
  ActionIcon,
} from '@mantine/core';
import { formatPrice } from '../../lib/utils';
import {
  CartItemResponseDto,
  CheckoutPricePreviewRequestDto,
  CheckoutPricePreviewResponseDto,
  CheckoutRequestDto,
  CouponResponseDto,
  PaymentMethod,
  UserAddressResponseDto,
  AddUserAddressRequestDto,
  EditUserAddressRequestDto,
} from '../../lib/types';
import { Link } from 'react-router';
import {
  FiAlertCircle,
  FiArrowLeft,
  FiCheckCircle,
  FiCreditCard,
  FiEdit2,
  FiMapPin,
  FiPlus,
  FiTag,
  FiTrash2,
  FiTruck,
} from 'react-icons/fi';
import { format } from 'date-fns';
import { useForm } from '@mantine/form';
import { useLocations } from '../../lib/hooks/useLocations';

// Format coupon value for display
const formatCouponValue = (coupon: CouponResponseDto) => {
  if (coupon.discountType === 'Percent') {
    return `${coupon.value}%`;
  } else {
    return formatPrice(coupon.value);
  }
};

// AddressForm component for add/edit address
type AddressFormProps = {
  initialValues?: UserAddressResponseDto;
  onSubmit: (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => void;
  onCancel: () => void;
  isSubmitting: boolean;
};

const AddressForm = ({
  initialValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: AddressFormProps) => {
  const [selectedProvinceId, setSelectedProvinceId] = useState<
    number | undefined
  >(initialValues?.wardId ? undefined : undefined);
  const [selectedDistrictId, setSelectedDistrictId] = useState<
    number | undefined
  >(initialValues?.wardId ? undefined : undefined);

  // Use the useLocations hook to get provinces, districts, and wards
  const {
    provinces,
    districts,
    wards,
    loadingProvinces,
    loadingDistricts,
    loadingWards,
  } = useLocations(selectedProvinceId, selectedDistrictId);

  const form = useForm<AddUserAddressRequestDto | EditUserAddressRequestDto>({
    mode: 'uncontrolled',
    initialValues: initialValues
      ? {
          name: initialValues.name,
          phoneNumber: initialValues.phoneNumber,
          address: initialValues.address,
          wardId: initialValues.wardId,
          isDefault: initialValues.isDefault,
        }
      : {
          name: '',
          phoneNumber: '',
          address: '',
          wardId: 0,
          isDefault: false,
        },
  });

  // Find the ward, district, and province for the selected address
  useEffect(() => {
    if (initialValues?.wardId && wards.length > 0) {
      const ward = wards.find((w) => w.id === initialValues.wardId);
      if (ward) {
        setSelectedDistrictId(ward.districtId);
      }
    }
  }, [initialValues?.wardId, wards]);

  useEffect(() => {
    if (selectedDistrictId && districts.length > 0) {
      const district = districts.find((d) => d.id === selectedDistrictId);
      if (district) {
        setSelectedProvinceId(district.provinceId);
      }
    }
  }, [selectedDistrictId, districts]);

  const handleProvinceChange = (value: string | null) => {
    if (value) {
      const provinceId = parseInt(value);
      setSelectedProvinceId(provinceId);
      form.setFieldValue('provinceId', provinceId);

      // Reset district and ward selections
      setSelectedDistrictId(undefined);
      form.setFieldValue('districtId', 0);
      form.setFieldValue('wardId', 0);
    }
  };

  const handleDistrictChange = (value: string | null) => {
    if (value) {
      const districtId = parseInt(value);
      setSelectedDistrictId(districtId);
      form.setFieldValue('districtId', districtId);

      // Reset ward selection
      form.setFieldValue('wardId', 0);
    }
  };

  const handleSubmit = (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => {
    onSubmit(values);
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack>
        <TextInput
          required
          label="Full Name"
          placeholder="Enter your full name"
          {...form.getInputProps('name')}
        />

        <TextInput
          required
          label="Phone Number"
          placeholder="Enter your phone number"
          {...form.getInputProps('phoneNumber')}
        />

        <TextInput
          required
          label="Address"
          placeholder="Street address, apartment, etc."
          {...form.getInputProps('address')}
        />

        <Select
          required
          label="Province"
          placeholder="Select province"
          data={
            provinces?.map((province) => ({
              value: province.id.toString(),
              label: province.name,
            })) || []
          }
          value={selectedProvinceId?.toString()}
          onChange={handleProvinceChange}
          disabled={loadingProvinces}
        />

        <Select
          required
          label="District"
          placeholder="Select district"
          data={
            districts?.map((district) => ({
              value: district.id.toString(),
              label: district.name,
            })) || []
          }
          value={selectedDistrictId?.toString()}
          onChange={handleDistrictChange}
          disabled={loadingDistricts || !selectedProvinceId}
        />

        <Select
          required
          label="Ward"
          placeholder="Select ward"
          data={
            wards?.map((ward) => ({
              value: ward.id.toString(),
              label: ward.name,
            })) || []
          }
          onChange={(value) => {
            if (value) {
              form.setFieldValue('wardId', parseInt(value));
            }
          }}
          disabled={loadingWards || !selectedDistrictId}
        />

        <Flex align="center" gap="md">
          <Radio
            label="Set as default address"
            checked={form.values.isDefault}
            onChange={(event) =>
              form.setFieldValue('isDefault', event.currentTarget.checked)
            }
          />
        </Flex>

        <Group justify="flex-end" mt="md">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" loading={isSubmitting}>
            {initialValues ? 'Update Address' : 'Add Address'}
          </Button>
        </Group>
      </Stack>
    </form>
  );
};

// ShippingAddressCard component
type ShippingAddressCardProps = {
  address: UserAddressResponseDto;
  isDefault: boolean;
};

const ShippingAddressCard = ({
  address,
  isDefault,
}: ShippingAddressCardProps) => {
  return (
    <Card withBorder p="md">
      <Group mb={4}>
        <FiMapPin />
        <Text fw={500}>{address.name}</Text>
        <Text size="sm" c="dimmed">
          {address.phoneNumber}
        </Text>
        {isDefault && (
          <Badge size="sm" color="blue">
            DEFAULT
          </Badge>
        )}
      </Group>
      <Text size="sm" ml={24}>
        {address.address}, {address.wardName}, {address.districtName},{' '}
        {address.provinceName}
      </Text>
    </Card>
  );
};

// AddressSelectionModal component
type AddressSelectionModalProps = {
  opened: boolean;
  onClose: () => void;
  addresses: UserAddressResponseDto[];
  selectedAddressId: number | null;
  onAddressSelect: (addressId: number) => void;
  onSetDefault: (addressId: number) => void;
  onEdit: (address: UserAddressResponseDto) => void;
  onDelete: (addressId: number) => void;
  onAdd: () => void;
};

const AddressSelectionModal = ({
  opened,
  onClose,
  addresses,
  selectedAddressId,
  onAddressSelect,
  onSetDefault,
  onEdit,
  onDelete,
  onAdd,
}: AddressSelectionModalProps) => {
  return (
    <Modal opened={opened} onClose={onClose} title="Shipping Address" size="lg">
      <Stack>
        <Button
          leftSection={<FiPlus size={16} />}
          variant="outline"
          onClick={onAdd}
          mb="sm"
        >
          Add New Address
        </Button>

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
            onClick={() => onAddressSelect(address.id)}
          >
            <Group align="flex-start" justify="space-between">
              <Group align="flex-start">
                <Radio
                  checked={selectedAddressId === address.id}
                  onChange={() => onAddressSelect(address.id)}
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
                  </Group>
                </Box>
              </Group>
              <Group>
                <ActionIcon
                  variant="subtle"
                  color="blue"
                  onClick={(e) => {
                    e.stopPropagation();
                    onEdit(address);
                  }}
                >
                  <FiEdit2 size={16} />
                </ActionIcon>
                {!address.isDefault && (
                  <ActionIcon
                    variant="subtle"
                    color="red"
                    onClick={(e) => {
                      e.stopPropagation();
                      onDelete(address.id);
                    }}
                  >
                    <FiTrash2 size={16} />
                  </ActionIcon>
                )}
                {!address.isDefault && (
                  <Button
                    variant="outline"
                    size="xs"
                    onClick={(e) => {
                      e.stopPropagation();
                      onSetDefault(address.id);
                    }}
                  >
                    Set as default
                  </Button>
                )}
              </Group>
            </Group>
          </Card>
        ))}
      </Stack>

      <Group justify="flex-end" mt="xl">
        <Button onClick={onClose}>Confirm Selection</Button>
      </Group>
    </Modal>
  );
};

// Modal for adding/editing addresses
type AddressFormModalProps = {
  opened: boolean;
  onClose: () => void;
  title: string;
  editingAddress?: UserAddressResponseDto;
  onSubmit: (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => void;
  isSubmitting: boolean;
};

const AddressFormModal = ({
  opened,
  onClose,
  title,
  editingAddress,
  onSubmit,
  isSubmitting,
}: AddressFormModalProps) => {
  return (
    <Modal opened={opened} onClose={onClose} title={title} size="lg">
      <AddressForm
        initialValues={editingAddress}
        onSubmit={onSubmit}
        onCancel={onClose}
        isSubmitting={isSubmitting}
      />
    </Modal>
  );
};

// ShippingAddressSection component
type ShippingAddressSectionProps = {
  addresses: UserAddressResponseDto[];
  selectedAddress: UserAddressResponseDto | undefined;
  onOpenAddressModal: () => void;
  addressModalOpened: boolean;
  onAddressModalClose: () => void;
  selectedAddressId: number | null;
  onAddressSelect: (addressId: number) => void;
  onSetDefault: (addressId: number) => void;
  onEdit: (address: UserAddressResponseDto) => void;
  onDelete: (addressId: number) => void;
  onAdd: () => void;
};

const ShippingAddressSection = ({
  addresses,
  selectedAddress,
  onOpenAddressModal,
  addressModalOpened,
  onAddressModalClose,
  selectedAddressId,
  onAddressSelect,
  onSetDefault,
  onEdit,
  onDelete,
  onAdd,
}: ShippingAddressSectionProps) => {
  return (
    <Paper shadow="xs" p="md" withBorder>
      <Group justify="space-between" mb="md">
        <Title order={4}>Shipping Address</Title>
        <Button
          size="xs"
          variant="outline"
          leftSection={<FiEdit2 size={16} />}
          onClick={onOpenAddressModal}
        >
          Change
        </Button>
      </Group>

      {addresses?.length === 0 ? (
        <Alert color="yellow">
          You don't have any saved addresses. Please add an address to continue.
        </Alert>
      ) : selectedAddress ? (
        <ShippingAddressCard
          address={selectedAddress}
          isDefault={selectedAddress.isDefault}
        />
      ) : (
        <Alert color="yellow">
          Please select a shipping address to continue.
        </Alert>
      )}

      <AddressSelectionModal
        opened={addressModalOpened}
        onClose={onAddressModalClose}
        addresses={addresses || []}
        selectedAddressId={selectedAddressId}
        onAddressSelect={onAddressSelect}
        onSetDefault={onSetDefault}
        onEdit={onEdit}
        onDelete={onDelete}
        onAdd={onAdd}
      />
    </Paper>
  );
};

// OrderItemCard component
type OrderItemCardProps = {
  item: CartItemResponseDto;
  baseImageUrl: string;
};

const OrderItemCard = ({ item, baseImageUrl }: OrderItemCardProps) => {
  return (
    <Group wrap="nowrap" align="flex-start" gap="md">
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
        <Text size="sm" fw={500} style={{ color: 'red' }} mt={4}>
          {formatPrice(item.subtotal)}
        </Text>
      </Box>
    </Group>
  );
};

// Define the shop group type
type ShopItemGroup = {
  shopId: string;
  shopName: string;
  shopImageUrl: string | null;
  items: CartItemResponseDto[];
};

// ShopItemsGroup component
type ShopItemsGroupProps = {
  group: ShopItemGroup;
  baseImageUrl: string;
};

const ShopItemsGroup = ({ group, baseImageUrl }: ShopItemsGroupProps) => {
  return (
    <Paper shadow="xs" p="md" withBorder key={group.shopId}>
      {/* Shop header */}
      <Group mb="sm">
        <Button
          variant="subtle"
          px="xs"
          component={Link}
          to={`/shop/${group.shopId}`}
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
          <OrderItemCard
            key={item.productId}
            item={item}
            baseImageUrl={baseImageUrl}
          />
        ))}
      </Stack>
    </Paper>
  );
};

// OrderItemsSection component
type OrderItemsSectionProps = {
  selectedItemsCount: number;
  groupedSelectedItems: ShopItemGroup[];
  baseImageUrl: string;
};

const OrderItemsSection = ({
  selectedItemsCount,
  groupedSelectedItems,
  baseImageUrl,
}: OrderItemsSectionProps) => {
  return (
    <Paper shadow="xs" p="md" withBorder>
      <Title order={4} mb="md">
        Order Items ({selectedItemsCount} items)
      </Title>

      <Stack gap="md">
        {groupedSelectedItems.map((group) => (
          <ShopItemsGroup
            key={group.shopId}
            group={group}
            baseImageUrl={baseImageUrl}
          />
        ))}
      </Stack>
    </Paper>
  );
};

// CouponSelectorModal component
type CouponSelectorModalProps = {
  opened: boolean;
  onClose: () => void;
  title: string;
  coupons: CouponResponseDto[];
  selectedCoupon: string | undefined;
  onCouponSelect: (code: string | undefined) => void;
  iconComponent: React.ReactNode;
  badgeColor: string;
};

const CouponSelectorModal = ({
  opened,
  onClose,
  title,
  coupons,
  selectedCoupon,
  onCouponSelect,
  iconComponent,
  badgeColor,
}: CouponSelectorModalProps) => {
  return (
    <Modal opened={opened} onClose={onClose} title={title} size="lg">
      {coupons.length > 0 ? (
        <Stack>
          <Button
            variant="subtle"
            color="red"
            onClick={() => onCouponSelect(undefined)}
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
              {coupons.map((coupon) => (
                <Table.Tr key={coupon.code}>
                  <Table.Td>
                    <Group>
                      {iconComponent}
                      <Text fw={500}>{coupon.code}</Text>
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={badgeColor}>
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
                      onClick={() => onCouponSelect(coupon.code)}
                      variant={
                        selectedCoupon === coupon.code ? 'filled' : 'outline'
                      }
                    >
                      {selectedCoupon === coupon.code ? 'Selected' : 'Select'}
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
            No {title.toLowerCase()} available at the moment.
          </Alert>
        </Center>
      )}
      <Group justify="flex-end" mt="xl">
        <Button onClick={onClose}>Close</Button>
      </Group>
    </Modal>
  );
};

// CouponSection component
type CouponSectionProps = {
  selectedProductCoupon: string | undefined;
  selectedShippingCoupon: string | undefined;
  onOpenProductCouponModal: () => void;
  onOpenShippingCouponModal: () => void;
  productCouponModalOpened: boolean;
  shippingCouponModalOpened: boolean;
  onProductCouponModalClose: () => void;
  onShippingCouponModalClose: () => void;
  productCoupons: CouponResponseDto[];
  shippingCoupons: CouponResponseDto[];
  onProductCouponSelect: (code: string | undefined) => void;
  onShippingCouponSelect: (code: string | undefined) => void;
};

const CouponSection = ({
  selectedProductCoupon,
  selectedShippingCoupon,
  onOpenProductCouponModal,
  onOpenShippingCouponModal,
  productCouponModalOpened,
  shippingCouponModalOpened,
  onProductCouponModalClose,
  onShippingCouponModalClose,
  productCoupons,
  shippingCoupons,
  onProductCouponSelect,
  onShippingCouponSelect,
}: CouponSectionProps) => {
  return (
    <Paper shadow="xs" p="md" withBorder>
      <Title order={4} mb="md">
        Coupons
      </Title>

      <Group justify="space-between">
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
          <Button variant="outline" onClick={onOpenProductCouponModal}>
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
          <Button variant="outline" onClick={onOpenShippingCouponModal}>
            {selectedShippingCoupon ? 'Change' : 'Select Coupon'}
          </Button>
        </Group>
      </Group>

      {/* Product Coupon Modal */}
      <CouponSelectorModal
        opened={productCouponModalOpened}
        onClose={onProductCouponModalClose}
        title="Select Product Coupon"
        coupons={productCoupons}
        selectedCoupon={selectedProductCoupon}
        onCouponSelect={onProductCouponSelect}
        iconComponent={<FiTag size={16} />}
        badgeColor="green"
      />

      {/* Shipping Coupon Modal */}
      <CouponSelectorModal
        opened={shippingCouponModalOpened}
        onClose={onShippingCouponModalClose}
        title="Select Shipping Coupon"
        coupons={shippingCoupons}
        selectedCoupon={selectedShippingCoupon}
        onCouponSelect={onShippingCouponSelect}
        iconComponent={<FiTruck size={16} />}
        badgeColor="blue"
      />
    </Paper>
  );
};

// PaymentMethodSection component
type PaymentMethodSectionProps = {
  paymentMethod: PaymentMethod;
  setPaymentMethod: (method: PaymentMethod) => void;
};

const PaymentMethodSection = ({
  paymentMethod,
  setPaymentMethod,
}: PaymentMethodSectionProps) => {
  return (
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
  );
};

// OrderSummarySection component
type OrderSummarySectionProps = {
  selectedItemsCount: number;
  selectedItemsTotal: number;
  checkoutPreview: CheckoutPricePreviewResponseDto | null | undefined;
  fetchingCheckoutPreview: boolean;
  checkoutIsPending: boolean;
  selectedAddress: UserAddressResponseDto | undefined;
  onCheckout: () => void;
};

const OrderSummarySection = ({
  selectedItemsCount,
  selectedItemsTotal,
  checkoutPreview,
  fetchingCheckoutPreview,
  checkoutIsPending,
  selectedAddress,
  onCheckout,
}: OrderSummarySectionProps) => {
  return (
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
            !selectedAddress || fetchingCheckoutPreview || checkoutIsPending
          }
          onClick={onCheckout}
          leftSection={<FiCheckCircle />}
          loading={fetchingCheckoutPreview}
        >
          {checkoutIsPending
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
  );
};

// Main CheckoutPage component
function CheckoutPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { cartItems, isLoadingCart } = useCart();
  const {
    addresses,
    loadingAddresses,
    setDefaultAddress,
    addAddress,
    editAddress,
    deleteAddress,
  } = useAddresses();
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

  // State for address form modal
  const [addressFormModalOpened, setAddressFormModalOpened] = useState(false);
  const [editingAddress, setEditingAddress] = useState<
    UserAddressResponseDto | undefined
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

  // Handle edit address
  const handleEditAddress = (address: UserAddressResponseDto) => {
    setEditingAddress(address);
    setAddressFormModalOpened(true);
  };

  // Handle delete address
  const handleDeleteAddress = (addressId: number) => {
    if (window.confirm('Are you sure you want to delete this address?')) {
      deleteAddress.mutate(addressId);
    }
  };

  // Handle add new address
  const handleAddAddress = () => {
    setEditingAddress(undefined);
    setAddressFormModalOpened(true);
  };

  // Handle form submission for adding/editing address
  const handleAddressFormSubmit = (
    values: AddUserAddressRequestDto | EditUserAddressRequestDto
  ) => {
    if (editingAddress) {
      // Edit existing address
      editAddress.mutate(
        {
          id: editingAddress.id,
          addressData: values as EditUserAddressRequestDto,
        },
        {
          onSuccess: () => {
            setAddressFormModalOpened(false);
            setAddressModalOpened(true);
          },
        }
      );
    } else {
      // Add new address
      addAddress.mutate(values as AddUserAddressRequestDto, {
        onSuccess: (newAddress) => {
          setAddressFormModalOpened(false);
          setSelectedAddressId(newAddress.id);
          setAddressModalOpened(true);
        },
      });
    }
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

  const groupedSelectedItems: ShopItemGroup[] =
    selectedItems.length > 0
      ? Object.values(
          selectedItems.reduce<Record<string, ShopItemGroup>>((acc, item) => {
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
        <ShippingAddressSection
          addresses={addresses || []}
          selectedAddress={selectedAddress}
          onOpenAddressModal={() => setAddressModalOpened(true)}
          addressModalOpened={addressModalOpened}
          onAddressModalClose={() => setAddressModalOpened(false)}
          selectedAddressId={selectedAddressId}
          onAddressSelect={handleAddressSelect}
          onSetDefault={handleSetDefault}
          onEdit={handleEditAddress}
          onDelete={handleDeleteAddress}
          onAdd={handleAddAddress}
        />

        {/* Address Form Modal */}
        <AddressFormModal
          opened={addressFormModalOpened}
          onClose={() => setAddressFormModalOpened(false)}
          title={editingAddress ? 'Edit Address' : 'Add New Address'}
          editingAddress={editingAddress}
          onSubmit={handleAddressFormSubmit}
          isSubmitting={addAddress.isPending || editAddress.isPending}
        />

        {/* Order Items Section */}
        <OrderItemsSection
          selectedItemsCount={selectedItemsCount}
          groupedSelectedItems={groupedSelectedItems}
          baseImageUrl={baseImageUrl}
        />

        {/* Coupons Section */}
        <CouponSection
          selectedProductCoupon={selectedProductCoupon}
          selectedShippingCoupon={selectedShippingCoupon}
          onOpenProductCouponModal={() => setProductCouponModalOpened(true)}
          onOpenShippingCouponModal={() => setShippingCouponModalOpened(true)}
          productCouponModalOpened={productCouponModalOpened}
          shippingCouponModalOpened={shippingCouponModalOpened}
          onProductCouponModalClose={() => setProductCouponModalOpened(false)}
          onShippingCouponModalClose={() => setShippingCouponModalOpened(false)}
          productCoupons={productCoupons}
          shippingCoupons={shippingCoupons}
          onProductCouponSelect={handleProductCouponSelect}
          onShippingCouponSelect={handleShippingCouponSelect}
        />

        {/* Payment Method Section */}
        <PaymentMethodSection
          paymentMethod={paymentMethod}
          setPaymentMethod={setPaymentMethod}
        />

        {/* Order Summary */}
        <OrderSummarySection
          selectedItemsCount={selectedItemsCount}
          selectedItemsTotal={selectedItemsTotal}
          checkoutPreview={checkoutPreview}
          fetchingCheckoutPreview={fetchingCheckoutPreview}
          checkoutIsPending={checkout.isPending}
          selectedAddress={selectedAddress}
          onCheckout={handleCheckout}
        />
      </Stack>
    </Container>
  );
}

export default CheckoutPage;

import { useEffect, useState } from 'react';
import { useCart } from '../../lib/hooks/useCart';
import { useAddresses } from '../../lib/hooks/useAddresses';
import { useOrders } from '../../lib/hooks/useOrders';
import { useCoupons } from '../../lib/hooks/useCoupons';
import { useNavigate, useSearchParams } from 'react-router';
import {
  Container,
  Title,
  Button,
  Stack,
  LoadingOverlay,
  Alert,
} from '@mantine/core';
import {
  CheckoutPricePreviewRequestDto,
  CheckoutRequestDto,
  PaymentMethod,
  UserAddressResponseDto,
  AddUserAddressRequestDto,
  EditUserAddressRequestDto,
} from '../../lib/types';
import { Link } from 'react-router';
import { FiAlertCircle, FiArrowLeft } from 'react-icons/fi';
import { AddressFormModal } from './AddressFormModal';
import { ShippingAddressSection } from './ShippingAddressSection';
import { OrderItemsSection } from './OrderItemsSection';
import { CouponSection } from './CouponSection';
import { PaymentMethodSection } from './PaymentMethodSection';
import { ShopItemGroup } from './ShopItemGroup';
import { OrderSummarySection } from './OrderSummarySection';

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

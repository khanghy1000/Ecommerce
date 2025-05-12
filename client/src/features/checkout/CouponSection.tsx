import { Paper, Title, Group, Box, Text, Button } from '@mantine/core';
import { FiTag, FiTruck } from 'react-icons/fi';
import { CouponResponseDto } from '../../lib/types';
import { CouponSelectorModal } from './CouponSelectorModal';

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
export const CouponSection = ({
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

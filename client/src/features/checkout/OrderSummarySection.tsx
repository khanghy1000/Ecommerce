import {
  Paper,
  Title,
  Stack,
  Group,
  Loader,
  Badge,
  Divider,
  Flex,
  Button,
  Text,
} from '@mantine/core';
import { FiCheckCircle, FiArrowLeft } from 'react-icons/fi';
import { Link } from 'react-router';
import {
  CheckoutPricePreviewResponseDto,
  UserAddressResponseDto,
} from '../../lib/types';
import { formatPrice } from '../../lib/utils';

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

export const OrderSummarySection = ({
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
          className={
            'checkout-button ' +
            (!fetchingCheckoutPreview && !checkoutIsPending
              ? 'ready-to-checkout'
              : '')
          }
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

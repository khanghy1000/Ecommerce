import { Group, Box, Image, Text } from "@mantine/core";
import { Link } from "react-router";
import { CartItemResponseDto } from "../../lib/types";
import { formatPrice } from "../../lib/utils";

// OrderItemCard component
type OrderItemCardProps = {
  item: CartItemResponseDto;
  baseImageUrl: string;
};

export const OrderItemCard = ({ item, baseImageUrl }: OrderItemCardProps) => {
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
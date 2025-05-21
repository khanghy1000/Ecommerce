import {
  Avatar,
  Button,
  Divider,
  Group,
  Paper,
  rem,
  Stack,
  Text,
} from '@mantine/core';
import { CartItemResponseDto } from '../../lib/types';
import { Link } from 'react-router';
import { OrderItemCard } from './OrderItemCard';

// Define the shop group type
export type ShopItemGroup = {
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

export const ShopItemsGroup = ({
  group,
  baseImageUrl,
}: ShopItemsGroupProps) => {
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

import { Paper, Title, Stack } from '@mantine/core';
import { ShopItemGroup, ShopItemsGroup } from './ShopItemGroup';

// OrderItemsSection component
type OrderItemsSectionProps = {
  selectedItemsCount: number;
  groupedSelectedItems: ShopItemGroup[];
  baseImageUrl: string;
};
export const OrderItemsSection = ({
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

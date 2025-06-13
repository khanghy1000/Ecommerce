import {
  Box,
  Group,
  Text,
  Rating,
  Stack,
  Progress,
  Paper,
  Skeleton,
} from '@mantine/core';
import { useReviews } from '../../lib/hooks/useReviews';

interface ReviewSummaryProps {
  productId: number;
}

function ReviewSummary({ productId }: ReviewSummaryProps) {
  const { reviewSummary, loadingReviewSummary } = useReviews(
    undefined,
    undefined,
    productId
  );

  if (loadingReviewSummary) {
    return (
      <Paper p="md" withBorder mb="lg">
        <Group gap="xl">
          <Box style={{ textAlign: 'center' }}>
            <Skeleton height={20} width={80} mb="xs" />
            <Skeleton height={24} width={120} mb="xs" />
            <Skeleton height={16} width={100} />
          </Box>
          <Box style={{ flex: 1 }}>
            <Skeleton height={16} width="100%" mb="xs" />
            <Skeleton height={16} width="100%" mb="xs" />
            <Skeleton height={16} width="100%" mb="xs" />
            <Skeleton height={16} width="100%" mb="xs" />
            <Skeleton height={16} width="100%" />
          </Box>
        </Group>
      </Paper>
    );
  }

  if (!reviewSummary || reviewSummary.totalReviews === 0) {
    return (
      <Paper p="md" withBorder mb="lg">
        <Text ta="center" c="dimmed">
          No reviews yet for this product
        </Text>
      </Paper>
    );
  }

  return (
    <Paper p="md" withBorder mb="lg">
      <Group gap="xl" align="flex-start">
        {/* Average rating display */}
        <Box style={{ textAlign: 'center' }}>
          <Text size="xl" fw={700} mb="xs">
            {reviewSummary.averageRating.toFixed(1)}
          </Text>
          <Rating
            value={reviewSummary.averageRating}
            readOnly
            size="lg"
            mb="xs"
          />
          <Text size="sm" c="dimmed">
            {reviewSummary.totalReviews} review
            {reviewSummary.totalReviews !== 1 ? 's' : ''}
          </Text>
        </Box>

        {/* Rating distribution */}
        <Box style={{ flex: 1 }}>
          <Stack gap="xs">
            {[5, 4, 3, 2, 1].map((star) => {
              const count = reviewSummary.ratingDistribution[star] || 0;
              const percentage =
                reviewSummary.totalReviews > 0
                  ? (count / reviewSummary.totalReviews) * 100
                  : 0;

              return (
                <Group key={star} gap="sm" align="center">
                  <Text size="xs" style={{ minWidth: 20 }}>
                    {star}
                  </Text>
                  <Rating value={1} count={1} readOnly size="xs" />
                  <Progress
                    value={percentage}
                    size="sm"
                    style={{ flex: 1 }}
                    color="blue"
                  />
                  <Text size="xs" style={{ minWidth: 30 }} ta="right">
                    {count}
                  </Text>
                </Group>
              );
            })}
          </Stack>
        </Box>
      </Group>
    </Paper>
  );
}

export default ReviewSummary;

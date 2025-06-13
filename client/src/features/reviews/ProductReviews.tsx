import { Box, Group, Button, Modal, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useAccount } from '../../lib/hooks/useAccount';
import { useReviews } from '../../lib/hooks/useReviews';
import ReviewForm from './ReviewForm';
import ReviewsList from './ReviewsList';
import ReviewSummary from './ReviewSummary';
import { FiStar } from 'react-icons/fi';

interface ProductReviewsProps {
  productId: number;
  productName: string;
}

function ProductReviews({ productId, productName }: ProductReviewsProps) {
  const [
    reviewModalOpened,
    { open: openReviewModal, close: closeReviewModal },
  ] = useDisclosure(false);
  const { currentUserInfo } = useAccount();

  // Check if current user has already reviewed this product
  const { reviews: userReviews, hasPurchased } = useReviews(undefined, {
    productId,
    userId: currentUserInfo?.id,
    pageSize: 1,
    pageNumber: 1,
  }, productId);

  const userHasReviewed = userReviews && userReviews.items.length > 0;
  const userReview = userHasReviewed ? userReviews.items[0] : undefined;

  const canReview = currentUserInfo?.role === 'Buyer' && !userHasReviewed && hasPurchased;

  const handleReviewSuccess = () => {
    closeReviewModal();
  };

  return (
    <Box>
      <Group justify="space-between" align="center" mb="lg">
        <Text size="xl" fw={600}>
          Reviews
        </Text>

        {currentUserInfo && (
          <>
            {canReview && (
              <Button
                leftSection={<FiStar />}
                onClick={openReviewModal}
                variant="outline"
              >
                Write a Review
              </Button>
            )}

            {userHasReviewed && (
              <Button
                leftSection={<FiStar />}
                onClick={openReviewModal}
                variant="outline"
              >
                Edit Your Review
              </Button>
            )}
          </>
        )}

        {!currentUserInfo && (
          <Text size="sm" c="dimmed">
            Please log in to write a review
          </Text>
        )}
      </Group>

      <ReviewSummary productId={productId} />
      <ReviewsList productId={productId} productName={productName} />

      {/* Review Modal */}
      <Modal
        opened={reviewModalOpened}
        onClose={closeReviewModal}
        title={userHasReviewed ? 'Edit Your Review' : 'Write a Review'}
        size="md"
      >
        <ReviewForm
          productId={productId}
          productName={productName}
          existingReview={userReview}
          onSuccess={handleReviewSuccess}
        />
      </Modal>
    </Box>
  );
}

export default ProductReviews;

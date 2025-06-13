import { useState } from 'react';
import {
  Box,
  Text,
  Rating,
  Group,
  Stack,
  Paper,
  Button,
  Modal,
  Pagination,
  Title,
  Skeleton,
  Avatar,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { useReviews } from '../../lib/hooks/useReviews';
import { useAccount } from '../../lib/hooks/useAccount';
import { ReviewResponseDto } from '../../lib/types';
import ReviewForm from './ReviewForm';
import { FiEdit2, FiTrash2, FiCheckCircle } from 'react-icons/fi';
import { format, parseISO } from 'date-fns';

interface ReviewsListProps {
  productId: number;
  productName: string;
}

function ReviewsList({ productId, productName }: ReviewsListProps) {
  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

  const [page, setPage] = useState(1);
  const [editingReview, setEditingReview] = useState<ReviewResponseDto | null>(
    null
  );
  const [deletingReviewId, setDeletingReviewId] = useState<number | null>(null);

  const [editModalOpened, { open: openEditModal, close: closeEditModal }] =
    useDisclosure(false);
  const [
    deleteModalOpened,
    { open: openDeleteModal, close: closeDeleteModal },
  ] = useDisclosure(false);

  const { currentUserInfo } = useAccount();
  const { reviews, loadingReviews, deleteReview } = useReviews(undefined, {
    productId,
    pageSize: 10,
    pageNumber: page,
  });

  const handleEditReview = (review: ReviewResponseDto) => {
    setEditingReview(review);
    openEditModal();
  };

  const handleDeleteReview = (reviewId: number) => {
    setDeletingReviewId(reviewId);
    openDeleteModal();
  };

  const confirmDeleteReview = async () => {
    if (!deletingReviewId) return;

    deleteReview.mutate(
      { id: deletingReviewId, productId },
      {
        onSuccess: () => {
          notifications.show({
            title: 'Review deleted',
            message: 'Your review has been deleted successfully',
            color: 'green',
            icon: <FiCheckCircle />,
          });
          closeDeleteModal();
          setDeletingReviewId(null);
        },
        onError: () => {
          notifications.show({
            title: 'Error',
            message: 'Failed to delete review. Please try again.',
            color: 'red',
            icon: <FiCheckCircle />,
          });
        },
      }
    );
  };

  const handleEditSuccess = () => {
    closeEditModal();
    setEditingReview(null);
  };

  if (loadingReviews) {
    return (
      <Box>
        <Title order={3} mb="md">
          Customer Reviews
        </Title>
        <Stack gap="md">
          {[...Array(3)].map((_, index) => (
            <Paper key={index} p="md" withBorder>
              <Group align="flex-start" gap="md">
                <Skeleton height={40} width={40} radius="xl" />
                <Box style={{ flex: 1 }}>
                  <Skeleton height={20} width="30%" mb="xs" />
                  <Skeleton height={16} width="20%" mb="sm" />
                  <Skeleton height={60} />
                </Box>
              </Group>
            </Paper>
          ))}
        </Stack>
      </Box>
    );
  }

  if (!reviews || reviews.items.length === 0) {
    return (
      <Box>
        <Title order={3} mb="md">
          Customer Reviews
        </Title>
        <Paper p="xl" withBorder style={{ textAlign: 'center' }}>
          <Text c="dimmed">
            No reviews yet. Be the first to review this product!
          </Text>
        </Paper>
      </Box>
    );
  }

  const totalPages = Math.ceil(reviews.totalCount / reviews.pageSize);

  return (
    <Box>
      <Title order={3} mb="md">
        Customer Reviews ({reviews.totalCount})
      </Title>

      <Stack gap="md">
        {reviews.items.map((review) => (
          <Paper key={review.id} p="md" withBorder>
            <Group align="flex-start" gap="md">
              <Avatar
                size="md"
                radius="xl"
                src={baseImageUrl + review.userImageUrl}
              >
                {review.userId.charAt(0).toUpperCase()}
              </Avatar>

              <Box style={{ flex: 1 }}>
                <Group justify="space-between" align="flex-start" mb="xs">
                  <Box>
                    <Text size="sm" fw={500}>
                      {review.userDisplayName}
                    </Text>
                    <Group gap="xs" align="center">
                      <Rating value={review.rating} readOnly size="sm" />
                      <Text size="xs" c="dimmed">
                        {format(review.updatedAt, 'PPpp')}
                        {parseISO(review.updatedAt) >
                          parseISO(review.createdAt) && ' (edited)'}
                      </Text>
                    </Group>
                  </Box>

                  {currentUserInfo?.id === review.userId && (
                    <Group gap="xs">
                      <Button
                        size="xs"
                        variant="subtle"
                        leftSection={<FiEdit2 size={12} />}
                        onClick={() => handleEditReview(review)}
                      >
                        Edit
                      </Button>
                      <Button
                        size="xs"
                        variant="subtle"
                        color="red"
                        leftSection={<FiTrash2 size={12} />}
                        onClick={() => handleDeleteReview(review.id)}
                      >
                        Delete
                      </Button>
                    </Group>
                  )}
                </Group>

                {review.review && (
                  <Text size="sm" mt="xs">
                    {review.review}
                  </Text>
                )}
              </Box>
            </Group>
          </Paper>
        ))}
      </Stack>

      {totalPages > 1 && (
        <Group justify="center" mt="xl">
          <Pagination total={totalPages} value={page} onChange={setPage} />
        </Group>
      )}

      {/* Edit Review Modal */}
      <Modal
        opened={editModalOpened}
        onClose={closeEditModal}
        title="Edit Review"
        size="md"
      >
        {editingReview && (
          <ReviewForm
            productId={productId}
            productName={productName}
            existingReview={editingReview}
            onSuccess={handleEditSuccess}
          />
        )}
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        opened={deleteModalOpened}
        onClose={closeDeleteModal}
        title="Delete Review"
        centered
      >
        <Text>
          Are you sure you want to delete this review? This action cannot be
          undone.
        </Text>
        <Group justify="flex-end" mt="md">
          <Button variant="default" onClick={closeDeleteModal}>
            Cancel
          </Button>
          <Button
            color="red"
            loading={deleteReview.isPending}
            onClick={confirmDeleteReview}
          >
            Delete
          </Button>
        </Group>
      </Modal>
    </Box>
  );
}

export default ReviewsList;

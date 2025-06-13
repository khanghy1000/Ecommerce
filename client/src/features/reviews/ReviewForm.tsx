import { useState } from 'react';
import {
  Button,
  Group,
  Rating,
  Textarea,
  Text,
  Box,
  Title,
} from '@mantine/core';
import { useForm, zodResolver } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { z } from 'zod';
import { useReviews } from '../../lib/hooks/useReviews';
import { ReviewResponseDto } from '../../lib/types';
import { FiCheckCircle } from 'react-icons/fi';

const reviewSchema = z.object({
  rating: z.number().min(1, 'Please select a rating').max(5),
  review: z
    .string()
    .max(1000, 'Review cannot exceed 1000 characters')
    .optional(),
});

interface ReviewFormProps {
  productId: number;
  productName: string;
  existingReview?: ReviewResponseDto;
  onSuccess?: () => void;
}

function ReviewForm({
  productId,
  productName,
  existingReview,
  onSuccess,
}: ReviewFormProps) {
  const { createReview, updateReview } = useReviews();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm({
    initialValues: {
      rating: existingReview?.rating || 0,
      review: existingReview?.review || '',
    },
    validate: zodResolver(reviewSchema),
  });

  const handleSubmit = async (values: { rating: number; review: string }) => {
    setIsSubmitting(true);

    if (existingReview) {
      // Update existing review
      await updateReview.mutateAsync(
        {
          id: existingReview.id,
          reviewData: {
            rating: values.rating,
            review: values.review || undefined,
          },
        },
        {
          onSuccess: () => {
            notifications.show({
              title: 'Review updated',
              message: 'Your review has been updated successfully',
              color: 'green',
              icon: <FiCheckCircle />,
            });
            onSuccess?.();
          },
          onSettled: () => {
            setIsSubmitting(false);
          },
        }
      );
    } else {
      // Create new review
      await createReview.mutateAsync(
        {
          productId,
          rating: values.rating,
          review: values.review || undefined,
        },
        {
          onSuccess: () => {
            notifications.show({
              title: 'Review submitted',
              message: 'Thank you for your review!',
              color: 'green',
              icon: <FiCheckCircle />,
            });

            form.reset();
            onSuccess?.();
          },
          onSettled: () => {
            setIsSubmitting(false);
          },
        }
      );
    }
  };

  return (
    <Box>
      <Title order={4} mb="md">
        {existingReview ? 'Edit Your Review' : 'Write a Review'}
      </Title>

      <Text size="sm" c="dimmed" mb="md">
        Share your experience with {productName}
      </Text>

      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Box mb="md">
          <Text size="sm" mb="xs">
            Rating *
          </Text>
          <Rating
            size="lg"
            {...form.getInputProps('rating')}
            onChange={(value) => form.setFieldValue('rating', value)}
          />
          {form.errors.rating && (
            <Text size="xs" c="red" mt="xs">
              {form.errors.rating}
            </Text>
          )}
        </Box>

        <Textarea
          label="Review (optional)"
          placeholder="Tell others about your experience with this product..."
          minRows={4}
          maxRows={8}
          {...form.getInputProps('review')}
          mb="md"
        />

        <Group justify="flex-end">
          <Button
            type="submit"
            loading={isSubmitting}
            disabled={form.values.rating === 0}
          >
            {existingReview ? 'Update Review' : 'Submit Review'}
          </Button>
        </Group>
      </form>
    </Box>
  );
}

export default ReviewForm;

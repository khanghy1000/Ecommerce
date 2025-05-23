import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import queryString from 'query-string';
import {
  CreateReviewRequestDto,
  ListReviewsRequest,
  PagedList,
  ReviewResponseDto,
  UpdateReviewRequestDto,
} from '../types';

export const useReviews = (
  reviewId?: number,
  listReviewsRequest?: ListReviewsRequest
) => {
  const queryClient = useQueryClient();

  // Get a single review by ID
  const { data: review, isLoading: loadingReview } = useQuery({
    queryKey: ['review', reviewId],
    queryFn: async () => {
      if (!reviewId) return null;
      return await customFetch<ReviewResponseDto>(`/reviews/${reviewId}`);
    },
    enabled: !!reviewId,
  });

  // Get a list of reviews with optional filtering
  const { data: reviews, isLoading: loadingReviews } = useQuery({
    queryKey: ['reviews', listReviewsRequest],
    queryFn: async () => {
      let url = '/reviews';

      if (listReviewsRequest) {
        const stringifiedParams = queryString.stringify(listReviewsRequest, {
          arrayFormat: 'none',
        });
        url = `${url}?${stringifiedParams}`;
      }

      return await customFetch<PagedList<ReviewResponseDto>>(url);
    },
    enabled: !reviewId,
  });

  // Create a new review
  const createReview = useMutation({
    mutationFn: async (reviewData: CreateReviewRequestDto) => {
      return await customFetch<ReviewResponseDto>('/reviews', {
        method: 'POST',
        body: JSON.stringify(reviewData),
      });
    },
    onSuccess: (data) => {
      // Invalidate relevant queries to refetch the updated data
      queryClient.invalidateQueries({
        queryKey: ['reviews'],
      });
      queryClient.invalidateQueries({
        queryKey: ['review', data.id],
      });
      // If this is a review for a product, invalidate the product queries too
      if (data.productId) {
        queryClient.invalidateQueries({
          queryKey: ['product', data.productId],
        });
      }
    },
  });

  // Update an existing review
  const updateReview = useMutation({
    mutationFn: async ({
      id,
      reviewData,
    }: {
      id: number;
      reviewData: UpdateReviewRequestDto;
    }) => {
      return await customFetch<ReviewResponseDto>(`/reviews/${id}`, {
        method: 'PUT',
        body: JSON.stringify(reviewData),
      });
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['reviews'],
      });
      queryClient.invalidateQueries({
        queryKey: ['review', data.id],
      });
      if (data.productId) {
        queryClient.invalidateQueries({
          queryKey: ['product', data.productId],
        });
      }
    },
  });

  // Delete a review
  const deleteReview = useMutation({
    mutationFn: async (id: number) => {
      await customFetch(`/reviews/${id}`, {
        method: 'DELETE',
      });
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['reviews'],
      });
      // Invalidate the individual review query
      queryClient.removeQueries({
        queryKey: ['review', variables],
      });
      // We don't have the product ID here to invalidate the product query
      // but generally this would be handled in the component using this hook
    },
  });

  return {
    // Single review
    review,
    loadingReview,

    // List of reviews
    reviews,
    loadingReviews,

    // Mutations
    createReview,
    updateReview,
    deleteReview,
  };
};

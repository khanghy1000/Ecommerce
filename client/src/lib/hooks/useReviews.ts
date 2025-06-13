import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { customFetch } from '../customFetch';
import queryString from 'query-string';
import {
  CreateReviewRequestDto,
  HasUserPurchasedProductDto,
  ListReviewsRequest,
  PagedList,
  ProductReviewSummaryDto,
  ReviewResponseDto,
  UpdateReviewRequestDto,
} from '../types';

export const useReviews = (
  reviewId?: number,
  listReviewsRequest?: ListReviewsRequest,
  productId?: number
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

  // Get a list of reviews
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
    enabled: !reviewId && !!listReviewsRequest,
  });

  const { data: hasPurchased, isLoading: loadingHasPurchased } = useQuery({
    queryKey: ['hasPurchased', productId],
    queryFn: async () => {
      if (!productId) return false;
      return (
        await customFetch<HasUserPurchasedProductDto>(
          `/reviews/has-purchased/${productId}`
        )
      ).hasPurchased;
    },
    enabled: !!productId,
  });

  // Get review summary for a product
  const { data: reviewSummary, isLoading: loadingReviewSummary } = useQuery({
    queryKey: ['reviewSummary', productId],
    queryFn: async () => {
      if (!productId) return null;
      return await customFetch<ProductReviewSummaryDto>(
        `/reviews/summary/${productId}`
      );
    },
    enabled: !!productId,
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
      queryClient.invalidateQueries({
        queryKey: ['reviewSummary', data.productId],
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
      queryClient.invalidateQueries({
        queryKey: ['reviewSummary', data.productId],
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
    mutationFn: async ({
      id,
      productId,
    }: {
      id: number;
      productId?: number;
    }) => {
      await customFetch(`/reviews/${id}`, {
        method: 'DELETE',
      });
      return { id, productId };
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ['reviews'],
      });
      // Invalidate the individual review query
      queryClient.removeQueries({
        queryKey: ['review', data.id],
      });
      // Invalidate review summary if productId is available
      if (data.productId) {
        queryClient.invalidateQueries({
          queryKey: ['reviewSummary', data.productId],
        });
        queryClient.invalidateQueries({
          queryKey: ['product', data.productId],
        });
      }
    },
  });

  return {
    // Single review
    review,
    loadingReview,

    // List of reviews
    reviews,
    loadingReviews,

    // Review summary
    reviewSummary,
    loadingReviewSummary,

    // Check if user has purchased the product
    hasPurchased,
    loadingHasPurchased,

    // Mutations
    createReview,
    updateReview,
    deleteReview,
  };
};

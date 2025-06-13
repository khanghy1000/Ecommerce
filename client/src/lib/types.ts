export type ErrorResponse = {
  title?: string;
  status?: number; // http status code
  detail?: string; // error message (include command handler error)
  message?: string; // exception message
  details?: string; // exception stack trace
  type?: string;
  // validation errors
  errors?: {
    [key: string]: string[];
  };
};

export type UserInfoResponse = {
  displayName: string;
  email: string;
  id: string;
  imageUrl: string | null;
  phoneNumber: string | null;
  role: 'Buyer' | 'Shop' | 'Admin';
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = {
  displayName: string;
  email: string;
  password: string;
  phoneNumber: string;
  address: string;
  wardId: number;
  role: string;
};

export type ChangePasswordRequest = {
  currentPassword: string;
  newPassword: string;
};

export type Province = {
  id: number;
  name: string;
};

export type District = {
  id: number;
  name: string;
  provinceId: number;
  provinceName: string;
};

export type Ward = {
  id: number;
  name: string;
  districtId: number;
  districtName: string;
};

export type ListProductsRequest = {
  keyword?: string;
  pageSize?: number;
  pageNumber?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  categoryId?: number;
  subCategoryIds?: number[];
  minPrice?: number;
  maxPrice?: number;
  includeInactive?: boolean;
  shopId?: string;
};

export type ProductResponseDto = {
  id: number;
  name: string;
  description: string;
  regularPrice: number;
  discountPrice: number | null;
  quantity: number;
  active: boolean;
  length: number;
  width: number;
  height: number;
  weight: number;
  shopId: string;
  shopName: string;
  shopImageUrl: string | null;
  subcategories: SubcategoryDto[];
  photos: ProductPhotoDto[];
};

export type ProductPhotoDto = {
  key: string;
  productId: number;
  displayOrder: number;
};

export type SubcategoryDto = {
  id: number;
  name: string;
  categoryId: number;
  categoryName: string;
};

export type PopularProductResponseDto = {
  categoryId: number;
  productId: number;
  salesCount: number;
  categoryName: string;
  productName: string;
  regularPrice: number;
  discountPrice: number | null;
  quantity: number;
  active: boolean;
  shopId: string;
  shopName: string;
  shopImageUrl: string | null;
  product: ProductResponseDto;
  photos: ProductPhotoDto[];
};

export type PagedList<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type SubcategoryIdNameResponseDto = {
  id: number;
  name: string;
};

export type CategoryResponseDto = {
  id: number;
  name: string;
  subcategories: SubcategoryIdNameResponseDto[];
};

export type CreateCategoryRequestDto = {
  name: string;
};

export type EditCategoryRequestDto = {
  name: string;
};

export type CategoryWithoutChildResponseDto = {
  id: number;
  name: string;
};

export type SubcategoryResponseDto = {
  id: number;
  name: string;
  categoryId: number;
  categoryName: string;
};

export type CreateSubcategoryRequestDto = {
  name: string;
  categoryId: number;
};

export type EditSubcategoryRequestDto = {
  name: string;
  categoryId: number;
};

export type CartItemResponseDto = {
  productId: number;
  quantity: number;
  maxQuantity: number;
  productName: string;
  unitPrice: number;
  discountPrice: number | null;
  subtotal: number;
  productImageUrl: string;
  shopId: string;
  shopName: string;
  shopImageUrl: string | null;
  categoryIds: number[];
};

export type AddToCartRequestDto = {
  productId: number;
  quantity: number;
};

export type UpdateCartItemRequestDto = {
  productId: number;
  quantity: number;
};

export type RemoveFromCartRequestDto = {
  productId: number;
};

export type OrderProductResponseDto = {
  id: number;
  productId: number;
  photos: ProductPhotoDto[];
  name: string;
  price: number;
  quantity: number;
  subtotal: number;
};

export type SalesOrderStatus =
  | 'PendingPayment'
  | 'PendingConfirmation'
  | 'Tracking'
  | 'Delivered'
  | 'Cancelled';
export type PaymentMethod = 'Cod' | 'Vnpay';

export type SalesOrderResponseDto = {
  id: number;
  orderTime: string;
  subtotal: number;
  shippingFee: number;
  productDiscountAmount: number;
  shippingDiscountAmount: number;
  total: number;
  buyerId: string;
  buyerName: string;
  shopId: string;
  shopName: string;
  shippingOrderCode: string;
  shippingName: string;
  shippingPhone: string;
  shippingAddress: string;
  shippingWardId: number;
  shippingDistrictId: number;
  shippingProvinceId: number;
  shippingWardName: string;
  shippingDistrictName: string;
  shippingProvinceName: string;
  productCouponCode: string;
  shippingCouponCode: string;
  paymentMethod: PaymentMethod;
  status: SalesOrderStatus;
  orderProducts: OrderProductResponseDto[];
};

export type ListOrdersRequest = {
  orderId?: number;
  fromDate?: string;
  toDate?: string;
  status?: SalesOrderStatus;
  buyerId?: string;
  shopId?: string;
  pageSize?: number;
  pageNumber?: number;
};

export type CheckoutRequestDto = {
  productCouponCode?: string;
  shippingCouponCode?: string;
  shippingName: string;
  shippingPhone: string;
  shippingAddress: string;
  shippingWardId: number;
  paymentMethod: PaymentMethod;
  productIds: number[];
};

export type CheckoutPricePreviewRequestDto = {
  productCouponCode?: string;
  shippingCouponCode?: string;
  shippingName: string;
  shippingPhone: string;
  shippingAddress: string;
  shippingWardId: number;
  productIds: number[];
};

export type CheckoutPricePreviewResponseDto = {
  subtotal: number;
  shippingFee: number;
  productDiscountAmount: number;
  shippingDiscountAmount: number;
  total: number;
  appliedProductCoupon?: string;
  appliedShippingCoupon?: string;
};

export type CheckoutResponseDto = {
  paymentUrl?: string;
  salesOrders: SalesOrderResponseDto[];
};

export type UserAddressResponseDto = {
  id: number;
  name: string;
  phoneNumber: string;
  address: string;
  wardId: number;
  districtId: number;
  provinceId: number;
  wardName: string;
  districtName: string;
  provinceName: string;
  isDefault: boolean;
};

export type AddUserAddressRequestDto = {
  name: string;
  phoneNumber: string;
  address: string;
  wardId: number;
  isDefault: boolean;
};

export type EditUserAddressRequestDto = {
  name: string;
  phoneNumber: string;
  address: string;
  wardId: number;
  isDefault: boolean;
};

export type CouponResponseDto = {
  code: string;
  active: boolean;
  startTime: string;
  endTime: string;
  type: 'Product' | 'Shipping';
  discountType: 'Percent' | 'Amount';
  value: number;
  minOrderValue: number;
  maxDiscountAmount: number;
  allowMultipleUse: boolean;
  maxUseCount: number;
  usedCount: number;
  createdAt: string;
  updatedAt: string;
  categories: CategoryResponseDto[];
};

export type CreateCouponRequestDto = {
  code: string;
  active: boolean;
  startTime: Date;
  endTime: Date;
  type: 'Product' | 'Shipping';
  discountType: 'Percent' | 'Amount';
  value: number;
  minOrderValue: number;
  maxDiscountAmount: number;
  allowMultipleUse: boolean;
  maxUseCount: number;
  categoryIds: number[];
};

export type EditCouponRequestDto = {
  active: boolean;
  startTime: Date;
  endTime: Date;
  type: 'Product' | 'Shipping';
  discountType: 'Percent' | 'Amount';
  value: number;
  minOrderValue: number;
  maxDiscountAmount: number;
  allowMultipleUse: boolean;
  maxUseCount: number;
  categoryIds: number[];
};

export type PaymentResponse = {
  paymentId: number;
  isSuccess: boolean;
  description: string | null;
  timestamp: string;
  vnpayTransactionId: number;
  paymentMethod: string | null;
  responseCode: string | null;
  responseDescription: string | null;
  transactionCode: string | null;
  transactionDescription: string | null;
  bankCode: string | null;
  bankTransactionId: string | null;
};

export type ShopPerformanceRequest = {
  shopId: string;
  timeRange?: 'Days' | 'Months' | 'Years' | 'All';
  timeValue?: number;
};

export type ShopPerformanceResponseDto = {
  time: string;
  quantity: number;
  value: number;
  orderCount: number;
};

export type ShopOrderStatsResponseDto = {
  totalOrders: number;
  averageRating: number;
}; // Define types needed for reviews

export type ReviewResponseDto = {
  id: number;
  productId: number;
  userId: string;
  rating: number;
  review: string | null;
  createdAt: string;
  updatedAt: string | null;
};

export type CreateReviewRequestDto = {
  productId: number;
  rating: number;
  review?: string;
};

export type UpdateReviewRequestDto = {
  rating: number;
  review?: string;
};

export type ListReviewsRequest = {
  productId?: number;
  userId?: string;
  pageSize?: number;
  pageNumber?: number;
};

// Product types
export type CreateProductRequestDto = {
  name: string;
  description: string;
  regularPrice: number;
  quantity: number;
  active: boolean;
  length: number;
  width: number;
  height: number;
  weight: number;
  subcategoryIds: number[];
};

export type EditProductRequestDto = {
  name: string;
  description: string;
  regularPrice: number;
  quantity: number;
  active: boolean;
  length: number;
  width: number;
  height: number;
  weight: number;
  subcategoryIds: number[];
};

// Product discount types
export type ProductDiscountResponseDto = {
  id: number;
  discountPrice: number;
  startTime: string;
  endTime: string;
  productId: number;
  productName: string;
  regularPrice: number;
  createdAt: string;
  updatedAt: string;
};

export type AddProductDiscountRequestDto = {
  discountPrice: number;
  startTime: Date;
  endTime: Date;
};

export type EditProductDiscountRequestDto = {
  discountPrice: number;
  startTime: Date;
  endTime: Date;
};

// Product photo types
export type UpdateProductPhotoDisplayOrderRequestDto = {
  key: string;
  displayOrder: number;
};

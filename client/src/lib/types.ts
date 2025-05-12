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
  role: string;
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
  userId: string;
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

export type UserAddressResponseDto = {
  id: number;
  name: string;
  phoneNumber: string;
  address: string;
  wardId: number;
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
  startTime: Date;
  endTime: Date;
  type: 'Product' | 'Shipping';
  discountType: 'Percent' | 'Amount';
  value: number;
  minOrderValue: number;
  maxDiscountAmount: number;
  allowMultipleUse: boolean;
  maxUseCount: number;
  usedCount: number;
  createdAt: Date;
  updatedAt: Date;
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

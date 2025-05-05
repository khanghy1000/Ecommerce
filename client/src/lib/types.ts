export type ErrorResponse = {
  detail?: string;
  status?: number;
  title?: string;
  type?: string;
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

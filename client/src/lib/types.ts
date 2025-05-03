export type ErrorResponse = {
  detail: string;
  status: number;
  title: string;
  type: string;
}

export type UserInfoResponse = {
  displayName: string;
  email: string;
  id: string;
  imageUrl: string | null;
  phoneNumber: string | null;
  address: string;
  wardName: string;
  districtName: string;
  provinceName: string;
  wardId: number;
  districtId: number;
  provinceId: number;
  role: 'Buyer' | 'Shop' | 'Admin';
} | null;

export type LoginRequest = {
  email: string;
  password: string;
};

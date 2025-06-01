export function formatPrice(price: number): string {
  return price.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
}
export function getOrderStatusText(status: string) {
  if (status === 'PendingConfirmation') return 'Pending Confirmation';
  if (status === 'PendingPayment') return 'Pending Payment';
  return status;
}

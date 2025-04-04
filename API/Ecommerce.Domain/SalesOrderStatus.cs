namespace Ecommerce.Domain;

public enum SalesOrderStatus
{
    PendingPayment,
    PendingConfirmation,
    Tracking,
    Delivered,
    Cancelled,
}

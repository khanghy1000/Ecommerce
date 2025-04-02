using System.Net;
using System.Net.Sockets;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace Ecommerce.Infrastructure.Payments;

public class PaymentService(IHttpContextAccessor httpContextAccessor, IVnpay vnpay)
    : IPaymentService
{
    public async Task<string> CreatePaymentUrl(double money, string description)
    {
        var request = new PaymentRequest
        {
            PaymentId = DateTime.Now.Ticks,
            Money = money,
            Description = description,
            IpAddress = GetIpAddress(), // IP address of the device performing the transaction
            BankCode = BankCode.ANY, // Optional. Default is all transaction methods
            CreatedDate = DateTime.Now, // Optional. Default is current time
            Currency = Currency.VND, // Optional. Default is VND (Vietnamese Dong)
            Language = DisplayLanguage.Vietnamese, // Optional. Default is Vietnamese
        };

        return await Task.Run(() => vnpay.GetPaymentUrl(request));
    }

    public async Task<PaymentResult?> GetPaymentResult()
    {
        var query = httpContextAccessor.HttpContext?.Request.Query;
        if (query == null)
            return null;

        return await Task.Run(() => vnpay.GetPaymentResult(query));
    }

    private string GetIpAddress()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null)
            throw new NullReferenceException("HttpContext is null");

        var remoteIpAddress = context.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
            throw new NullReferenceException("Remote IP address is null");

        if (remoteIpAddress.AddressFamily != AddressFamily.InterNetworkV6)
            return remoteIpAddress.ToString();

        var ip = Dns.GetHostEntry(remoteIpAddress)
            .AddressList.FirstOrDefault(
                (Func<IPAddress, bool>)(x => x.AddressFamily == AddressFamily.InterNetwork)
            );
        return ip == null ? "127.0.0.1" : ip.ToString();
    }
}

using Ecommerce.Application.Interfaces;

namespace Ecommerce.Infrastructure.Shipping;

public class FakeShippingService : IShippingService
{
    public Task<GetShippingDetailsResponse?> GetShippingDetails(string shippingOrderCode)
    {
        var response = new GetShippingDetailsResponse
        {
            Code = 200,
            Message = "Success",
            Data = new GetShippingDetailsResponseData
            {
                OrderCode = shippingOrderCode,
                Status = "delivering",
                FromName = "Store Name",
                FromPhone = "0987654321",
                FromAddress = "123 Store Address",
                FromWardCode = "W01",
                ToName = "Customer Name",
                ToPhone = "0123456789",
                ToAddress = "456 Customer Address",
                ToWardCode = "W02",
                Weight = 500,
                Length = 20,
                Width = 15,
                Height = 10,
                CodAmount = 150000,
                InsuranceValue = 150000,
                Leadtime = DateTime.Now.AddDays(3),
                OrderDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Items = new List<GetShippingDetailsResponseDataItem>
                {
                    new GetShippingDetailsResponseDataItem
                    {
                        Name = "Product 1",
                        Code = "P001",
                        Quantity = 1,
                        Weight = 500,
                        Length = 20,
                        Width = 15,
                        Height = 10,
                        Status = "delivering",
                    },
                },
                Log = new List<GetShippingDetailsResponseDataLog>
                {
                    new GetShippingDetailsResponseDataLog
                    {
                        Status = "delivering",
                        PaymentTypeId = "1",
                        UpdatedDate = DateTime.Now,
                    },
                },
            },
        };

        return Task.FromResult<GetShippingDetailsResponse?>(response);
    }

    public Task<CreateShippingResponse?> PreviewShipping(CreateShippingRequest shippingRequest)
    {
        var response = new CreateShippingResponse
        {
            Code = 200,
            Message = "Success",
            Data = new CreateShippingResponseData
            {
                OrderCode = $"FAKE{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                SortCode = "A1.B2.C3",
                TransType = "truck",
                WardEncode = "W02",
                DistrictEncode = "D02",
                TotalFee = 30000,
                ExpectedDeliveryTime = DateTime.Now.AddDays(3),
                Fee = new CreateShippingResponseDataFee
                {
                    MainService = 25000,
                    Insurance = 5000,
                    StationDo = 0,
                    StationPu = 0,
                    Return = 0,
                    R2S = 0,
                    Coupon = 0,
                    CodFailedFee = 0,
                },
            },
        };

        return Task.FromResult<CreateShippingResponse?>(response);
    }

    public Task<CreateShippingResponse?> CreateShipping(CreateShippingRequest shippingRequest)
    {
        var response = new CreateShippingResponse
        {
            Code = 200,
            Message = "Success",
            Data = new CreateShippingResponseData
            {
                OrderCode = $"FAKE{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                SortCode = "A1.B2.C3",
                TransType = "truck",
                WardEncode = "W02",
                DistrictEncode = "D02",
                TotalFee = 30000,
                ExpectedDeliveryTime = DateTime.Now.AddDays(3),
                Fee = new CreateShippingResponseDataFee
                {
                    MainService = 25000,
                    Insurance = 5000,
                    StationDo = 0,
                    StationPu = 0,
                    Return = 0,
                    R2S = 0,
                    Coupon = 0,
                    CodFailedFee = 0,
                },
            },
        };

        return Task.FromResult<CreateShippingResponse?>(response);
    }

    public Task<CancelShippingResponse?> CancelShipping(string shippingOrderCode)
    {
        var response = new CancelShippingResponse
        {
            Code = 200,
            Message = "Success",
            Data = new List<CancelShippingResponseData>
            {
                new CancelShippingResponseData
                {
                    OrderCode = shippingOrderCode,
                    Result = true,
                    Message = "Order canceled successfully",
                },
            },
        };

        return Task.FromResult<CancelShippingResponse?>(response);
    }
}

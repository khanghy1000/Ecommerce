namespace Ecommerce.Application.Interfaces;

public interface IShippingService
{
    Task<GetShippingDetailsResponse?> GetShippingDetails(string shippingOrderCode);
    Task<CreateShippingResponse?> PreviewShipping(CreateShippingRequest shippingRequest);
    Task<CreateShippingResponse?> CreateShipping(CreateShippingRequest shippingRequest);
    Task<CancelShippingResponse?> CancelShipping(string shippingOrderCode);
}

public class CreateShippingRequest
{
    // Mã người thanh toán phí dịch vụ.
    // 1: Người bán/Người gửi.
    // 2: Người mua/Người nhận.
    public required int PaymentTypeId { get; set; }
    public string? Note { get; set; }
    public required string RequiredNote { get; set; } // CHOTHUHANG, CHOXEMHANGKHONGTHU, KHONGCHOXEMHANG
    public required string FromName { get; set; }
    public required string FromPhone { get; set; }
    public required string FromAddress { get; set; }
    public required string FromWardName { get; set; }
    public required string FromDistrictName { get; set; }
    public required string FromProvinceName { get; set; }
    public required string ToName { get; set; }
    public required string ToPhone { get; set; }
    public required string ToAddress { get; set; }
    public required string ToWardName { get; set; }
    public required string ToDistrictName { get; set; }
    public required string ToProvinceName { get; set; }
    public required int CodAmount { get; set; }
    public required int Length { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required int Weight { get; set; }
    public int ServiceTypeId { get; set; } = 2; // 2: Hàng nhẹ, 5: Hàng nặng
    public required List<CreateShippingRequestItem> Items { get; set; } = [];
}

public class CreateShippingRequestItem
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required int Quantity { get; set; }
    public required int Price { get; set; }
    public required int Length { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required int Weight { get; set; }
}

public class CreateShippingResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public CreateShippingResponseData? Data { get; set; }
    public string? MessageDisplay { get; set; }
}

public class CreateShippingResponseData
{
    public string OrderCode { get; set; } = "";
    public string SortCode { get; set; } = "";
    public string TransType { get; set; } = "";
    public string WardEncode { get; set; } = "";
    public string DistrictEncode { get; set; } = "";
    public CreateShippingResponseDataFee? Fee { get; set; }
    public int TotalFee { get; set; }
    public DateTime ExpectedDeliveryTime { get; set; }
}

public class CreateShippingResponseDataFee
{
    public int MainService { get; set; }
    public int Insurance { get; set; }
    public int StationDo { get; set; }
    public int StationPu { get; set; }
    public int Return { get; set; }
    public int R2S { get; set; }
    public int Coupon { get; set; }
    public int CodFailedFee { get; set; }
}

public class CancelShippingResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public List<CancelShippingResponseData>? Data { get; set; }
}

public class CancelShippingResponseData
{
    public string OrderCode { get; set; } = "";
    public bool Result { get; set; }
    public string Message { get; set; } = "";
}

public class GetShippingDetailsResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public GetShippingDetailsResponseData? Data { get; set; }
}

public class GetShippingDetailsResponseData
{
    public int ShopId { get; set; }
    public int ClientId { get; set; }
    public string ReturnName { get; set; } = "";
    public string ReturnPhone { get; set; } = "";
    public string ReturnAddress { get; set; } = "";
    public string ReturnWardCode { get; set; } = "";
    public int ReturnDistrictId { get; set; }
    public GetShippingDetailsResponseDataReturnLocation? ReturnLocation { get; set; }
    public string FromName { get; set; } = "";
    public string FromPhone { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromWardCode { get; set; } = "";
    public int FromDistrictId { get; set; }
    public GetShippingDetailsResponseDataFromLocation? FromLocation { get; set; }
    public int DeliverStationId { get; set; }
    public string ToName { get; set; } = "";
    public string ToPhone { get; set; } = "";
    public string ToAddress { get; set; } = "";
    public string ToWardCode { get; set; } = "";
    public int ToDistrictId { get; set; }
    public GetShippingDetailsResponseDataToLocation? ToLocation { get; set; }
    public int Weight { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int ConvertedWeight { get; set; }
    public string ImageIds { get; set; } = "";
    public int ServiceTypeId { get; set; }
    public int ServiceId { get; set; }
    public int PaymentTypeId { get; set; }
    public List<int> PaymentTypeIds { get; set; } = [];
    public int CustomServiceFee { get; set; }
    public string SortCode { get; set; } = "";
    public int CodAmount { get; set; }
    public string CodCollectDate { get; set; } = "";
    public string CodTransferDate { get; set; } = "";
    public bool IsCodTransferred { get; set; }
    public bool IsCodCollected { get; set; }
    public int InsuranceValue { get; set; }
    public int OrderValue { get; set; }
    public int PickStationId { get; set; }
    public string ClientOrderCode { get; set; } = "";
    public int CodFailedAmount { get; set; }
    public string CodFailedCollectDate { get; set; } = "";
    public string RequiredNote { get; set; } = "";
    public string Content { get; set; } = "";
    public string Note { get; set; } = "";
    public string EmployeeNote { get; set; } = "";
    public string SealCode { get; set; } = "";
    public DateTime PickupTime { get; set; }
    public List<GetShippingDetailsResponseDataItem> Items { get; set; } = [];
    public string Coupon { get; set; } = "";
    public string Id { get; set; } = "";
    public string OrderCode { get; set; } = "";
    public string VersionNo { get; set; } = "";
    public string UpdatedIp { get; set; } = "";
    public int UpdatedEmployee { get; set; }
    public int UpdatedClient { get; set; }
    public string UpdatedSource { get; set; } = "";
    public DateTime UpdatedDate { get; set; }
    public int UpdatedWarehouse { get; set; }
    public string CreatedIp { get; set; } = "";
    public int CreatedEmployee { get; set; }
    public int CreatedClient { get; set; }
    public string CreatedSource { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = "";
    public int PickWarehouseId { get; set; }
    public int DeliverWarehouseId { get; set; }
    public int CurrentWarehouseId { get; set; }
    public int ReturnWarehouseId { get; set; }
    public int NextWarehouseId { get; set; }
    public DateTime Leadtime { get; set; }
    public DateTime OrderDate { get; set; }
    public string SocId { get; set; } = "";
    public DateTime S2rTime { get; set; }
    public DateTime ReturnTime { get; set; }
    public string FinishDate { get; set; } = "";
    public List<string> Tag { get; set; } = [];
    public List<GetShippingDetailsResponseDataLog> Log { get; set; } = [];
}

public class GetShippingDetailsResponseDataFromLocation
{
    public double Lat { get; set; }
    public double Long { get; set; }
    public string CellCode { get; set; } = "";
    public string PlaceId { get; set; } = "";
    public int TrustLevel { get; set; }
    public string Wardcode { get; set; } = "";
}

public class GetShippingDetailsResponseDataItem
{
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public int Quantity { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public string Status { get; set; } = "";
}

public class GetShippingDetailsResponseDataLog
{
    public string Status { get; set; } = "";
    public string PaymentTypeId { get; set; } = "";
    public DateTime UpdatedDate { get; set; }
}

public class GetShippingDetailsResponseDataReturnLocation
{
    public double Lat { get; set; }
    public double Long { get; set; }
    public string CellCode { get; set; } = "";
    public string PlaceId { get; set; } = "";
    public int TrustLevel { get; set; }
    public string Wardcode { get; set; } = "";
}

public class GetShippingDetailsResponseDataToLocation
{
    public double Lat { get; set; }
    public double Long { get; set; }
    public string CellCode { get; set; } = "";
    public string PlaceId { get; set; } = "";
    public int TrustLevel { get; set; }
    public string Wardcode { get; set; } = "";
}

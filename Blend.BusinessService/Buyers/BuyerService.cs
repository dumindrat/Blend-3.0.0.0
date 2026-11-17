using Blend.DataService.Buyers;

namespace Blend.BusinessService.Buyers;

public sealed class BuyerService : IBuyerService
{
    private readonly IBuyerDataService _buyerDataService;

    public BuyerService(IBuyerDataService buyerDataService)
    {
        _buyerDataService = buyerDataService ?? throw new ArgumentNullException(nameof(buyerDataService));
    }

    public IReadOnlyList<Buyer> GetAllBuyers(string searchText, int companyId) =>
        _buyerDataService.GetAllBuyers(searchText, companyId).Select(ToBuyer).ToArray();

    public BusinessOperationResult CreateBuyer(Buyer buyer)
    {
        ArgumentNullException.ThrowIfNull(buyer);
        return ToOperationResult(
            _buyerDataService.CreateBuyer(ToRecord(buyer)),
            "The buyer could not be created.");
    }

    public BusinessOperationResult UpdateBuyer(Buyer buyer)
    {
        ArgumentNullException.ThrowIfNull(buyer);
        return ToOperationResult(
            _buyerDataService.UpdateBuyer(ToRecord(buyer)),
            "The buyer could not be updated.");
    }

    public BusinessOperationResult DeleteBuyer(int buyerId) => ToOperationResult(
        _buyerDataService.DeleteBuyer(buyerId),
        "The buyer could not be deleted.");

    private static BusinessOperationResult ToOperationResult(bool succeeded, string failureMessage) =>
        succeeded ? BusinessOperationResult.Success() : BusinessOperationResult.Failure(failureMessage);

    private static Buyer ToBuyer(BuyerRecord record) => new()
    {
        BuyerId = record.BuyerId,
        CompanyId = record.CompanyId,
        BuyerCode = record.BuyerCode,
        BuyerName = record.BuyerName,
        Address = record.Address,
        Email = record.Email,
        ModifiedDate = record.ModifiedDate,
        RowVersion = record.RowVersion,
        User = record.User
    };

    private static BuyerRecord ToRecord(Buyer buyer) => new()
    {
        BuyerId = buyer.BuyerId,
        CompanyId = buyer.CompanyId,
        BuyerCode = buyer.BuyerCode,
        BuyerName = buyer.BuyerName,
        Address = buyer.Address,
        Email = buyer.Email,
        ModifiedDate = buyer.ModifiedDate,
        RowVersion = buyer.RowVersion,
        User = buyer.User
    };
}

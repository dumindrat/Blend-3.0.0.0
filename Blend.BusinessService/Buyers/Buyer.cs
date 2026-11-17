namespace Blend.BusinessService.Buyers;

public sealed class Buyer
{
    public int BuyerId { get; set; }
    public int CompanyId { get; set; }
    public string? BuyerCode { get; set; }
    public string? BuyerName { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? ModifiedDate { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}

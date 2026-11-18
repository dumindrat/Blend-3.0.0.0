namespace Blend.DataService.Contracts;

public sealed class ContractRecord
{
    public long ContractId { get; set; }
    public int CompanyId { get; set; }
    public string? ContractName { get; set; }
    public string? Contract { get; set; }
    public int BuyerId { get; set; }
    public string? BuyerCode { get; set; }
    public string? BuyerName { get; set; }
    public string? CompletionDate { get; set; }
    public string? ModifiedDate { get; set; }
    public bool IsClosed { get; set; }
    public string? CloseStatus { get; set; }
    public byte[]? RowVersion { get; set; }
    public string? User { get; set; }
}

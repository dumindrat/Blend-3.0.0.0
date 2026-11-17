namespace Blend.DataService.Buyers
{
    public sealed class BuyerRecord
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

        // trn_BuyerGet returns the buyer name in a column called "Name".
        public string? Name
        {
            get => BuyerName;
            set => BuyerName = value;
        }
    }
}

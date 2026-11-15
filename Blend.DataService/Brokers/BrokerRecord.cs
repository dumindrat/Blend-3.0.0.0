namespace Blend.DataService.Brokers
{
    public sealed class BrokerRecord
    {
        public int BrokerId { get; set; }
        public int CompanyId { get; set; }
        public string? BrokerCode { get; set; }
        public string? BrokerName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Tel { get; set; }
        public string? ContactPerson { get; set; }
        public string? ModifiedDate { get; set; }
        public byte[]? RowVersion { get; set; }
        public string? User { get; set; }
    }
}

namespace Blend.DataService.Documents;

public sealed class DocumentRecord
{
    public int Id { get; set; }
    public int DocType { get; set; }
    public int DocNo { get; set; }
    public string? Prefix { get; set; }
    public string? DocFormat { get; set; }
    public string? Description { get; set; }
    public int? ModuleId { get; set; }
    public int? CompanyId { get; set; }
    public string? User { get; set; }
}

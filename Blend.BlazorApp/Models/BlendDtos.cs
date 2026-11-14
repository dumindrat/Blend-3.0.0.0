namespace Blend.BlazorApp.Models;

public sealed class BlendIssueNote
{
    public int Id { get; set; }
    public string IssueNoteNo { get; set; } = string.Empty;
    public string BlendNo { get; set; } = string.Empty;
    public int BlendId { get; set; }
    public bool IsAutoDocNo { get; set; }
    public string AddedDate { get; set; } = string.Empty;
    public string ModifiedDate { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string ModifiedUser { get; set; } = string.Empty;
}

public sealed class BlendChoice
{
    public int BlendId { get; set; }
    public string BlendNo { get; set; } = string.Empty;
}

public sealed class CompanyChoice
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

public sealed class IssueNoteItem
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string UomName { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal Price { get; set; }
    public string StoreLocation { get; set; } = string.Empty;
}

public sealed class GeneratedDocumentNumber
{
    public string FormattedDocNo { get; set; } = string.Empty;
}

public sealed class LogoutRequest
{
    public string UserName { get; set; } = string.Empty;
    public string CertId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

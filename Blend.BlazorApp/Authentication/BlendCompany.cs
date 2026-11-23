using System.ComponentModel.DataAnnotations;

namespace Blend.BlazorApp.Authentication;

public sealed class BlendCompany
{
    public Guid Id { get; set; }

    public int CompanyNumber { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string DateFormat { get; set; } = CompanyDateFormats.MonthFirst;

    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Region { get; set; }

    [StringLength(30)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    public DateOnly SubscriptionStartDate { get; set; }
    public DateOnly SubscriptionEndDate { get; set; }
    public bool IsEnabled { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Roles = BlendRoles.Admin, Policy = "PasswordChangeComplete")]
public sealed class CompaniesModel : PageModel
{
    private readonly BlendIdentityDbContext _database;
    private readonly RoleManager<BlendIdentityRole> _roleManager;

    public CompaniesModel(BlendIdentityDbContext database, RoleManager<BlendIdentityRole> roleManager)
    {
        _database = database;
        _roleManager = roleManager;
    }

    [BindProperty]
    public CompanyInput Input { get; set; } = new();

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public string? SuccessMessage { get; private set; }
    public bool ShowCreateDialog { get; private set; }

    public async Task OnGetAsync() => await LoadCompaniesAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ShowCreateDialog = true;
        NormalizeInput();
        ValidateSubscriptionDates();
        if (ModelState.IsValid)
        {
            var company = new BlendCompany
            {
                Id = Guid.NewGuid(),
                Name = Input.Name,
                AddressLine1 = Input.AddressLine1,
                AddressLine2 = Input.AddressLine2,
                City = Input.City,
                Region = Input.Region,
                PostalCode = Input.PostalCode,
                Country = Input.Country,
                SubscriptionStartDate = Input.SubscriptionStartDate,
                SubscriptionEndDate = Input.SubscriptionEndDate,
                IsEnabled = Input.IsEnabled
            };
            await using var transaction = await _database.Database.BeginTransactionAsync(HttpContext.RequestAborted);
            _database.Companies.Add(company);
            await _database.SaveChangesAsync(HttpContext.RequestAborted);
            var roleCreationErrors = await CreateDefaultRolesAsync(company);
            if (roleCreationErrors.Count > 0)
            {
                await transaction.RollbackAsync(HttpContext.RequestAborted);
                foreach (var error in roleCreationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                await transaction.CommitAsync(HttpContext.RequestAborted);
                SuccessMessage = $"Company '{company.Name}' created with Company Admin and User roles.";
                ShowCreateDialog = false;
                Input = new CompanyInput { SubscriptionStartDate = DateOnly.FromDateTime(DateTime.UtcNow), SubscriptionEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), IsEnabled = true };
            }
        }

        await LoadCompaniesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id)
    {
        NormalizeInput();
        ValidateSubscriptionDates();
        if (!ModelState.IsValid)
        {
            await LoadCompaniesAsync();
            return Page();
        }

        var company = await _database.Companies.FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);
        if (company is null)
        {
            ModelState.AddModelError(string.Empty, "The selected company no longer exists.");
            await LoadCompaniesAsync();
            return Page();
        }

        company.Name = Input.Name;
        company.AddressLine1 = Input.AddressLine1;
        company.AddressLine2 = Input.AddressLine2;
        company.City = Input.City;
        company.Region = Input.Region;
        company.PostalCode = Input.PostalCode;
        company.Country = Input.Country;
        company.SubscriptionStartDate = Input.SubscriptionStartDate;
        company.SubscriptionEndDate = Input.SubscriptionEndDate;
        company.IsEnabled = Input.IsEnabled;
        await _database.SaveChangesAsync(HttpContext.RequestAborted);
        SuccessMessage = $"Company '{company.Name}' updated.";
        await LoadCompaniesAsync();
        return Page();
    }

    private async Task<IReadOnlyList<string>> CreateDefaultRolesAsync(BlendCompany company)
    {
        var errors = new List<string>();
        var defaults = new[]
        {
            (Name: "Company Admin", Permissions: new[] { BlendPermissions.ManageUsers, BlendPermissions.ManageRoles, BlendPermissions.ManageMenuItems }),
            (Name: BlendRoles.User, Permissions: Array.Empty<string>())
        };
        foreach (var (displayName, permissions) in defaults)
        {
            var role = new BlendIdentityRole($"{company.Id:N}:{Guid.NewGuid():N}", displayName, company.Id);
            var createResult = await _roleManager.CreateAsync(role);
            if (!createResult.Succeeded)
            {
                errors.AddRange(createResult.Errors.Select(error => error.Description));
                continue;
            }

            foreach (var permission in permissions)
            {
                var claimResult = await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(BlendPermissions.ClaimType, permission));
                if (!claimResult.Succeeded)
                {
                    errors.AddRange(claimResult.Errors.Select(error => error.Description));
                }
            }
        }

        return errors;
    }

    private void NormalizeInput()
    {
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        Input.AddressLine1 = Clean(Input.AddressLine1);
        Input.AddressLine2 = Clean(Input.AddressLine2);
        Input.City = Clean(Input.City);
        Input.Region = Clean(Input.Region);
        Input.PostalCode = Clean(Input.PostalCode);
        Input.Country = Clean(Input.Country);
    }

    private void ValidateSubscriptionDates()
    {
        if (Input.SubscriptionEndDate < Input.SubscriptionStartDate)
        {
            ModelState.AddModelError(nameof(Input.SubscriptionEndDate), "The subscription end date must be on or after the start date.");
        }
    }

    private async Task LoadCompaniesAsync()
    {
        Companies = await _database.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .ToListAsync(HttpContext.RequestAborted);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class CompanyInput
    {
        [Required]
        [StringLength(160)]
        public string Name { get; set; } = string.Empty;

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

        [DataType(DataType.Date)]
        public DateOnly SubscriptionStartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [DataType(DataType.Date)]
        public DateOnly SubscriptionEndDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

        public bool IsEnabled { get; set; } = true;
    }
}

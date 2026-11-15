using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Account;

[Authorize(Policy = "PasswordChangeComplete")]
public sealed class WorkingCompanyModel : PageModel
{
    private readonly BlendIdentityDbContext _database;
    private readonly UserManager<BlendIdentityUser> _userManager;
    private readonly BlendUserClaimsPrincipalFactory _claimsPrincipalFactory;

    public WorkingCompanyModel(
        BlendIdentityDbContext database,
        UserManager<BlendIdentityUser> userManager,
        BlendUserClaimsPrincipalFactory claimsPrincipalFactory)
    {
        _database = database;
        _userManager = userManager;
        _claimsPrincipalFactory = claimsPrincipalFactory;
    }

    public IReadOnlyList<CompanyOption> Companies { get; private set; } = [];
    public Guid? WorkingCompanyId { get; private set; }
    public string? SuccessMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public bool Changed { get; set; }

    [BindProperty]
    [Required]
    public Guid? SelectedCompanyId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SuccessMessage = Changed ? "Working company updated." : null;
        await LoadCompaniesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCompaniesAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var companyId = SelectedCompanyId!.Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var company = User.IsInRole(BlendRoles.Admin)
            ? await _database.Companies.SingleOrDefaultAsync(item => item.Id == companyId, HttpContext.RequestAborted)
            : await _database.UserCompanies
                .Where(membership => membership.UserId == user.Id && membership.CompanyId == companyId)
                .Select(membership => membership.Company)
                .SingleOrDefaultAsync(HttpContext.RequestAborted);
        if (company is null
            || !company.IsEnabled
            || today < company.SubscriptionStartDate
            || today > company.SubscriptionEndDate)
        {
            ModelState.AddModelError(nameof(SelectedCompanyId), "Choose a company with active access.");
            await LoadCompaniesAsync();
            return Page();
        }

        user.WorkingCompanyId = companyId;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadCompaniesAsync();
            return Page();
        }

        var principal = await _claimsPrincipalFactory.CreatePrincipalAsync(user, companyId);
        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        return LocalRedirect("/");
    }

    private async Task LoadCompaniesAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            Companies = [];
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);
        WorkingCompanyId = user?.WorkingCompanyId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Companies = User.IsInRole(BlendRoles.Admin)
            ? await _database.Companies.AsNoTracking()
                .OrderBy(company => company.Name)
                .Select(company => new CompanyOption(
                    company.Id,
                    company.Name,
                    company.IsEnabled
                        && today >= company.SubscriptionStartDate
                        && today <= company.SubscriptionEndDate,
                    company.Id == WorkingCompanyId))
                .ToListAsync(HttpContext.RequestAborted)
            : await _database.UserCompanies.AsNoTracking()
                .Where(membership => membership.UserId == userId)
                .OrderBy(membership => membership.Company.Name)
                .Select(membership => new CompanyOption(
                    membership.CompanyId,
                    membership.Company.Name,
                    membership.Company.IsEnabled
                        && today >= membership.Company.SubscriptionStartDate
                        && today <= membership.Company.SubscriptionEndDate,
                    membership.CompanyId == WorkingCompanyId))
                .ToListAsync(HttpContext.RequestAborted);
        SelectedCompanyId ??= WorkingCompanyId;
    }

    public sealed record CompanyOption(Guid Id, string Name, bool IsAvailable, bool IsWorkingCompany);
}

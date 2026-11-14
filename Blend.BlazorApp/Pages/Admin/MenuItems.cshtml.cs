using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Policy = "PasswordChangeComplete")]
[Authorize(Policy = BlendPermissions.ManageMenuItemsPolicy)]
public sealed class MenuItemsModel : PageModel
{
    private readonly BlendIdentityDbContext _database;

    public MenuItemsModel(BlendIdentityDbContext database)
    {
        _database = database;
    }

    [BindProperty]
    public MenuItemInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? CompanyId { get; set; }

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public IReadOnlyList<BlendMenuItem> MenuItems { get; private set; } = [];
    public string? SuccessMessage { get; private set; }

    public async Task OnGetAsync() => await LoadMenuItemsAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        Input.GroupName = Input.GroupName?.Trim() ?? string.Empty;
        Input.Path = Input.Path?.Trim() ?? string.Empty;
        ValidateLocalPath();
        var companyId = await GetManagedCompanyIdAsync();
        if (companyId is null)
        {
            ModelState.AddModelError(string.Empty, "Select a company you are allowed to manage.");
        }

        if (ModelState.IsValid)
        {
            _database.MenuItems.Add(new BlendMenuItem
            {
                CompanyId = companyId!.Value,
                Name = Input.Name,
                GroupName = Input.GroupName,
                Path = Input.Path,
                SortOrder = Input.SortOrder
            });
            await _database.SaveChangesAsync(HttpContext.RequestAborted);
            SuccessMessage = $"Menu item '{Input.Name}' created.";
            Input = new MenuItemInput();
        }

        await LoadMenuItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id)
    {
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        Input.GroupName = Input.GroupName?.Trim() ?? string.Empty;
        Input.Path = Input.Path?.Trim() ?? string.Empty;
        ValidateLocalPath();
        var companyId = await GetManagedCompanyIdAsync();
        if (companyId is null)
        {
            ModelState.AddModelError(string.Empty, "Select a company you are allowed to manage.");
        }

        if (!ModelState.IsValid)
        {
            await LoadMenuItemsAsync();
            return Page();
        }

        var menuItem = await _database.MenuItems.FirstOrDefaultAsync(
            item => item.Id == id && item.CompanyId == companyId,
            HttpContext.RequestAborted);
        if (menuItem is null)
        {
            ModelState.AddModelError(string.Empty, "The selected menu item no longer exists.");
            await LoadMenuItemsAsync();
            return Page();
        }

        menuItem.Name = Input.Name;
        menuItem.GroupName = Input.GroupName;
        menuItem.Path = Input.Path;
        menuItem.SortOrder = Input.SortOrder;
        await _database.SaveChangesAsync(HttpContext.RequestAborted);
        SuccessMessage = $"Menu item '{menuItem.Name}' updated.";
        await LoadMenuItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var companyId = await GetManagedCompanyIdAsync();
        if (companyId is null)
        {
            ModelState.AddModelError(string.Empty, "Select a company you are allowed to manage.");
            await LoadMenuItemsAsync();
            return Page();
        }

        var menuItem = await _database.MenuItems.FirstOrDefaultAsync(
            item => item.Id == id && item.CompanyId == companyId,
            HttpContext.RequestAborted);
        if (menuItem is null)
        {
            ModelState.AddModelError(string.Empty, "The selected menu item no longer exists.");
        }
        else
        {
            _database.MenuItems.Remove(menuItem);
            await _database.SaveChangesAsync(HttpContext.RequestAborted);
            SuccessMessage = $"Menu item '{menuItem.Name}' deleted.";
        }

        await LoadMenuItemsAsync();
        return Page();
    }

    private void ValidateLocalPath()
    {
        if (!Url.IsLocalUrl(Input.Path))
        {
            ModelState.AddModelError(nameof(Input.Path), "Enter an application-local path beginning with a single slash.");
        }
    }

    private async Task LoadMenuItemsAsync()
    {
        var isPlatformAdmin = User.IsInRole(BlendRoles.Admin);
        if (isPlatformAdmin)
        {
            Companies = await _database.Companies.AsNoTracking().OrderBy(company => company.Name).ToListAsync(HttpContext.RequestAborted);
            if (!Guid.TryParse(CompanyId, out var selectedCompanyId) || !Companies.Any(company => company.Id == selectedCompanyId))
            {
                CompanyId = Companies.FirstOrDefault()?.Id.ToString("D");
            }
        }
        else
        {
            Companies = [];
            CompanyId = User.FindFirst(BlendClaims.CompanyId)?.Value;
        }

        if (!Guid.TryParse(CompanyId, out var companyId))
        {
            MenuItems = [];
            return;
        }

        MenuItems = await _database.MenuItems
            .AsNoTracking()
            .Where(item => item.CompanyId == companyId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(HttpContext.RequestAborted);
    }

    private async Task<Guid?> GetManagedCompanyIdAsync()
    {
        if (!Guid.TryParse(CompanyId, out var companyId))
        {
            return null;
        }

        if (User.IsInRole(BlendRoles.Admin))
        {
            return await _database.Companies.AnyAsync(company => company.Id == companyId, HttpContext.RequestAborted)
                ? companyId
                : null;
        }

        return Guid.TryParse(User.FindFirst(BlendClaims.CompanyId)?.Value, out var claimedCompanyId)
            && claimedCompanyId == companyId
                ? companyId
                : null;
    }

    public sealed class MenuItemInput
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string GroupName { get; set; } = "Operations";

        [Required]
        [StringLength(512)]
        public string Path { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }
    }
}

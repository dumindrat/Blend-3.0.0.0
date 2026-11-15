using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Pages.Admin;

[Authorize(Policy = "PasswordChangeComplete")]
[Authorize(Policy = BlendPermissions.ManageMenuItemsPolicy)]
public sealed class MenuItemsModel : PageModel
{
    private static readonly (string Key, string Name, int SortOrder)[] DefaultOperations =
    [
        ("create", "Create", 0),
        ("edit", "Edit", 1),
        ("delete", "Delete", 2),
        ("print", "Print", 3)
    ];

    private readonly BlendIdentityDbContext _database;

    public MenuItemsModel(BlendIdentityDbContext database)
    {
        _database = database;
    }

    [BindProperty]
    public MenuItemInput Input { get; set; } = new();

    [BindProperty]
    [ValidateNever]
    public OperationFormInput OperationInput { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? CompanyId { get; set; }

    public IReadOnlyList<BlendCompany> Companies { get; private set; } = [];
    public IReadOnlyList<BlendMenuItem> MenuItems { get; private set; } = [];
    public IReadOnlyDictionary<int, IReadOnlyList<BlendMenuItemOperation>> OperationsByMenuItem { get; private set; } = new Dictionary<int, IReadOnlyList<BlendMenuItemOperation>>();
    public string? SuccessMessage { get; private set; }
    public bool ShowCreateDialog { get; private set; }
    public int? ShowEditItemId { get; private set; }
    public int? ShowOperationsMenuItemId { get; private set; }

    public async Task OnGetAsync() => await LoadMenuItemsAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ShowCreateDialog = true;
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
            var menuItem = new BlendMenuItem
            {
                CompanyId = companyId!.Value,
                Name = Input.Name,
                GroupName = Input.GroupName,
                Path = Input.Path,
                SortOrder = Input.SortOrder
            };
            _database.MenuItems.Add(menuItem);
            foreach (var operation in DefaultOperations)
            {
                menuItem.Operations.Add(new BlendMenuItemOperation
                {
                    Key = operation.Key,
                    Name = operation.Name,
                    SortOrder = operation.SortOrder
                });
            }

            await _database.SaveChangesAsync(HttpContext.RequestAborted);
            SuccessMessage = $"Menu item '{Input.Name}' created with Create, Edit, Delete, and Print operations.";
            ShowCreateDialog = false;
            Input = new MenuItemInput();
        }

        await LoadMenuItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id)
    {
        ShowEditItemId = id;
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
        ShowEditItemId = null;
        await LoadMenuItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddOperationAsync(int id)
    {
        ModelState.Clear();
        TryValidateModel(OperationInput, nameof(OperationInput));
        OperationInput.Name = OperationInput.Name?.Trim() ?? string.Empty;
        if (!TryCreateOperationKey(OperationInput.Name, out var operationKey))
        {
            ModelState.AddModelError(nameof(OperationInput.Name), "Enter an operation name containing at least one letter or number.");
        }

        var companyId = await GetManagedCompanyIdAsync();
        var menuItem = companyId is null
            ? null
            : await _database.MenuItems.FirstOrDefaultAsync(item => item.Id == id && item.CompanyId == companyId, HttpContext.RequestAborted);
        if (menuItem is null)
        {
            ModelState.AddModelError(string.Empty, "The selected menu item does not belong to a company you are allowed to manage.");
        }
        else
        {
            ShowOperationsMenuItemId = id;
        }

        if (ModelState.IsValid)
        {
            if (await _database.MenuItemOperations.AnyAsync(operation => operation.MenuItemId == id && operation.Key == operationKey, HttpContext.RequestAborted))
            {
                ModelState.AddModelError(nameof(OperationInput.Name), "This operation already exists for the menu item.");
            }
            else
            {
                var sortOrder = await _database.MenuItemOperations
                    .Where(operation => operation.MenuItemId == id)
                    .Select(operation => (int?)operation.SortOrder)
                    .MaxAsync(HttpContext.RequestAborted) ?? -1;
                _database.MenuItemOperations.Add(new BlendMenuItemOperation
                {
                    MenuItemId = id,
                    Key = operationKey,
                    Name = OperationInput.Name,
                    SortOrder = sortOrder + 1
                });
                await _database.SaveChangesAsync(HttpContext.RequestAborted);
                SuccessMessage = $"Operation '{OperationInput.Name}' added to '{menuItem!.Name}'.";
                OperationInput = new OperationFormInput();
            }
        }

        await LoadMenuItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteOperationAsync(int id, int operationId)
    {
        ModelState.Clear();
        var companyId = await GetManagedCompanyIdAsync();
        var operation = companyId is null
            ? null
            : await _database.MenuItemOperations
                .Include(itemOperation => itemOperation.MenuItem)
                .FirstOrDefaultAsync(itemOperation => itemOperation.Id == operationId && itemOperation.MenuItemId == id
                    && itemOperation.MenuItem.CompanyId == companyId, HttpContext.RequestAborted);
        if (operation is null)
        {
            ModelState.AddModelError(string.Empty, "The selected operation does not belong to a company you are allowed to manage.");
        }
        else
        {
            ShowOperationsMenuItemId = id;
            _database.MenuItemOperations.Remove(operation);
            await _database.SaveChangesAsync(HttpContext.RequestAborted);
            SuccessMessage = $"Operation '{operation.Name}' removed. Its role grants were also removed.";
        }

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
        var operations = await _database.MenuItemOperations
            .AsNoTracking()
            .Where(operation => operation.MenuItem.CompanyId == companyId)
            .OrderBy(operation => operation.SortOrder)
            .ThenBy(operation => operation.Name)
            .ToListAsync(HttpContext.RequestAborted);
        OperationsByMenuItem = operations
            .GroupBy(operation => operation.MenuItemId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<BlendMenuItemOperation>)group.ToArray());
    }

    private static bool TryCreateOperationKey(string name, out string key)
    {
        var keyBuilder = new System.Text.StringBuilder();
        var previousWasSeparator = false;
        foreach (var character in name.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                keyBuilder.Append(char.ToLowerInvariant(character));
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator && keyBuilder.Length > 0)
            {
                keyBuilder.Append('-');
                previousWasSeparator = true;
            }
        }

        key = keyBuilder.ToString().Trim('-');
        return key.Length is > 0 and <= 80;
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

    public sealed class OperationFormInput
    {
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class MenuItemInput
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Display name")]
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

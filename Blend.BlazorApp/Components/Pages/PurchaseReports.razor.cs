using System.Security.Claims;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Services;
using Blend.BusinessService.Brokers;
using Blend.BusinessService.Purchases;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Blend.BlazorApp.Components.Pages;

public partial class PurchaseReports
{
    private const string PagePath = "/Reports/Purchase";
    private IReadOnlyList<PurchaseSale> Sales = [];
    private Guid? CompanyId;
    private int? MappingId;
    private string? CompanyName, Error, DialogError;
    private string Search = "";
    private bool Loading = true, Busy, CanView, CanPrint, LoadingBrokers, OpenAfterRender;
    private DialogKind Dialog;
    private PurchaseSale? SelectedSale;
    private IReadOnlyList<Broker> BrokerItems = [];
    private int? BrokerId;
    private DateTime FromDate = DateTime.Today, ToDate = DateTime.Today;
    private string Grade = "", Standard = "", SaleType = "MS", BrokerCode = "", Output = "print";
    private bool WithValues;
    private ElementReference DialogElement;
    private IJSObjectReference? DialogModule, ReportModule;
    private DotNetObjectReference<PurchaseReports>? DialogCallback;
    private string? PermissionReason => CanPrint ? null : "Your role does not have the Purchase Reports Print permission.";
    private string ReportActionReason => CanPrint ? "Report generation is not available yet." : PermissionReason!;
    private string DialogTitle => Dialog switch { DialogKind.Summary => "Sale Summary options", DialogKind.Sheet => "Purchase Sheet options", DialogKind.Advanced => "Advanced filter options", DialogKind.Broker => "Broker-wise purchases", _ => "Report options" };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
            var company = await GetCompanyAsync(user);
            if (company is null) return;
            CompanyId = company.Id;
            MappingId = company.MappingId;
            CompanyName = company.Name;
            await LoadPermissionsAsync(user, company.Id);
            if (CanView) Sales = await Task.Run(() => Purchases.GetAllSales("all", company.MappingId));
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Unable to load purchase report sales for {CompanyId}", CompanyId);
            Error = "Sales could not be loaded. Reload the page and try again.";
        }
        finally { Loading = false; }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!OpenAfterRender) return;
        OpenAfterRender = false;
        DialogModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Buyers.razor.js");
        DialogCallback ??= DotNetObjectReference.Create(this);
        await DialogModule.InvokeVoidAsync("show", DialogElement, DialogCallback, nameof(CancelFromDialogAsync));
    }

    [JSInvokable]
    public Task CancelFromDialogAsync() => InvokeAsync(async () => { await CloseDialogAsync(); StateHasChanged(); });

    public async ValueTask DisposeAsync()
    {
        DialogCallback?.Dispose();
        if (DialogModule is not null) { try { await DialogModule.DisposeAsync(); } catch (JSDisconnectedException) { } }
        if (ReportModule is not null) { try { await ReportModule.DisposeAsync(); } catch (JSDisconnectedException) { } }
    }

    private async Task CloseDialogAsync()
    {
        if (Busy) return;
        if (DialogModule is not null) await DialogModule.InvokeVoidAsync("close", DialogElement);
        Dialog = DialogKind.None;
        SelectedSale = null;
        BrokerItems = [];
        DialogError = null;
    }

    private async Task OpenOptionsAsync(DialogKind kind, PurchaseSale? sale = null)
    {
        if (Busy || Dialog != DialogKind.None) return;
        Busy = true;
        LoadingBrokers = kind == DialogKind.Broker;
        Error = null;
        try
        {
            var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
            var company = await GetCompanyAsync(user);
            if (company is null || company.Id != CompanyId || company.MappingId != MappingId)
            {
                Sales = [];
                CanView = CanPrint = false;
                Error = "The working company changed or is unavailable. Reload the page.";
                return;
            }
            await LoadPermissionsAsync(user, company.Id);
            if (!CanView) { Sales = []; Error = "Your role no longer has access to Purchase Reports."; return; }
            if (!CanPrint) { Error = "Your role no longer has the Purchase Reports Print permission."; return; }
            if ((kind == DialogKind.Summary || kind == DialogKind.Sheet) && (sale is null || !Sales.Any(item => item.SaleId == sale.SaleId)))
            {
                Error = "The selected sale is no longer available. Search and try again.";
                return;
            }
            BrokerItems = kind == DialogKind.Broker ? await Task.Run(() => Brokers.GetAllBrokers("all", company.MappingId)) : [];
            SelectedSale = sale;
            BrokerId = null;
            FromDate = ToDate = DateTime.Today;
            Grade = Standard = BrokerCode = "";
            SaleType = "MS";
            Output = "print";
            WithValues = kind == DialogKind.Summary;
            Dialog = kind;
            OpenAfterRender = true;
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Unable to open purchase report options for {CompanyId}", CompanyId);
            Error = "Report options could not be loaded. Please try again.";
        }
        finally { LoadingBrokers = false; Busy = false; }
    }

    private async Task GenerateSummaryAsync()
    {
        if (Busy || Dialog != DialogKind.Summary || SelectedSale is null) return;
        Busy = true;
        DialogError = null;
        try
        {
            var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
            var company = await GetCompanyAsync(user);
            if (company is null || company.Id != CompanyId || company.MappingId != MappingId)
            {
                CanView = CanPrint = false;
                Error = "The working company changed or is unavailable. Reload the page.";
                return;
            }
            await LoadPermissionsAsync(user, company.Id);
            if (!CanView)
            {
                Error = "Your role no longer has access to Purchase Reports.";
                return;
            }
            if (!CanPrint)
            {
                DialogError = "Your role no longer has access to print Purchase Reports.";
                return;
            }
            if (!Environment.IsDevelopment())
            {
                DialogError = "Sale Summary PDF is available only in Development.";
                return;
            }
            var saleId = SelectedSale.SaleId;
            var document = await Task.Run(() => Summaries.Generate(saleId, company.Name, company.MappingId));
            ReportModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/PurchaseReports.razor.js");
            await ReportModule.InvokeVoidAsync("downloadPdf", saleId, document);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Unable to generate sale summary for {CompanyId}", CompanyId);
            DialogError = "The report could not be generated. Please try again.";
        }
        catch (SaleNotInCompanyException ex)
        {
            Logger.LogWarning(ex, "Unable to generate sale summary for {CompanyId}", CompanyId);
            DialogError = "The selected sale is no longer available for the working company.";
        }
        catch (EmptySalesSummaryException ex)
        {
            Logger.LogInformation(ex, "No sale summary data for {CompanyId}", CompanyId);
            DialogError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            Logger.LogError(ex, "Invalid sale summary data for {CompanyId}", CompanyId);
            DialogError = "The report contains invalid or inconsistent data.";
        }
        catch (JSException ex)
        {
            Logger.LogError(ex, "Unable to display sale summary for {CompanyId}", CompanyId);
            DialogError = "The report could not be displayed. Please try again.";
        }
        finally { Busy = false; }
    }

    private async Task SearchAsync()
    {
        if (Busy) return;
        Busy = true;
        Error = null;
        try
        {
            var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
            var company = await GetCompanyAsync(user);
            if (company is null || company.Id != CompanyId || company.MappingId != MappingId)
            {
                Sales = [];
                CanView = CanPrint = false;
                Error = "The working company changed or is unavailable. Reload the page.";
                return;
            }
            await LoadPermissionsAsync(user, company.Id);
            if (!CanView)
            {
                Sales = [];
                Error = "Your role no longer has access to Purchase Reports.";
                return;
            }
            var search = string.IsNullOrWhiteSpace(Search) ? "all" : Search.Trim();
            Sales = await Task.Run(() => Purchases.GetAllSales(search, company.MappingId));
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Unable to search purchase report sales for {CompanyId}", CompanyId);
            Sales = [];
            Error = "Sales could not be loaded. Please try again.";
        }
        finally { Busy = false; }
    }

    private async Task<WorkingCompany?> GetCompanyAsync(ClaimsPrincipal user)
    {
        if (!Guid.TryParse(user.FindFirst(BlendClaims.CompanyId)?.Value, out var id)) return null;
        var company = await Database.Companies.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new { c.Id, c.Name, c.CompanyNumber, c.IsEnabled, c.SubscriptionStartDate, c.SubscriptionEndDate }).SingleOrDefaultAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (company is null || !company.IsEnabled || today < company.SubscriptionStartDate || today > company.SubscriptionEndDate) return null;
        if (!user.IsInRole(BlendRoles.Admin))
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null || !await Database.UserCompanies.AsNoTracking().AnyAsync(m => m.CompanyId == id && m.UserId == userId)) return null;
        }
        return new(company.Id, company.Name, company.CompanyNumber);
    }

    private async Task LoadPermissionsAsync(ClaimsPrincipal user, Guid companyId)
    {
        CanView = CanPrint = false;
        if (user.IsInRole(BlendRoles.Admin)) { CanView = CanPrint = true; return; }
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;
        var roleIds = user.FindAll(BlendClaims.RoleId).Select(c => c.Value).Distinct(StringComparer.Ordinal).ToArray();
        var menus = await Database.MenuItems.AsNoTracking().Where(m => m.CompanyId == companyId).Select(m => new { m.Id, m.Path }).ToListAsync();
        var menuIds = menus.Where(m => string.Equals(m.Path.TrimEnd('/'), PagePath, StringComparison.OrdinalIgnoreCase)).Select(m => m.Id).ToArray();
        CanView = await Database.MenuItemRoles.AsNoTracking().AnyAsync(m => menuIds.Contains(m.MenuItemId) && roleIds.Contains(m.RoleId)
            && m.Role.CompanyId == companyId && Database.UserRoles.Any(r => r.UserId == userId && r.RoleId == m.RoleId));
        if (!CanView) return;
        var keys = await Database.RoleOperations.AsNoTracking().Where(grant => roleIds.Contains(grant.RoleId)
            && grant.Role.CompanyId == companyId && Database.UserRoles.Any(r => r.UserId == userId && r.RoleId == grant.RoleId)
            && menuIds.Contains(grant.Operation.MenuItemId)
            && Database.MenuItemRoles.Any(m => m.MenuItemId == grant.Operation.MenuItemId && m.RoleId == grant.RoleId))
            .Select(grant => grant.Operation.Key).ToListAsync();
        CanPrint = keys.Contains("print", StringComparer.OrdinalIgnoreCase);
    }

    private enum DialogKind { None, Summary, Sheet, Advanced, Broker }
    private sealed record WorkingCompany(Guid Id, string Name, int MappingId);
}

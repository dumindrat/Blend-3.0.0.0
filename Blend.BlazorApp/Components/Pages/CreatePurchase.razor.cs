using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Blend.BlazorApp.Authentication;
using Blend.BusinessService.Brokers;
using Blend.BusinessService.Purchases;
using Blend.DataService.Purchases;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Blend.BlazorApp.Components.Pages;

public partial class CreatePurchase
{
    private const string PagePath = "/Operations/Purchase";
    private IReadOnlyList<PurchaseSale> Sales = [];
    private IReadOnlyList<StraightLine> Lines = [];
    private IReadOnlyList<Broker> BrokerItems = [];
    private IReadOnlyList<string> Marks = [], GradeItems = [], Certificates = [];
    private readonly HashSet<string> SelectedCertificates = new(StringComparer.Ordinal);
    private IReadOnlyList<PurchaseCsvRow> UploadRows = [];
    private readonly List<UploadResult> UploadResults = [];
    private PurchaseSale? SelectedSale;
    private StraightLine? SelectedLine;
    private SaleInput SaleForm = new();
    private LineInput LineForm = new();
    private Guid? CompanyId;
    private int? MappingId, UploadBroker, LoadingLinesSaleId;
    private string? CompanyName, Error, PageError, Success, UploadFileName, NextDocumentNumber;
    private string Search = "", AppliedSearch = "all", UploadType = "", UploadCode = "";
    private bool Loading = true, Busy, LoadingAddLine, CanView, CanCreate, CanEdit, CanDelete, OpenAfterRender, UploadAttempted;
    private DialogKind Dialog;
    private ElementReference DialogElement;
    private IJSObjectReference? DialogModule;
    private DotNetObjectReference<CreatePurchase>? DialogCallback;
    private string SearchText => string.IsNullOrWhiteSpace(Search) ? "all" : Search.Trim();
    private string DialogTitle => Dialog switch { DialogKind.Sale => SelectedSale is null ? "Create sale" : "Edit sale", DialogKind.Status => "Confirm sale status", DialogKind.Lines => "Straight lines", DialogKind.Line => SelectedLine is null ? "Add straight line" : "Edit straight line", DialogKind.Delete => "Confirm deletion", DialogKind.Upload or DialogKind.UploadConfirm => "Upload purchase CSV", _ => "Purchase" };
    private static string FormatGridAmount(decimal? value) => value?.ToString("N2", CultureInfo.InvariantCulture) ?? string.Empty;
    private decimal? PreviewWeight
    {
        get { try { return LineForm.Net1 * LineForm.Pack1 + LineForm.Net2 * LineForm.Pack2 - (LineForm.ReducedQty ?? 0); } catch (OverflowException) { return null; } }
    }
    private decimal? PreviewValue
    {
        get { try { return PreviewWeight * (LineForm.Rate ?? 0); } catch (OverflowException) { return null; } }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
            var company = await GetCompanyAsync(user);
            if (company is null) return;
            CompanyId = company.Id; MappingId = company.MappingId; CompanyName = company.Name;
            await LoadPermissionsAsync(user, company.Id);
            if (CanView) Sales = Purchases.GetAllSales("all", company.MappingId);
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to load purchase sales for {CompanyId}", CompanyId); PageError = "Sales could not be loaded. Reload the page and try again."; }
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
    }

    private async Task<WorkingCompany?> GetCompanyAsync(ClaimsPrincipal user)
    {
        if (!Guid.TryParse(user.FindFirst(BlendClaims.CompanyId)?.Value, out var id)) return null;
        var company = await Database.Companies.AsNoTracking().Where(c => c.Id == id).Select(c => new { c.Id, c.Name, c.CompanyNumber, c.IsEnabled, c.SubscriptionStartDate, c.SubscriptionEndDate }).SingleOrDefaultAsync();
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
        if (user.IsInRole(BlendRoles.Admin)) { CanView = CanCreate = CanEdit = CanDelete = true; return; }
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleIds = user.FindAll(BlendClaims.RoleId).Select(c => c.Value).Distinct(StringComparer.Ordinal).ToArray();
        var menus = await Database.MenuItems.AsNoTracking().Where(m => m.CompanyId == companyId).Select(m => new { m.Id, m.Path }).ToListAsync();
        var ids = menus.Where(m => string.Equals(m.Path.TrimEnd('/'), PagePath, StringComparison.OrdinalIgnoreCase)).Select(m => m.Id).ToArray();
        CanView = userId is not null && await Database.MenuItemRoles.AsNoTracking().AnyAsync(m => ids.Contains(m.MenuItemId) && roleIds.Contains(m.RoleId) && m.Role.CompanyId == companyId && Database.UserRoles.Any(r => r.UserId == userId && r.RoleId == m.RoleId));
        var keys = await Database.RoleOperations.AsNoTracking().Where(a => userId != null && roleIds.Contains(a.RoleId) && a.Role.CompanyId == companyId && Database.UserRoles.Any(r => r.UserId == userId && r.RoleId == a.RoleId) && ids.Contains(a.Operation.MenuItemId) && Database.MenuItemRoles.Any(m => m.MenuItemId == a.Operation.MenuItemId && m.RoleId == a.RoleId)).Select(a => a.Operation.Key).ToListAsync();
        CanCreate = keys.Contains("create", StringComparer.OrdinalIgnoreCase); CanEdit = keys.Contains("edit", StringComparer.OrdinalIgnoreCase); CanDelete = keys.Contains("delete", StringComparer.OrdinalIgnoreCase);
    }

    private async Task<WorkingCompany?> CheckContextAsync(string? operation = null)
    {
        var user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
        var company = await GetCompanyAsync(user);
        if (company is null || company.Id != CompanyId || company.MappingId != MappingId) { Error = "The working company changed or is unavailable. Reload the page."; return null; }
        await LoadPermissionsAsync(user, company.Id);
        if (!CanView) { Error = "Your role no longer has access to Create Purchase."; return null; }
        if (operation is not null && !(operation switch { "create" => CanCreate, "edit" => CanEdit, "delete" => CanDelete, _ => false })) { Error = "Your role is not permitted to perform this operation."; return null; }
        return company;
    }

    private PurchaseSale? CurrentSale(int companyId) => SelectedSale is null ? null : Purchases.GetAllSales(AppliedSearch, companyId).FirstOrDefault(s => s.SaleId == SelectedSale.SaleId);
    private bool CheckSale(int companyId, bool requireOpen)
    {
        var sale = CurrentSale(companyId);
        if (SelectedSale is null || sale is null || sale.SaleDate != SelectedSale.SaleDate || sale.IsClosed != SelectedSale.IsClosed || (requireOpen && sale.IsClosed))
        { Error = "The sale changed or is no longer available. Refresh and reopen it."; return false; }
        return true;
    }
    private async Task<string?> GetUserAsync()
    {
        var name = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(name) || name.Length > 45) { Error = "Your account name cannot be recorded by the purchase database."; return null; }
        return name;
    }
    private void OpenDialog(DialogKind kind) { Dialog = kind; Error = null; Success = null; OpenAfterRender = true; }
    private async Task CloseDialogAsync()
    {
        if (Busy) return;
        if (DialogModule is not null) await DialogModule.InvokeVoidAsync("close", DialogElement);
        Dialog = DialogKind.None; SelectedSale = null; SelectedLine = null; Error = null;
    }
    private void BackToLines() { if (Busy) return; Dialog = DialogKind.Lines; SelectedLine = null; Error = null; }
    private void BeginCreate() { if (Busy || !CanCreate) return; SelectedSale = null; SaleForm = new(); OpenDialog(DialogKind.Sale); }
    private void BeginEdit(PurchaseSale sale)
    {
        if (Busy || !CanEdit || sale.IsClosed) return;
        if (!DateTime.TryParse(sale.SaleDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) { Error = "The sale date is invalid."; return; }
        SelectedSale = sale; SaleForm = new() { Date = date }; OpenDialog(DialogKind.Sale);
    }
    private void BeginStatus(PurchaseSale sale) { if (Busy || !CanEdit) return; SelectedSale = sale; OpenDialog(DialogKind.Status); }
    private async Task SearchAsync()
    {
        if (Busy) return;
        try { var company = await CheckContextAsync(); if (company is null) return; var search = SearchText; Sales = Purchases.GetAllSales(search, company.MappingId); AppliedSearch = search; Error = null; }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to search purchase sales"); Error = "Sales could not be loaded."; }
    }
    private async Task SaveSaleAsync()
    {
        if (Busy || Dialog != DialogKind.Sale) return;
        Busy = true; Error = null;
        try
        {
            var company = await CheckContextAsync(SelectedSale is null ? "create" : "edit"); if (company is null) return;
            var user = await GetUserAsync(); if (user is null) return;
            if (SelectedSale is not null && !CheckSale(company.MappingId, true)) return;
            var sale = new PurchaseSale { SaleId = SelectedSale?.SaleId ?? 0, CompanyId = company.MappingId, SaleDate = SaleForm.Date!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), User = user };
            var result = SelectedSale is null ? Purchases.CreateSale(sale) : Purchases.UpdateSale(sale);
            if (!result.Succeeded) { Error = result.ErrorMessage; return; }
            await FinishSaleWriteAsync(company.MappingId, "Sale saved.");
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to save purchase sale"); Error = "The sale save could not be confirmed. Refresh and check the sale before retrying."; }
        finally { Busy = false; }
    }
    private async Task ChangeStatusAsync()
    {
        if (Busy || Dialog != DialogKind.Status || SelectedSale is null) return;
        Busy = true; Error = null;
        try
        {
            var company = await CheckContextAsync("edit"); if (company is null || !CheckSale(company.MappingId, false)) return;
            var user = await GetUserAsync(); if (user is null) return;
            var sale = new PurchaseSale { SaleId = SelectedSale.SaleId, CompanyId = company.MappingId, User = user };
            var result = SelectedSale.IsClosed ? Purchases.OpenSale(sale) : Purchases.CloseSale(sale);
            if (!result.Succeeded) { Error = result.ErrorMessage; return; }
            await FinishSaleWriteAsync(company.MappingId, "Sale status updated.");
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to change sale status"); Error = "The status change could not be confirmed. Refresh and check the sale before retrying."; }
        finally { Busy = false; }
    }
    private async Task FinishSaleWriteAsync(int mappingId, string message)
    {
        if (DialogModule is not null) await DialogModule.InvokeVoidAsync("close", DialogElement);
        Dialog = DialogKind.None; SelectedSale = null; Success = message;
        try { Sales = Purchases.GetAllSales(AppliedSearch, mappingId); }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to refresh sales"); Error = "The change was saved, but sales could not be refreshed."; }
    }
    private async Task OpenLinesAsync(PurchaseSale sale)
    {
        if (Busy) return;
        Busy = true; LoadingLinesSaleId = sale.SaleId; SelectedSale = sale; Error = null;
        try
        {
            var company = await CheckContextAsync();
            if (company is null) return;
            var search = AppliedSearch;
            var current = await Task.Run(() => Purchases.GetAllSales(search, company.MappingId).FirstOrDefault(s => s.SaleId == sale.SaleId));
            if (current is null || current.SaleDate != sale.SaleDate || current.IsClosed != sale.IsClosed)
            { Error = "The sale changed or is no longer available. Refresh and reopen it."; return; }
            Lines = await Task.Run(() => Purchases.GetAddedStraightLines(sale.SaleId));
            OpenDialog(DialogKind.Lines);
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to load purchase lines"); Error = "Purchase lines could not be loaded."; }
        finally { LoadingLinesSaleId = null; Busy = false; }
    }
    private PurchaseLookups GetLookups(int mappingId) => new(
        Brokers.GetAllBrokers("all", mappingId),
        GardenMarks.GetGardenMarksList().Select(m => m.GardenMarkName).Where(m => !string.IsNullOrEmpty(m)).Select(m => m!).Distinct().ToArray(),
        Grades.GetGradeList().Select(g => g.GradeName).Where(g => !string.IsNullOrEmpty(g)).Select(g => g!).Distinct().ToArray(),
        Purchases.GetTeaCertificates().Select(c => c.CertifiedName).Where(c => !string.IsNullOrEmpty(c)).Select(c => c!).Distinct().ToArray());

    private void ApplyLookups(PurchaseLookups lookups)
    {
        BrokerItems = lookups.Brokers; Marks = lookups.Marks; GradeItems = lookups.Grades; Certificates = lookups.Certificates;
    }
    private async Task BeginAddLineAsync()
    {
        if (Busy || !CanCreate || SelectedSale is null) return;
        var selectedSale = SelectedSale;
        Busy = true; LoadingAddLine = true; NextDocumentNumber = null; Error = null;
        try
        {
            var company = await CheckContextAsync("create"); if (company is null) return;
            var search = AppliedSearch;
            var sale = await Task.Run(() => Purchases.GetAllSales(search, company.MappingId).FirstOrDefault(s => s.SaleId == selectedSale.SaleId));
            if (sale is null || sale.SaleDate != selectedSale.SaleDate || sale.IsClosed != selectedSale.IsClosed || sale.IsClosed)
            { Error = "The sale changed or is no longer available. Refresh and reopen it."; return; }
            string number;
            try
            {
                var formatted = await Task.Run(() => Documents.GetDocumentNo(1, company.MappingId).FormattedDocNo);
                if (string.IsNullOrWhiteSpace(formatted) || formatted.Length > 10) throw new InvalidOperationException("The formatted purchase document number must contain at most 10 characters.");
                number = formatted;
            }
            catch (InvalidOperationException ex) { Logger.LogError(ex, "Unable to obtain purchase document number for company {CompanyId}", company.MappingId); Error = "The purchase document number is unavailable or invalid. Check its configuration and try again."; return; }
            var lookups = await Task.Run(() => GetLookups(company.MappingId));
            ApplyLookups(lookups); NextDocumentNumber = number; SelectedLine = null; LineForm = new(); SelectedCertificates.Clear(); Dialog = DialogKind.Line;
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to load purchase document number or lookups"); Error = "The purchase document number or lookups could not be loaded. Try again."; }
        finally { LoadingAddLine = false; Busy = false; }
    }
    private async Task BeginEditLineAsync(StraightLine row)
    {
        if (Busy || !CanEdit || SelectedSale is null) return;
        try
        {
            var company = await CheckContextAsync("edit"); if (company is null || !CheckSale(company.MappingId, true)) return;
            if (!Purchases.GetAddedStraightLines(SelectedSale.SaleId).Any(l => l.Id == row.Id && !l.IsRepeated)) { Error = "The line is no longer available."; return; }
            var line = Purchases.GetStraightLineByReservedNo(row.Id);
            if (line is null || line.SaleId != SelectedSale.SaleId) { Error = "The line is no longer available."; return; }
            ApplyLookups(GetLookups(company.MappingId)); SelectedLine = line; LineForm = LineInput.FromLine(line); SelectedCertificates.Clear();
            foreach (var value in (line.TeaCertificates ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) SelectedCertificates.Add(value);
            Certificates = Certificates.Concat(SelectedCertificates).Distinct().ToArray(); Dialog = DialogKind.Line; Error = null;
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to load purchase line"); Error = "The purchase line could not be loaded."; }
        catch (InvalidOperationException ex) { Logger.LogWarning(ex, "Invalid purchase line group"); Error = "The purchase line cannot be edited; reload the sale."; }
    }
    private void SetCertificate(string value, bool selected) { if (selected) SelectedCertificates.Add(value); else SelectedCertificates.Remove(value); }
    private async Task SaveLineAsync()
    {
        if (Busy || Dialog != DialogKind.Line || SelectedSale is null) return;
        Busy = true; Error = null;
        try
        {
            var company = await CheckContextAsync(SelectedLine is null ? "create" : "edit"); if (company is null || !CheckSale(company.MappingId, true)) return;
            var user = await GetUserAsync(); if (user is null) return;
            if (!Brokers.GetAllBrokers("all", company.MappingId).Any(b => b.BrokerId == LineForm.BrokerId)) { Error = "The selected broker is no longer available for this company."; return; }
            if (!ValidateLine()) return;
            var line = LineForm.ToLine(SelectedLine); line.CompanyId = company.MappingId; line.SaleId = SelectedSale.SaleId; line.User = user; line.TeaCertificates = string.Join(",", Certificates.Where(SelectedCertificates.Contains));
            if (line.TeaCertificates.Length > 100) { Error = "Selected certifications exceed 100 characters."; return; }
            var result = SelectedLine is null ? Purchases.AddStraightLine(line) : Purchases.UpdateStraightLine(line);
            if (!result.Succeeded) { Error = result.ErrorMessage; return; }
            Dialog = DialogKind.Lines; SelectedLine = null;
            RefreshLinesAfterWrite("Line saved.");
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to save purchase line"); Error = "The line save could not be confirmed. Reopen the sale and check before retrying."; }
        catch (ArgumentException ex) { Logger.LogWarning(ex, "Invalid purchase line"); Error = ex.Message; }
        catch (InvalidOperationException ex) { Logger.LogWarning(ex, "Purchase line could not be saved"); Error = ex.Message; }
        finally { Busy = false; }
    }
    private bool ValidateLine()
    {
        if (!new[] { "MS", "PS", "OL" }.Contains(LineForm.SaleType)) { Error = "Select a valid sale type."; return false; }
        var values = new[] { LineForm.Net1, LineForm.Pack1, LineForm.Net2, LineForm.Pack2, LineForm.ReducedQty ?? 0, LineForm.Rate ?? 0 };
        if (values.Any(v => v < 0 || v > 9999999999999999.99m || decimal.Round(v, 2) != v)) { Error = "Amounts must be non-negative with at most two decimal places."; return false; }
        try
        {
            var repeated = LineForm.Net2 > 0 && LineForm.Pack2 > 0;
            var first = LineForm.Net1 * LineForm.Pack1 - (repeated ? 0 : LineForm.ReducedQty ?? 0);
            var second = repeated ? LineForm.Net2 * LineForm.Pack2 - (LineForm.ReducedQty ?? 0) : 0;
            var total = first + second;
            var rate = LineForm.Rate ?? 0;
            var calculated = new[] { first, second, total, first * rate, second * rate, total * rate };
            if (calculated.Any(v => v < 0 || v > 9999999999999999.99m || decimal.Round(v, 2) != v)) { Error = "Calculated weight or value is invalid or has more than two decimal places."; return false; }
        }
        catch (OverflowException) { Error = "Calculated weight or value is too large."; return false; }
        return true;
    }
    private void BeginDeleteLine(StraightLine line) { if (Busy || !CanDelete) return; SelectedLine = line; Error = null; Dialog = DialogKind.Delete; }
    private async Task DeleteLineAsync()
    {
        if (Busy || Dialog != DialogKind.Delete || SelectedSale is null || SelectedLine is null) return;
        Busy = true; Error = null;
        try
        {
            var company = await CheckContextAsync("delete"); if (company is null || !CheckSale(company.MappingId, true)) return;
            var user = await GetUserAsync(); if (user is null) return;
            if (!Purchases.GetAddedStraightLines(SelectedSale.SaleId).Any(l => l.Id == SelectedLine.Id)) { Error = "The line is no longer available."; return; }
            var result = Purchases.DeleteStraightLine(new StraightLine { Id = SelectedLine.Id, User = user });
            if (!result.Succeeded) { Error = result.ErrorMessage; return; }
            Dialog = DialogKind.Lines; SelectedLine = null; RefreshLinesAfterWrite("Line deleted.");
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to delete purchase line"); Error = "The deletion could not be confirmed. Reopen the sale and check before retrying."; }
        catch (InvalidOperationException ex) { Logger.LogWarning(ex, "Purchase line deletion rejected"); Error = ex.Message; }
        finally { Busy = false; }
    }
    private void RefreshLinesAfterWrite(string message)
    {
        Success = message;
        try { Lines = Purchases.GetAddedStraightLines(SelectedSale!.SaleId); }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to refresh purchase lines"); Lines = []; Error = "The change was saved, but lines could not be refreshed. Close and reopen the sale."; }
    }

    private async Task BeginUploadAsync()
    {
        if (Busy || !CanCreate || SelectedSale is null) return;
        try
        {
            var company = await CheckContextAsync("create"); if (company is null || !CheckSale(company.MappingId, true)) return;
            BrokerItems = Brokers.GetAllBrokers("all", company.MappingId); UploadRows = []; UploadResults.Clear(); UploadBroker = null; UploadType = UploadCode = ""; UploadFileName = null; UploadAttempted = false; Error = null; Dialog = DialogKind.Upload;
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to load brokers for purchase upload"); Error = "Brokers could not be loaded."; }
    }
    private async Task ReadFileAsync(InputFileChangeEventArgs args)
    {
        if (Busy || UploadAttempted) return;
        Busy = true;
        UploadRows = []; UploadResults.Clear(); UploadFileName = null; Error = null;
        try
        {
            var file = args.File;
            if (!file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) || file.Name.Length > 255) throw new InvalidDataException("Choose a CSV file with a name of at most 255 characters.");
            await using var stream = file.OpenReadStream(5 * 1024 * 1024);
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;
            UploadRows = PurchaseCsvReader.Read(buffer);
            UploadFileName = Path.GetFileName(file.Name);
        }
        catch (InvalidDataException ex) { Error = ex.Message; }
        catch (IOException ex) { Logger.LogWarning(ex, "Unable to read purchase CSV"); Error = "The file could not be read or exceeds 5 MB."; }
        finally { Busy = false; }
    }
    private void ConfirmUpload()
    {
        if (Busy || UploadAttempted || UploadRows.Count == 0) return;
        if (UploadBroker is null or <= 0 || !new[] { "MS", "PS", "OL" }.Contains(UploadType) || string.IsNullOrWhiteSpace(UploadCode) || UploadCode.Trim().Length > 3)
        { Error = "Select a broker and sale type and enter a sale code of up to three characters."; return; }
        Error = null; Dialog = DialogKind.UploadConfirm;
    }
    private async Task UploadAsync()
    {
        if (Busy || Dialog != DialogKind.UploadConfirm || SelectedSale is null || UploadAttempted || UploadBroker is null || UploadFileName is null) return;
        Busy = true; Error = null;
        try
        {
            var company = await CheckContextAsync("create"); if (company is null || !CheckSale(company.MappingId, true)) return;
            var user = await GetUserAsync(); if (user is null) return;
            if (!Brokers.GetAllBrokers("all", company.MappingId).Any(b => b.BrokerId == UploadBroker)) { Error = "The selected broker is no longer available for this company."; return; }
            UploadAttempted = true;
            foreach (var row in UploadRows)
            {
                try
                {
                    var current = await CheckContextAsync("create");
                    if (current is null || !CheckSale(company.MappingId, true)) break;
                }
                catch (SqlException ex) { Logger.LogError(ex, "Unable to recheck purchase import permissions or sale"); Error = "Import stopped because the sale or permissions could not be rechecked."; break; }
                catch (InvalidOperationException ex) { Logger.LogError(ex, "Unable to recheck purchase import permissions or sale"); Error = "Import stopped because the sale or permissions could not be rechecked."; break; }
                try
                {
                    var line = new StraightLine { CompanyId = company.MappingId, SaleId = SelectedSale.SaleId, BrokerId = UploadBroker, User = user, SaleType = UploadType, SaleCode = UploadCode.Trim(), LotNo = row.Lot, Invoice = row.Invoice, GardenMark = row.Mark, Grade = row.Grade, Standard = "", TeaCertificates = "", Pack1 = row.Packs, Net1 = row.Net, ReducedQty = row.Reduction, Rate = row.Rate };
                    var result = Purchases.AddStraightLine(line);
                    UploadResults.Add(new(row.RowNumber, row.Lot, result.Succeeded ? "Saved" : result.ErrorMessage ?? "Failed"));
                }
                catch (SqlException ex) { Logger.LogError(ex, "Purchase upload failed for row {Row}", row.RowNumber); UploadResults.Add(new(row.RowNumber, row.Lot, "Unknown: database error; check the sale before retrying.")); }
                catch (ArgumentException ex) { Logger.LogWarning(ex, "Invalid purchase upload row {Row}", row.RowNumber); UploadResults.Add(new(row.RowNumber, row.Lot, "Failed: invalid row.")); }
                catch (InvalidOperationException ex) { Logger.LogWarning(ex, "Purchase upload rejected for row {Row}", row.RowNumber); UploadResults.Add(new(row.RowNumber, row.Lot, "Failed: purchase could not be saved.")); }
            }
            foreach (var row in UploadRows.Skip(UploadResults.Count)) UploadResults.Add(new(row.RowNumber, row.Lot, "Not attempted"));
            var completed = UploadResults.Count(r => r.Status == "Saved");
            try
            {
                UploadLog.Insert(new PurchaseUploadLog { FileName = UploadFileName, SaleId = SelectedSale.SaleId, BrokerId = UploadBroker.Value, SaleCode = UploadCode.Trim(), SaleType = UploadType, TotalRecords = UploadRows.Count, ErrorRecords = UploadRows.Count - completed, CreateUser = user, UploadData = JsonSerializer.Serialize(UploadRows.Select((row, i) => new { LotNo = row.Lot, InvNo = row.Invoice, row.Mark, row.Grade, Net = row.Net.ToString(CultureInfo.InvariantCulture), Bag = row.Packs.ToString(CultureInfo.InvariantCulture), Qty = row.Quantity.ToString(CultureInfo.InvariantCulture), Price = (row.Quantity * row.Rate).ToString(CultureInfo.InvariantCulture), ReducedQty = row.Reduction.ToString(CultureInfo.InvariantCulture), row.RowNumber, row.Rate, UploadResults[i].Status })) });
            }
            catch (SqlException ex) { Logger.LogError(ex, "Unable to record purchase upload log"); Error = $"{Error} The upload audit log could not be confirmed. Do not retry without checking the sale.".Trim(); }
            catch (InvalidOperationException ex) { Logger.LogError(ex, "Unable to record purchase upload log"); Error = $"{Error} The upload audit log could not be confirmed. Do not retry without checking the sale.".Trim(); }
            Dialog = DialogKind.Upload;
            Success = $"Import complete: {completed} saved, {UploadRows.Count - completed} not confirmed. Check the results and sale before uploading again.";
            try { Lines = Purchases.GetAddedStraightLines(SelectedSale.SaleId); }
            catch (SqlException ex) { Logger.LogError(ex, "Unable to refresh purchase lines after upload"); Error = $"{Error} The line list could not be refreshed. Reopen the sale before retrying.".Trim(); }
        }
        catch (SqlException ex) { Logger.LogError(ex, "Unable to start purchase upload"); Error = "The import could not be started."; Dialog = DialogKind.Upload; }
        finally { Busy = false; }
    }

    private sealed record WorkingCompany(Guid Id, string Name, int MappingId);
    private sealed record PurchaseLookups(IReadOnlyList<Broker> Brokers, IReadOnlyList<string> Marks, IReadOnlyList<string> Grades, IReadOnlyList<string> Certificates);
    private sealed record UploadResult(long RowNumber, string Lot, string Status);
    private enum DialogKind { None, Sale, Status, Lines, Line, Delete, Upload, UploadConfirm }
    private sealed class SaleInput { [Required] public DateTime? Date { get; set; } = DateTime.Today; }
    private sealed class LineInput
    {
        [Required, StringLength(2)] public string SaleType { get; set; } = "";
        [Required, StringLength(3)] public string SaleCode { get; set; } = "";
        [Required, StringLength(10)] public string LotNo { get; set; } = "";
        [Required, StringLength(10)] public string Invoice { get; set; } = "";
        [Required, Range(1, int.MaxValue)] public int? BrokerId { get; set; }
        [Required, StringLength(45)] public string GardenMark { get; set; } = "";
        [Required, StringLength(45)] public string Grade { get; set; } = "";
        [StringLength(45)] public string Standard { get; set; } = "";
        public decimal Net1 { get; set; }
        public decimal Pack1 { get; set; }
        public decimal Net2 { get; set; }
        public decimal Pack2 { get; set; }
        [Required] public decimal? ReducedQty { get; set; } = 0;
        [Required] public decimal? Rate { get; set; } = 0;
        public static LineInput FromLine(StraightLine line) => new() { SaleType = line.SaleType ?? "", SaleCode = line.SaleCode ?? "", LotNo = line.LotNo ?? "", Invoice = line.Invoice ?? "", BrokerId = line.BrokerId, GardenMark = line.GardenMark ?? "", Grade = line.Grade ?? "", Standard = line.Standard ?? "", Net1 = line.Net1, Pack1 = line.Pack1, Net2 = line.Net2, Pack2 = line.Pack2, ReducedQty = line.ReducedQty, Rate = line.Rate };
        public StraightLine ToLine(StraightLine? original) => new() { Id = original?.Id ?? 0, StockId = original?.StockId ?? 0, RepeatingId = original?.RepeatingId ?? 0, RepeatingStockId = original?.RepeatingStockId ?? 0, RowVersion1 = original?.RowVersion1, RowVersion2 = original?.RowVersion2, SaleType = SaleType.Trim(), SaleCode = SaleCode.Trim(), LotNo = LotNo.Trim(), Invoice = Invoice.Trim(), BrokerId = BrokerId, GardenMark = GardenMark.Trim(), Grade = Grade.Trim(), Standard = Standard.Trim(), Net1 = Net1, Pack1 = Pack1, Net2 = Net2, Pack2 = Pack2, ReducedQty = ReducedQty, Rate = Rate };
    }
}

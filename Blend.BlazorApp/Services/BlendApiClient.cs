using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blend.BlazorApp.Models;

namespace Blend.BlazorApp.Services;

public sealed class BlendApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly BlendSession _session;
    private readonly string _accessLink;

    public BlendApiClient(HttpClient httpClient, IConfiguration configuration, BlendSession session)
    {
        _httpClient = httpClient;
        _session = session;
        _accessLink = configuration["BlendApi:AccessLink"] ?? string.Empty;

        var baseUrl = configuration["BlendApi:BaseUrl"];
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseAddress))
        {
            _httpClient.BaseAddress = baseAddress;
        }
    }

    public async Task SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/User");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authorization);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var token = response.Headers.TryGetValues("Token", out var tokenValues)
            ? tokenValues.FirstOrDefault()
            : null;
        var authenticatedUser = response.Headers.TryGetValues("SysUser", out var userValues)
            ? userValues.FirstOrDefault()
            : userName;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("The Blend API did not return an authentication token.");
        }

        _session.SignIn(authenticatedUser ?? userName, authorization, token);
    }

    public Task<List<CompanyChoice>> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var userName = Uri.EscapeDataString(_session.UserName.Replace('/', '~'));
        return GetListAsync<CompanyChoice>($"api/Company/GetUserCompanies/{userName}", cancellationToken, requireCompany: false);
    }

    public Task<List<BlendIssueNote>> GetIssueNotesAsync(string search, CancellationToken cancellationToken = default)
    {
        var query = Uri.EscapeDataString(string.IsNullOrWhiteSpace(search) ? "all" : search.Trim());
        return GetListAsync<BlendIssueNote>($"api/ItemIssue/GetAllIssueNotes/{query}", cancellationToken);
    }

    public Task<List<BlendChoice>> GetBlendsAsync(CancellationToken cancellationToken = default) =>
        GetListAsync<BlendChoice>("api/ItemIssue/GetBlendList/all", cancellationToken);

    public Task<List<IssueNoteItem>> GetIssueItemsAsync(int issueNoteId, CancellationToken cancellationToken = default) =>
        GetListAsync<IssueNoteItem>($"api/ItemIssue/GetIssueNoteItemsById/{issueNoteId}", cancellationToken);

    public async Task<string> GetNextIssueNoteNumberAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateApiRequest(HttpMethod.Get, "api/Document/GetDocumentNo/5");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<GeneratedDocumentNumber>(JsonOptions, cancellationToken);
        return string.IsNullOrWhiteSpace(result?.FormattedDocNo)
            ? throw new InvalidOperationException("The Blend API returned an empty issue-note number.")
            : result.FormattedDocNo;
    }

    public async Task SaveIssueNoteAsync(BlendIssueNote note, bool isNew, CancellationToken cancellationToken = default)
    {
        var method = isNew ? HttpMethod.Post : HttpMethod.Put;
        using var request = CreateApiRequest(method, "api/ItemIssue");
        request.Content = JsonContent.Create(note, options: JsonOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteIssueNoteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var request = CreateApiRequest(HttpMethod.Delete, $"api/ItemIssue/{id}");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var certificate = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_session.UserName}:xyz#"));
        using var request = new HttpRequestMessage(HttpMethod.Put, "api/User")
        {
            Content = JsonContent.Create(new LogoutRequest
            {
                UserName = _session.UserName,
                CertId = certificate,
                Token = _session.Token
            }, options: JsonOptions)
        };
        request.Headers.Authorization = ParseAuthorization(_session.Authorization);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<List<T>> GetListAsync<T>(string path, CancellationToken cancellationToken, bool requireCompany = true)
    {
        using var request = CreateApiRequest(HttpMethod.Get, path, requireCompany);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The Blend API returned an empty response instead of a list.");
    }

    private HttpRequestMessage CreateApiRequest(HttpMethod method, string path, bool requireCompany = true)
    {
        EnsureConfigured();
        if (string.IsNullOrWhiteSpace(_accessLink))
        {
            throw new InvalidOperationException("Configure BlendApi:AccessLink with the item-issue access link from the existing Blend menu.");
        }
        if (!_session.IsAuthenticated)
        {
            throw new InvalidOperationException("Sign in before requesting Blend data.");
        }

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = ParseAuthorization(_session.Authorization);
        request.Headers.Add("Token", _session.Token);
        request.Headers.Add("SysUser", _session.UserName);
        request.Headers.Add("AccessLink", _accessLink);
        if (requireCompany)
        {
            if (_session.Company is null)
            {
                request.Dispose();
                throw new InvalidOperationException("Select a company before requesting Blend data.");
            }
            request.Headers.Add("SysCompany", _session.Company.CompanyId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        return request;
    }

    private void EnsureConfigured()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configure BlendApi:BaseUrl in appsettings.json with the existing Blend API URL.");
        }
    }

    private static AuthenticationHeaderValue ParseAuthorization(string authorization) =>
        new("Basic", authorization);

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(details) ? response.ReasonPhrase : details;
        throw new HttpRequestException(
            $"Blend API request failed ({(int)response.StatusCode} {response.StatusCode}): {message}",
            null,
            response.StatusCode);
    }
}

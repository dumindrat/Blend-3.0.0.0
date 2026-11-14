using Blend.BlazorApp.Models;

namespace Blend.BlazorApp.Services;

public sealed class BlendApplicationService : IBlendApplicationService
{
    private readonly BlendApiClient _apiClient;

    public BlendApplicationService(BlendApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task SignInAsync(string userName, string password, CancellationToken cancellationToken = default) =>
        _apiClient.SignInAsync(userName, password, cancellationToken);

    public Task<List<CompanyChoice>> GetCompaniesAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetCompaniesAsync(cancellationToken);

    public Task<List<BlendIssueNote>> GetIssueNotesAsync(string search, CancellationToken cancellationToken = default) =>
        _apiClient.GetIssueNotesAsync(search, cancellationToken);

    public Task<List<BlendChoice>> GetBlendsAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetBlendsAsync(cancellationToken);

    public Task<List<IssueNoteItem>> GetIssueItemsAsync(int issueNoteId, CancellationToken cancellationToken = default) =>
        _apiClient.GetIssueItemsAsync(issueNoteId, cancellationToken);

    public Task<string> GetNextIssueNoteNumberAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetNextIssueNoteNumberAsync(cancellationToken);

    public Task SaveIssueNoteAsync(BlendIssueNote note, bool isNew, CancellationToken cancellationToken = default) =>
        _apiClient.SaveIssueNoteAsync(note, isNew, cancellationToken);

    public Task DeleteIssueNoteAsync(int id, CancellationToken cancellationToken = default) =>
        _apiClient.DeleteIssueNoteAsync(id, cancellationToken);

    public Task SignOutAsync(CancellationToken cancellationToken = default) =>
        _apiClient.SignOutAsync(cancellationToken);
}

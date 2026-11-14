using Blend.BlazorApp.Models;

namespace Blend.BlazorApp.Services;

public interface IBlendApplicationService
{
    Task SignInAsync(string userName, string password, CancellationToken cancellationToken = default);
    Task<List<CompanyChoice>> GetCompaniesAsync(CancellationToken cancellationToken = default);
    Task<List<BlendIssueNote>> GetIssueNotesAsync(string search, CancellationToken cancellationToken = default);
    Task<List<BlendChoice>> GetBlendsAsync(CancellationToken cancellationToken = default);
    Task<List<IssueNoteItem>> GetIssueItemsAsync(int issueNoteId, CancellationToken cancellationToken = default);
    Task<string> GetNextIssueNoteNumberAsync(CancellationToken cancellationToken = default);
    Task SaveIssueNoteAsync(BlendIssueNote note, bool isNew, CancellationToken cancellationToken = default);
    Task DeleteIssueNoteAsync(int id, CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
}

using Blend.BlazorApp.Models;

namespace Blend.BlazorApp.Services;

public sealed class BlendSession
{
    public string UserName { get; private set; } = string.Empty;
    public string Authorization { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public CompanyChoice? Company { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void SignIn(string userName, string authorization, string token)
    {
        UserName = userName;
        Authorization = authorization;
        Token = token;
        Company = null;
    }

    public void SelectCompany(CompanyChoice company) => Company = company;

    public void Clear()
    {
        UserName = string.Empty;
        Authorization = string.Empty;
        Token = string.Empty;
        Company = null;
    }
}

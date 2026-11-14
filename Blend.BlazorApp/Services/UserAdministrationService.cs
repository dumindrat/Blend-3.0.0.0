using System.Security.Cryptography;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Services;

public sealed class UserAdministrationService : IUserAdministrationService
{
    private readonly UserManager<BlendIdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserAdministrationService(
        UserManager<BlendIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserAdministrationResult> CreateUserAsync(
        string userName,
        string displayName,
        string? email,
        string role,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        userName = userName.Trim();
        displayName = displayName.Trim();
        email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        if (!await _roleManager.RoleExistsAsync(role))
        {
            return Failure("Choose a valid role.");
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(displayName))
        {
            return Failure("Username and display name are required.");
        }

        var user = new BlendIdentityUser
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            MustChangePassword = true
        };
        var temporaryPassword = GenerateTemporaryPassword();
        var createResult = await _userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            return new UserAdministrationResult
            {
                Errors = createResult.Errors.Select(error => error.Description).ToArray()
            };
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            var deleteErrors = await DeleteCreatedUserAsync(user);
            return new UserAdministrationResult
            {
                Errors = addRoleResult.Errors.Select(error => error.Description).Concat(deleteErrors).ToArray()
            };
        }

        return new UserAdministrationResult
        {
            Succeeded = true,
            TemporaryPassword = temporaryPassword
        };
    }

    private async Task<IReadOnlyList<string>> DeleteCreatedUserAsync(BlendIdentityUser user)
    {
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? []
            : result.Errors.Select(error => $"Account cleanup failed: {error.Description}").ToArray();
    }

    private static UserAdministrationResult Failure(string message) => new()
    {
        Errors = [message]
    };

    private static string GenerateTemporaryPassword()
    {
        var random = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return $"{random}aA1!";
    }
}

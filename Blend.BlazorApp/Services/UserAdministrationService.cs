using System.Security.Cryptography;
using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blend.BlazorApp.Services;

public sealed class UserAdministrationService : IUserAdministrationService
{
    private readonly UserManager<BlendIdentityUser> _userManager;
    private readonly RoleManager<BlendIdentityRole> _roleManager;
    private readonly BlendIdentityDbContext _database;

    public UserAdministrationService(
        UserManager<BlendIdentityUser> userManager,
        RoleManager<BlendIdentityRole> roleManager,
        BlendIdentityDbContext database)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _database = database;
    }

    public async Task<UserAdministrationResult> CreateUserAsync(
        string userName,
        string displayName,
        string? email,
        string roleId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        userName = userName.Trim();
        displayName = displayName?.Trim() ?? string.Empty;
        email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role is null || role.CompanyId != companyId || string.IsNullOrWhiteSpace(role.Name))
        {
            return Failure("Choose a valid role for the selected company.");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            return Failure("Username is required.");
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user is not null)
        {
            if (await _userManager.IsInRoleAsync(user, BlendRoles.Admin))
            {
                return Failure("The platform administrator cannot be added to a company role.");
            }

            if (await _database.UserCompanies.AnyAsync(
                membership => membership.UserId == user.Id && membership.CompanyId == companyId,
                cancellationToken))
            {
                return Failure("This user already has access to the selected company.");
            }

            _database.UserCompanies.Add(new BlendUserCompany { UserId = user.Id, CompanyId = companyId });
            await _database.SaveChangesAsync(cancellationToken);
            var addExistingRoleResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!addExistingRoleResult.Succeeded)
            {
                _database.UserCompanies.Remove(new BlendUserCompany { UserId = user.Id, CompanyId = companyId });
                await _database.SaveChangesAsync(cancellationToken);
                return new UserAdministrationResult
                {
                    Errors = addExistingRoleResult.Errors.Select(error => error.Description).ToArray()
                };
            }

            return new UserAdministrationResult { Succeeded = true, IsExistingUser = true };
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Failure("Display name is required when creating a new account.");
        }

        user = new BlendIdentityUser
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            CompanyId = companyId,
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

        _database.UserCompanies.Add(new BlendUserCompany { UserId = user.Id, CompanyId = companyId });
        await _database.SaveChangesAsync(cancellationToken);
        var addRoleResult = await _userManager.AddToRoleAsync(user, role.Name);
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

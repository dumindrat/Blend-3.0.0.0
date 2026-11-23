using System.ComponentModel.DataAnnotations;
using Blend.BlazorApp.Authentication;
using Blend.BlazorApp.Models;
using Blend.BlazorApp.Pages.Account;
using Blend.BlazorApp.Pages.Admin;
using Blend.BlazorApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class SessionAndValidationTests
{
    [TestMethod]
    public void SignInResetsCompanyAndClearRemovesCredentials()
    {
        var session = new BlendSession();
        session.SignIn("first", "authorization", "token");
        session.SelectCompany(new CompanyChoice { CompanyId = 1 });
        session.SignIn("second", "new authorization", "new token");
        Assert.IsNull(session.Company);
        Assert.IsTrue(session.IsAuthenticated);
        session.Clear();
        Assert.IsFalse(session.IsAuthenticated);
        Assert.AreEqual(string.Empty, session.UserName);
        Assert.AreEqual(string.Empty, session.Authorization);
        Assert.IsNull(session.Company);
    }

    [TestMethod]
    public void PasswordChangeRequiresMatchingStrongLengthPassword()
    {
        var input = new ChangePasswordModel.ChangePasswordInput { CurrentPassword = "old", NewPassword = "short", ConfirmPassword = "different" };
        var errors = new List<ValidationResult>();
        Assert.IsFalse(Validator.TryValidateObject(input, new ValidationContext(input), errors, validateAllProperties: true));
        Assert.IsTrue(errors.Any(error => error.MemberNames.Contains(nameof(input.NewPassword))));
        Assert.IsTrue(errors.Any(error => error.MemberNames.Contains(nameof(input.ConfirmPassword))));
    }

    [TestMethod]
    public async Task CompanyUpdatePersistsSupportedDateFormatAndRejectsUnsupportedFormat()
    {
        var factory = new TestDatabaseFactory();
        await using var db = factory.CreateDbContext();
        var company = TestData.Company();
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        var page = new CompaniesModel(db, null!)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
            Input = new CompaniesModel.CompanyInput
            {
                Name = company.Name,
                DateFormat = CompanyDateFormats.DayFirst,
                SubscriptionStartDate = company.SubscriptionStartDate,
                SubscriptionEndDate = company.SubscriptionEndDate
            }
        };
        await page.OnPostUpdateAsync(company.Id);
        Assert.AreEqual(CompanyDateFormats.DayFirst, company.DateFormat);
        page.Input.DateFormat = "yyyy-MM-dd";
        await page.OnPostUpdateAsync(company.Id);
        Assert.IsFalse(page.ModelState.IsValid);
        Assert.AreEqual(CompanyDateFormats.DayFirst, company.DateFormat);
    }

    [TestMethod]
    public async Task CompanyUpdateRejectsReversedDatesWithoutSaving()
    {
        var factory = new TestDatabaseFactory();
        await using var db = factory.CreateDbContext();
        var company = TestData.Company();
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        var page = new CompaniesModel(db, null!)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
            Input = new CompaniesModel.CompanyInput
            {
                Name = "Changed", SubscriptionStartDate = new DateOnly(2026, 2, 1), SubscriptionEndDate = new DateOnly(2026, 1, 1)
            }
        };
        await page.OnPostUpdateAsync(company.Id);
        Assert.IsFalse(page.ModelState.IsValid);
        Assert.AreEqual("Test Company", company.Name);
    }
}

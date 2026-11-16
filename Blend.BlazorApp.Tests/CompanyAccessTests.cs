using Blend.BlazorApp.Authentication;
using Microsoft.AspNetCore.Http;

namespace Blend.BlazorApp.Tests;

[TestClass]
public sealed class CompanyAccessTests
{
    [TestMethod]
    public async Task ActiveMembershipAllowsRequest()
    {
        var factory = new TestDatabaseFactory();
        var company = TestData.Company();
        await using (var db = factory.CreateDbContext())
        {
            db.Companies.Add(company);
            db.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            await db.SaveChangesAsync();
        }
        var context = new DefaultHttpContext { User = TestData.Principal(company.Id) };
        context.Request.Path = "/Brokers";
        var nextCalled = false;
        await new BlendCompanyAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(context, factory);
        Assert.IsTrue(nextCalled);
    }

    [TestMethod]
    [DataRow("missing")]
    [DataRow("expired")]
    [DataRow("disabled")]
    public async Task InactiveOrMissingMembershipRedirects(string condition)
    {
        var factory = new TestDatabaseFactory();
        var company = TestData.Company();
        if (condition == "expired") company.SubscriptionEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        if (condition == "disabled") company.IsEnabled = false;
        await using (var db = factory.CreateDbContext())
        {
            db.Companies.Add(company);
            if (condition != "missing") db.UserCompanies.Add(new BlendUserCompany { UserId = "user-1", CompanyId = company.Id });
            await db.SaveChangesAsync();
        }
        var context = new DefaultHttpContext { User = TestData.Principal(company.Id) };
        context.Request.Path = "/Brokers";
        var nextCalled = false;
        await new BlendCompanyAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(context, factory);
        Assert.IsFalse(nextCalled);
        Assert.AreEqual("/Account/CompanyAccessUnavailable", context.Response.Headers.Location.ToString());
    }

    [TestMethod]
    [DataRow("/Account/Logout")]
    [DataRow("/Account/WorkingCompany")]
    [DataRow("/_framework/blazor.web.js")]
    public async Task RecoveryRoutesRemainAvailable(string path)
    {
        var context = new DefaultHttpContext { User = TestData.Principal(null) };
        context.Request.Path = path;
        var nextCalled = false;
        await new BlendCompanyAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(context, new TestDatabaseFactory());
        Assert.IsTrue(nextCalled);
    }

    [TestMethod]
    public async Task PlatformAdminBypassesCompanyGate()
    {
        var context = new DefaultHttpContext { User = TestData.Principal(null, admin: true) };
        context.Request.Path = "/Brokers";
        var nextCalled = false;
        await new BlendCompanyAccessMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }).InvokeAsync(context, new TestDatabaseFactory());
        Assert.IsTrue(nextCalled);
    }
}

using AppiumInterviewFramework.Pages;
using NUnit.Framework;

namespace AppiumInterviewFramework.Tests.Tests;

public sealed class LoginTests : AppiumTestBase
{
    [Test, Category("Mobile")]
    public void User_can_log_in_with_valid_credentials()
    {
        var username = Environment.GetEnvironmentVariable("DEMO_APP_USERNAME");
        var password = Environment.GetEnvironmentVariable("DEMO_APP_PASSWORD");

        Assume.That(username, Is.Not.Null.And.Not.Empty, "Set DEMO_APP_USERNAME in the local shell or CI secret store.");
        Assume.That(password, Is.Not.Null.And.Not.Empty, "Set DEMO_APP_PASSWORD in the local shell or CI secret store.");

        var loginPage = new LoginPage(Driver!, Waits!);
        loginPage.Login(username!, password!);

        Assert.That(loginPage.IsHomeScreenDisplayed(), Is.True, "The home screen should be visible after a successful login.");
    }
}

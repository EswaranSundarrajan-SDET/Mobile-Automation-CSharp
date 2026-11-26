using MobileAutomation.Pages;
using NUnit.Framework;

namespace MobileAutomation.Tests;

[Category("Mobile")]
public sealed class LoginTests : BaseTest
{
    [Test]
    [Description("A user who signs in with valid credentials should reach the home screen.")]
    public void ValidCredentials_OpenHomeScreen()
    {
        var username = ReadLoginCredential("DEMO_APP_USERNAME");
        var password = ReadLoginCredential("DEMO_APP_PASSWORD");

        var loginPage = new LoginPage(Driver, WaitTimeout);
        var homePage = loginPage.SignIn(username, password);

        Assert.That(homePage.IsDisplayed(), Is.True, "The home screen should appear after sign-in.");
    }

    private static string ReadLoginCredential(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrEmpty(value))
        {
            Assert.Inconclusive($"Set {variableName} in the local shell or CI secret store.");
        }

        return value;
    }
}

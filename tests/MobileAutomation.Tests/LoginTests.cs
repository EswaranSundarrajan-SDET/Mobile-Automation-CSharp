using MobileAutomation.Pages;
using NUnit.Framework;

namespace MobileAutomation.Tests;

/// <summary>Checks that the mobile app lets a user sign in and reach the home screen.</summary>
[Category("Mobile")]
public sealed class LoginTests : BaseTest
{
    [Test]
    [Description("A user who signs in with valid credentials should reach the home screen.")]
    public void ValidCredentials_OpenHomeScreen()
    {
        // Read the sign-in name for the test account from the environment.
        var username = ReadLoginCredential("DEMO_APP_USERNAME");
        // Read the password for that account from the environment.
        var password = ReadLoginCredential("DEMO_APP_PASSWORD");

        // Use the setup driver to operate the sign-in screen.
        var loginPage = new LoginPage(Driver, WaitTimeout);
        // Submit the credentials and get a page object for the expected destination.
        var homePage = loginPage.SignIn(username, password);

        Assert.That(homePage.IsDisplayed(), Is.True, "The home screen should appear after sign-in.");
    }

    /// <summary>Read a login credential required by this test.</summary>
    /// <param name="variableName">The name of the environment variable that holds the credential.</param>
    /// <returns>The credential supplied for the test run.</returns>
    private static string ReadLoginCredential(string variableName)
    {
        // Read the configured credential from the current process.
        var value = Environment.GetEnvironmentVariable(variableName);
        // Mark the test inconclusive when there is no credential to try.
        if (string.IsNullOrEmpty(value))
        {
            Assert.Inconclusive($"Set {variableName} in the local shell or CI secret store.");
        }

        return value;
    }
}

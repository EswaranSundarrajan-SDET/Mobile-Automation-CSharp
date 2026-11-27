using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace MobileAutomation.Pages;

/// <summary>The sign-in screen. It owns the locators for the two fields and the sign-in button.</summary>
public sealed class LoginPage : BasePage
{
    // Match these sample accessibility IDs to the IDs in your app.
    /// <summary>Finds the field where the user types their username.</summary>
    private static readonly By UsernameField = MobileBy.AccessibilityId("login_username");
    /// <summary>Finds the field where the user types their password.</summary>
    private static readonly By PasswordField = MobileBy.AccessibilityId("login_password");
    /// <summary>Finds the button that submits the sign-in form.</summary>
    private static readonly By SignInButton = MobileBy.AccessibilityId("login_button");

    /// <summary>Set up the sign-in page object using the current session.</summary>
    /// <param name="driver">The driver for the current Appium session.</param>
    /// <param name="waitTimeout">The longest time to wait for an element.</param>
    public LoginPage(IWebDriver driver, TimeSpan waitTimeout) : base(driver, waitTimeout)
    {
    }

    /// <summary>Enter the credentials and submit the form. The test can then check the home screen.</summary>
    /// <param name="username">The test account name to enter.</param>
    /// <param name="password">The test account password to enter.</param>
    /// <returns>A HomePage object the test can use to check whether sign-in succeeded.</returns>
    public HomePage SignIn(string username, string password)
    {
        EnterText(UsernameField, username);
        EnterText(PasswordField, password);
        Click(SignInButton);
        return new HomePage(Driver, WaitTimeout);
    }
}

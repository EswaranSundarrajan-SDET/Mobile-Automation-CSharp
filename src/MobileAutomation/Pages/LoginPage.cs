using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace MobileAutomation.Pages;

/// <summary>The sign-in screen. It owns the locators for the two fields and the sign-in button.</summary>
public sealed class LoginPage : BasePage
{
    // Match these sample accessibility IDs to the IDs in your app.
    private static readonly By UsernameField = MobileBy.AccessibilityId("login_username");
    private static readonly By PasswordField = MobileBy.AccessibilityId("login_password");
    private static readonly By SignInButton = MobileBy.AccessibilityId("login_button");

    public LoginPage(IWebDriver driver, TimeSpan waitTimeout) : base(driver, waitTimeout)
    {
    }

    /// <summary>Enter the credentials and submit the form. The test can then check the home screen.</summary>
    public HomePage SignIn(string username, string password)
    {
        EnterText(UsernameField, username);
        EnterText(PasswordField, password);
        Click(SignInButton);
        return new HomePage(Driver, WaitTimeout);
    }
}

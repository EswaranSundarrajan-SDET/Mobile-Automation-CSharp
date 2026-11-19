using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace AppiumInterviewFramework.Pages;

public sealed class LoginPage : BasePage
{
    // Replace these accessibility IDs with the contract exposed by the application under test.
    private static readonly By UsernameInput = MobileBy.AccessibilityId("login_username");
    private static readonly By PasswordInput = MobileBy.AccessibilityId("login_password");
    private static readonly By LoginButton = MobileBy.AccessibilityId("login_button");
    private static readonly By HomeScreen = MobileBy.AccessibilityId("home_screen");

    public LoginPage(AppiumDriver driver, Utilities.Waits waits)
        : base(driver, waits)
    {
    }

    public void Login(string username, string password)
    {
        EnterText(UsernameInput, username);
        EnterText(PasswordInput, password);
        Tap(LoginButton);
    }

    public bool IsHomeScreenDisplayed() => Waits.IsDisplayed(HomeScreen);
}

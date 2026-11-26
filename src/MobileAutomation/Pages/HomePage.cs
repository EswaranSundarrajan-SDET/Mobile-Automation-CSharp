using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace MobileAutomation.Pages;

/// <summary>The first screen a user sees after signing in successfully.</summary>
public sealed class HomePage : BasePage
{
    // Use an accessibility ID that appears only on the home screen.
    private static readonly By HomeScreen = MobileBy.AccessibilityId("home_screen");

    public HomePage(IWebDriver driver, TimeSpan waitTimeout) : base(driver, waitTimeout)
    {
    }

    /// <summary>Wait for the home screen to appear and report whether it was displayed.</summary>
    public bool IsDisplayed() => IsElementDisplayed(HomeScreen);
}

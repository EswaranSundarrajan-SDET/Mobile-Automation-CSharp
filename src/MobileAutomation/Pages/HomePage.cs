using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace MobileAutomation.Pages;

/// <summary>The first screen a user sees after signing in successfully.</summary>
public sealed class HomePage : BasePage
{
    // Use the accessibility ID of an element that appears only on the home screen.
    private static readonly By HomeScreen = MobileBy.AccessibilityId("home_screen");

    /// <summary>Set up the home page object to check the screen using the current session.</summary>
    /// <param name="driver">The driver for the current Appium session.</param>
    /// <param name="waitTimeout">The longest time to wait for an element.</param>
    public HomePage(IWebDriver driver, TimeSpan waitTimeout) : base(driver, waitTimeout)
    {
    }

    /// <summary>Wait for the home screen to appear and report whether it was displayed.</summary>
    /// <returns>True if the home screen element appears within the timeout. Otherwise, false.</returns>
    public bool IsDisplayed() => IsElementDisplayed(HomeScreen);
}

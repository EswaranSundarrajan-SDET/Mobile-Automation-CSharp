using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MobileAutomation.Pages;

/// <summary>Provides the driver and common element actions used by every page object.</summary>
public abstract class BasePage
{
    private readonly WebDriverWait wait;

    protected BasePage(IWebDriver driver, TimeSpan waitTimeout)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        WaitTimeout = waitTimeout;
        wait = new WebDriverWait(driver, waitTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };

        // App screens can redraw while loading. Find the element again on the next poll.
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    protected IWebDriver Driver { get; }

    protected TimeSpan WaitTimeout { get; }

    /// <summary>Wait for a visible, enabled element, then tap it.</summary>
    protected void Click(By locator) => WaitForClickableElement(locator).Click();

    /// <summary>Replace the contents of a visible text field.</summary>
    protected void EnterText(By locator, string text)
    {
        var field = WaitForVisibleElement(locator);
        field.Clear();
        field.SendKeys(text);
    }

    /// <summary>Return the element once it is visible. A timeout fails the current action.</summary>
    protected IWebElement WaitForVisibleElement(By locator) => wait.Until(driver =>
    {
        var element = driver.FindElement(locator);
        return element.Displayed ? element : null;
    })!;

    /// <summary>Return the element once it is visible and enabled.</summary>
    protected IWebElement WaitForClickableElement(By locator) => wait.Until(driver =>
    {
        var element = driver.FindElement(locator);
        return element.Displayed && element.Enabled ? element : null;
    })!;

    /// <summary>Give a screen time to load and return false if its identifying element never appears.</summary>
    protected bool IsElementDisplayed(By locator)
    {
        try
        {
            WaitForVisibleElement(locator);
            return true;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
}

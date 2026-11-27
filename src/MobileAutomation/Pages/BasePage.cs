using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MobileAutomation.Pages;

/// <summary>Provides the driver and common element actions used by every page object.</summary>
public abstract class BasePage
{
    /// <summary>Repeatedly checks for an element until it is ready or the timeout is reached.</summary>
    private readonly WebDriverWait wait;

    /// <summary>Give the page the current driver and set up its explicit wait.</summary>
    /// <param name="driver">The driver for the current Appium session.</param>
    /// <param name="waitTimeout">The longest time to wait for an element.</param>
    protected BasePage(IWebDriver driver, TimeSpan waitTimeout)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        WaitTimeout = waitTimeout;
        // Check for the element every 250 milliseconds until the wait ends.
        wait = new WebDriverWait(driver, waitTimeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };

        // App screens can redraw while loading. Find the element again on the next poll.
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    /// <summary>The shared driver this page uses to interact with the app.</summary>
    protected IWebDriver Driver { get; }

    /// <summary>The maximum wait to pass to the next page object.</summary>
    protected TimeSpan WaitTimeout { get; }

    /// <summary>Wait for a visible, enabled element, then tap it.</summary>
    /// <param name="locator">How Selenium finds the element, such as by its accessibility ID.</param>
    protected void Click(By locator) => WaitForClickableElement(locator).Click();

    /// <summary>Replace the contents of a visible text field.</summary>
    /// <param name="locator">How Selenium finds the element, such as by its accessibility ID.</param>
    /// <param name="text">The new text to enter after clearing the field.</param>
    protected void EnterText(By locator, string text)
    {
        // Find the visible field before clearing it and typing the new text.
        var field = WaitForVisibleElement(locator);
        field.Clear();
        field.SendKeys(text);
    }

    /// <summary>Return the element once it is visible. A timeout fails the current action.</summary>
    /// <param name="locator">How Selenium finds the element, such as by its accessibility ID.</param>
    /// <returns>The first element found that is displayed.</returns>
    protected IWebElement WaitForVisibleElement(By locator) => wait.Until(driver =>
    {
        // Look up the element on this poll. Returning null tells Selenium to keep waiting.
        var element = driver.FindElement(locator);
        return element.Displayed ? element : null;
    })!;

    /// <summary>Return the element once it is visible and enabled.</summary>
    /// <param name="locator">How Selenium finds the element, such as by its accessibility ID.</param>
    /// <returns>The first element found that is displayed and enabled.</returns>
    protected IWebElement WaitForClickableElement(By locator) => wait.Until(driver =>
    {
        // Look up the element on this poll. It must be visible and ready for input.
        var element = driver.FindElement(locator);
        return element.Displayed && element.Enabled ? element : null;
    })!;

    /// <summary>Give a screen time to load and return false if its identifying element never appears.</summary>
    /// <param name="locator">How Selenium finds the element, such as by its accessibility ID.</param>
    /// <returns>True if the element appears within the timeout. Otherwise, false.</returns>
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

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AppiumInterviewFramework.Utilities;

public sealed class Waits
{
    private readonly WebDriverWait wait;

    public Waits(IWebDriver driver, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(driver);
        wait = new WebDriverWait(driver, timeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    public IWebElement UntilVisible(By locator) => wait.Until(driver =>
    {
        var element = driver.FindElement(locator);
        return element.Displayed ? element : null;
    })!;

    public IWebElement UntilClickable(By locator) => wait.Until(driver =>
    {
        var element = driver.FindElement(locator);
        return element.Displayed && element.Enabled ? element : null;
    })!;

    public bool IsDisplayed(By locator)
    {
        try
        {
            return UntilVisible(locator).Displayed;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
}

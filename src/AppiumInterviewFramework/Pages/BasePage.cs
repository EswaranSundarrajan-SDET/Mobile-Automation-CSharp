using AppiumInterviewFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace AppiumInterviewFramework.Pages;

public abstract class BasePage
{
    protected BasePage(AppiumDriver driver, Waits waits)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Waits = waits ?? throw new ArgumentNullException(nameof(waits));
    }

    protected AppiumDriver Driver { get; }

    protected Waits Waits { get; }

    protected void Tap(By locator) => Waits.UntilClickable(locator).Click();

    protected void EnterText(By locator, string text)
    {
        var element = Waits.UntilVisible(locator);
        element.Clear();
        element.SendKeys(text);
    }
}

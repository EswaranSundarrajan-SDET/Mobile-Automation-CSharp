using MobileAutomation.Configuration;
using MobileAutomation.Drivers;
using MobileAutomation.Reporting;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium.Appium;

namespace MobileAutomation.Tests;

/// <summary>Starts a fresh driver for each mobile test and closes the session when the test finishes.</summary>
public abstract class BaseTest
{
    private AppiumDriver? activeDriver;

    protected AppiumDriver Driver => activeDriver
        ?? throw new InvalidOperationException("The mobile driver is available after test setup.");

    protected TimeSpan WaitTimeout { get; private set; }

    [SetUp]
    public void StartDriver()
    {
        var runMobileTests = Environment.GetEnvironmentVariable("RUN_APPIUM_TESTS");
        if (!string.Equals(runMobileTests, "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("Set RUN_APPIUM_TESTS=true to run mobile tests on a configured device.");
        }

        var settings = TestSettingsLoader.Load();
        WaitTimeout = settings.WaitTimeout;
        activeDriver = DriverFactory.CreateDriver(settings);
    }

    [TearDown]
    public void StopDriver()
    {
        if (activeDriver is null)
        {
            return;
        }

        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                SaveFailureDetails(activeDriver);
            }
        }
        finally
        {
            // In Selenium's .NET driver Dispose ends the session and releases the connection.
            activeDriver.Dispose();
            activeDriver = null;
        }
    }

    private static void SaveFailureDetails(AppiumDriver driver)
    {
        var context = TestContext.CurrentContext;
        var directory = Path.Combine(context.WorkDirectory, "artifacts");
        try
        {
            var files = FailureReport.Save(driver, directory, context.Test.Name);
            TestContext.AddTestAttachment(files.LogPath, "Failure details");
            if (files.ScreenshotPath is not null)
            {
                TestContext.AddTestAttachment(files.ScreenshotPath, "Screen at failure");
            }
        }
        catch (Exception error)
        {
            // An unavailable device should not hide the assertion that failed.
            TestContext.Progress.WriteLine($"Could not save the failure report: {error.Message}");
        }
    }
}

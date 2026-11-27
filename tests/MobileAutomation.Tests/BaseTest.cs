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
    /// <summary>The driver created during setup, or null when there is no active session.</summary>
    private AppiumDriver? activeDriver;

    /// <summary>The active driver that a test passes to its page objects.</summary>
    protected AppiumDriver Driver => activeDriver
        ?? throw new InvalidOperationException("The mobile driver is available after test setup.");

    /// <summary>The time a page can wait for an element during this test.</summary>
    protected TimeSpan WaitTimeout { get; private set; }

    /// <summary>Read the settings and start the driver before each enabled mobile test.</summary>
    [SetUp]
    public void StartDriver()
    {
        // Read the flag that allows this run to open a device session.
        var runMobileTests = Environment.GetEnvironmentVariable("RUN_APPIUM_TESTS");
        if (!string.Equals(runMobileTests, "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("Set RUN_APPIUM_TESTS=true to run mobile tests on a configured device.");
        }

        // Read the selected device and server along with the element wait time.
        var settings = TestSettingsLoader.Load();
        WaitTimeout = settings.WaitTimeout;
        activeDriver = DriverFactory.CreateDriver(settings);
    }

    /// <summary>Attach details when the test fails, then close the driver session.</summary>
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

    /// <summary>Save the failure log and available screenshot as NUnit attachments.</summary>
    /// <param name="driver">The driver for the current Appium session.</param>
    private static void SaveFailureDetails(AppiumDriver driver)
    {
        // Get the current test's name and working folder from NUnit.
        var context = TestContext.CurrentContext;
        // Place the report under the test run's artifacts folder.
        var directory = Path.Combine(context.WorkDirectory, "artifacts");
        try
        {
            // Get the paths of the files saved for this failure.
            var files = FailureReport.Save(driver, directory, context.Test.Name);
            TestContext.AddTestAttachment(files.LogPath, "Failure details");
            if (files.ScreenshotPath is not null)
            {
                TestContext.AddTestAttachment(files.ScreenshotPath, "Screen at failure");
            }
        }
        // If reporting fails, write the error to the test output.
        catch (Exception error)
        {
            // An unavailable device should not hide the assertion that failed.
            TestContext.Progress.WriteLine($"Could not save the failure report: {error.Message}");
        }
    }
}

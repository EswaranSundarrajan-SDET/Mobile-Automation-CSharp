using AppiumInterviewFramework.Configuration;
using AppiumInterviewFramework.Drivers;
using AppiumInterviewFramework.Utilities;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium.Appium;

namespace AppiumInterviewFramework.Tests.Tests;

public abstract class AppiumTestBase
{
    protected AppiumDriver? Driver { get; private set; }

    protected Waits? Waits { get; private set; }

    protected FrameworkOptions? Options { get; private set; }

    [SetUp]
    public void SetUp()
    {
        if (!IsMobileExecutionEnabled())
        {
            Assert.Ignore("Mobile execution is disabled. Set RUN_APPIUM_TESTS=true after configuring a real device and app.");
        }

        Options = ConfigurationLoader.Load();
        Driver = DriverFactory.Create(Options);
        Waits = new Waits(Driver, Options.WaitTimeout);
    }

    [TearDown]
    public void TearDown()
    {
        if (Driver is null)
        {
            return;
        }

        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            try
            {
                var result = FailureArtifacts.Capture(
                    Driver,
                    Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts"),
                    TestContext.CurrentContext.Test.Name);

                TestContext.Progress.WriteLine($"Failure screenshot: {result.ScreenshotPath ?? "not supported"}");
                TestContext.Progress.WriteLine($"Failure log: {result.LogPath}");
            }
            catch (Exception artifactError)
            {
                TestContext.Progress.WriteLine($"Could not capture failure artifacts: {artifactError.Message}");
            }
        }

        try
        {
            Driver.Quit();
        }
        finally
        {
            Driver.Dispose();
            Driver = null;
        }
    }

    private static bool IsMobileExecutionEnabled() =>
        string.Equals(Environment.GetEnvironmentVariable("RUN_APPIUM_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
}

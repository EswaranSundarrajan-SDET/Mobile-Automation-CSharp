using MobileAutomation.Configuration;
using NUnit.Framework;

namespace MobileAutomation.Tests;

/// <summary>Checks the configuration without opening an Appium session.</summary>
[Category("BuildValidation")]
public sealed class ConfigurationTests
{
    [Test]
    public void DefaultSettings_SelectLocalAndroid()
    {
        var settings = TestSettingsLoader.Load();

        Assert.That(settings.AppiumServerUrl, Is.EqualTo("http://127.0.0.1:4723"));
        Assert.That(settings.WaitTimeoutSeconds, Is.GreaterThan(0));
        Assert.That(settings.TargetPlatform, Is.EqualTo(MobilePlatform.Android));
        Assert.That(settings.BrowserStack.Enabled, Is.False);
        Assert.That(settings.EffectiveServerUrl, Is.EqualTo(settings.AppiumServerUrl));
    }

    [Test]
    public void MobilePlatformVariable_SelectsIosDeviceSettings()
    {
        var previousPlatform = Environment.GetEnvironmentVariable("MOBILE_PLATFORM");
        try
        {
            Environment.SetEnvironmentVariable("MOBILE_PLATFORM", "Ios");
            var settings = TestSettingsLoader.Load();

            Assert.That(settings.TargetPlatform, Is.EqualTo(MobilePlatform.Ios));
            Assert.That(settings.Device.AutomationName, Is.EqualTo("XCUITest"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("MOBILE_PLATFORM", previousPlatform);
        }
    }

    [Test]
    public void BrowserStackValidation_ListsMissingCredentialsAndApp()
    {
        var settings = new TestSettings
        {
            BrowserStack = new BrowserStackSettings { Enabled = true }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => TestSettingsLoader.Validate(settings));

        Assert.That(exception!.Message, Does.Contain("BrowserStack.Username"));
        Assert.That(exception.Message, Does.Contain("BrowserStack.AccessKey"));
        Assert.That(exception.Message, Does.Contain("BrowserStack.AppUrl"));

        Assert.That(settings.BrowserStack.DeviceName, Is.EqualTo("Google Pixel 7"));
        Assert.That(settings.BrowserStack.OsVersion, Is.EqualTo("13.0"));
        Assert.That(settings.BrowserStack.ProjectName, Is.EqualTo("Mobile Automation"));
        Assert.That(settings.BrowserStack.BuildName, Is.EqualTo("Regression Build"));
        Assert.That(settings.BrowserStack.SessionName, Is.EqualTo("Login Test"));
        Assert.That(settings.BrowserStack.Debug, Is.True);
        Assert.That(settings.BrowserStack.Video, Is.True);
        Assert.That(settings.BrowserStack.NetworkLogs, Is.True);
    }

    [Test]
    public void EnvironmentVariables_OverrideBrowserStackDefaults()
    {
        var namesAndValues = new Dictionary<string, string>
        {
            ["BROWSERSTACK_ENABLED"] = "true",
            ["BROWSERSTACK_USERNAME"] = "test-user",
            ["BROWSERSTACK_ACCESS_KEY"] = "test-key",
            ["BROWSERSTACK_APP_URL"] = "bs://test-app",
            ["BROWSERSTACK_DEVICE_NAME"] = "Pixel 7",
            ["BROWSERSTACK_OS_VERSION"] = "13.0",
            ["BROWSERSTACK_BUILD_NAME"] = "build-from-environment",
            ["BROWSERSTACK_DEBUG"] = "true"
        };
        var previousValues = namesAndValues.Keys.ToDictionary(
            name => name,
            Environment.GetEnvironmentVariable);

        try
        {
            foreach (var pair in namesAndValues)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }

            var settings = TestSettingsLoader.Load();

            Assert.That(settings.BrowserStack.Enabled, Is.True);
            Assert.That(settings.BrowserStack.AppUrl, Is.EqualTo("bs://test-app"));
            Assert.That(settings.BrowserStack.DeviceName, Is.EqualTo("Pixel 7"));
            Assert.That(settings.BrowserStack.BuildName, Is.EqualTo("build-from-environment"));
            Assert.That(settings.BrowserStack.Debug, Is.True);
        }
        finally
        {
            foreach (var pair in previousValues)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }
}

using AppiumInterviewFramework.Configuration;
using NUnit.Framework;

namespace AppiumInterviewFramework.Tests.Tests;

public sealed class ConfigurationTests
{
    [Test, Category("BuildValidation")]
    public void Configuration_loads_without_secrets_or_device_requirements()
    {
        var options = ConfigurationLoader.Load();

        Assert.That(options.AppiumServerUrl, Is.EqualTo("http://127.0.0.1:4723"));
        Assert.That(options.WaitTimeoutSeconds, Is.GreaterThan(0));
        Assert.That(options.TargetPlatform, Is.EqualTo(MobilePlatform.Android));
        Assert.That(options.BrowserStack.Enabled, Is.False);
        Assert.That(options.EffectiveServerUrl, Is.EqualTo(options.AppiumServerUrl));
    }

    [Test, Category("BuildValidation")]
    public void BrowserStack_validation_lists_missing_required_settings()
    {
        var options = new FrameworkOptions
        {
            BrowserStack = new BrowserStackOptions { Enabled = true }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => ConfigurationLoader.Validate(options));

        Assert.That(exception!.Message, Does.Contain("BrowserStack.Username"));
        Assert.That(exception.Message, Does.Contain("BrowserStack.AccessKey"));
        Assert.That(exception.Message, Does.Contain("BrowserStack.AppUrl"));
        Assert.That(exception.Message, Does.Contain("BrowserStack.DeviceName"));
        Assert.That(exception.Message, Does.Contain("BrowserStack.OsVersion"));
    }

    [Test, Category("BuildValidation")]
    public void BrowserStack_environment_aliases_override_json_defaults()
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

            var options = ConfigurationLoader.Load();

            Assert.That(options.BrowserStack.Enabled, Is.True);
            Assert.That(options.BrowserStack.AppUrl, Is.EqualTo("bs://test-app"));
            Assert.That(options.BrowserStack.DeviceName, Is.EqualTo("Pixel 7"));
            Assert.That(options.BrowserStack.BuildName, Is.EqualTo("build-from-environment"));
            Assert.That(options.BrowserStack.Debug, Is.True);
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

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
}

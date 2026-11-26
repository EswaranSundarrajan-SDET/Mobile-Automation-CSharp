using MobileAutomation.Configuration;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;

namespace MobileAutomation.Drivers;

/// <summary>Starts an Android or iOS Appium session on the selected server.</summary>
public static class DriverFactory
{
    /// <summary>Create one driver for the current test. The caller owns the session and closes it.</summary>
    public static AppiumDriver CreateDriver(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        TestSettingsLoader.Validate(settings);

        var device = settings.Device;
        var browserStack = settings.BrowserStack;
        var deviceName = browserStack.Enabled ? browserStack.DeviceName : device.DeviceName;
        var platformVersion = browserStack.Enabled ? browserStack.OsVersion : device.PlatformVersion;
        var app = browserStack.Enabled ? browserStack.AppUrl : device.AppPath;
        var capabilities = new AppiumOptions
        {
            PlatformName = settings.TargetPlatform == MobilePlatform.Android ? "Android" : "iOS"
        };

        if (!string.IsNullOrWhiteSpace(device.AutomationName))
        {
            capabilities.AutomationName = device.AutomationName;
        }

        if (!string.IsNullOrWhiteSpace(deviceName))
        {
            capabilities.DeviceName = deviceName;
        }

        if (!string.IsNullOrWhiteSpace(platformVersion))
        {
            capabilities.PlatformVersion = platformVersion;
        }

        if (!string.IsNullOrWhiteSpace(app))
        {
            capabilities.App = app;
        }

        capabilities.AddAdditionalAppiumOption("noReset", settings.NoReset);
        AddBrowserStackCapabilities(capabilities, browserStack);

        if (settings.TargetPlatform == MobilePlatform.Android)
        {
            AddIfPresent(capabilities, "appPackage", device.AppPackage);
            AddIfPresent(capabilities, "appActivity", device.AppActivity);
            return new AndroidDriver(new Uri(settings.EffectiveServerUrl), capabilities, TimeSpan.FromMinutes(3));
        }

        AddIfPresent(capabilities, "bundleId", device.BundleId);
        return new IOSDriver(new Uri(settings.EffectiveServerUrl), capabilities, TimeSpan.FromMinutes(3));
    }

    private static void AddIfPresent(AppiumOptions settings, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            settings.AddAdditionalAppiumOption(key, value);
        }
    }

    private static void AddBrowserStackCapabilities(AppiumOptions capabilities, BrowserStackSettings settings)
    {
        if (!settings.Enabled)
        {
            return;
        }

        var browserStackOptions = new Dictionary<string, object>
        {
            ["userName"] = settings.Username,
            ["accessKey"] = settings.AccessKey,
            ["projectName"] = settings.ProjectName,
            ["buildName"] = settings.BuildName,
            ["sessionName"] = settings.SessionName,
            ["debug"] = settings.Debug,
            ["video"] = settings.Video,
            ["networkLogs"] = settings.NetworkLogs
        };

        capabilities.AddAdditionalOption("bstack:options", browserStackOptions);
    }
}

using AppiumInterviewFramework.Configuration;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using System.Collections.Generic;

namespace AppiumInterviewFramework.Drivers;

public static class DriverFactory
{
    public static AppiumDriver Create(FrameworkOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ConfigurationLoader.Validate(options);

        var device = options.Device;
        var browserStack = options.BrowserStack;
        var deviceName = browserStack.Enabled ? browserStack.DeviceName : device.DeviceName;
        var platformVersion = browserStack.Enabled ? browserStack.OsVersion : device.PlatformVersion;
        var app = browserStack.Enabled ? browserStack.AppUrl : device.AppPath;
        var capabilities = new AppiumOptions
        {
            PlatformName = options.TargetPlatform == MobilePlatform.Android ? "Android" : "iOS"
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

        capabilities.AddAdditionalAppiumOption("noReset", options.NoReset);
        AddBrowserStackOptions(capabilities, browserStack);

        if (options.TargetPlatform == MobilePlatform.Android)
        {
            AddIfPresent(capabilities, "appPackage", device.AppPackage);
            AddIfPresent(capabilities, "appActivity", device.AppActivity);
            return new AndroidDriver(new Uri(options.EffectiveServerUrl), capabilities, TimeSpan.FromMinutes(3));
        }

        AddIfPresent(capabilities, "bundleId", device.BundleId);
        return new IOSDriver(new Uri(options.EffectiveServerUrl), capabilities, TimeSpan.FromMinutes(3));
    }

    private static void AddIfPresent(AppiumOptions options, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            options.AddAdditionalAppiumOption(key, value);
        }
    }

    private static void AddBrowserStackOptions(AppiumOptions capabilities, BrowserStackOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        var browserStackOptions = new Dictionary<string, object>
        {
            ["userName"] = options.Username,
            ["accessKey"] = options.AccessKey,
            ["projectName"] = options.ProjectName,
            ["buildName"] = options.BuildName,
            ["sessionName"] = options.SessionName,
            ["debug"] = options.Debug,
            ["video"] = options.Video,
            ["networkLogs"] = options.NetworkLogs
        };

        capabilities.AddAdditionalOption("bstack:options", browserStackOptions);
    }
}

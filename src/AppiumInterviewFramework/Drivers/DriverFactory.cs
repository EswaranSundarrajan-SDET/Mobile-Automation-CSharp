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

        var device = options.Device;
        var capabilities = new AppiumOptions
        {
            PlatformName = options.TargetPlatform == MobilePlatform.Android ? "Android" : "iOS"
        };

        if (!string.IsNullOrWhiteSpace(device.AutomationName))
        {
            capabilities.AutomationName = device.AutomationName;
        }

        if (!string.IsNullOrWhiteSpace(device.DeviceName))
        {
            capabilities.DeviceName = device.DeviceName;
        }

        if (!string.IsNullOrWhiteSpace(device.PlatformVersion))
        {
            capabilities.PlatformVersion = device.PlatformVersion;
        }

        if (!string.IsNullOrWhiteSpace(device.AppPath))
        {
            capabilities.App = device.AppPath;
        }

        capabilities.AddAdditionalAppiumOption("noReset", options.NoReset);
        AddBrowserStackOptions(capabilities, options.BrowserStack);

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
            ["sessionName"] = options.SessionName
        };

        if (!string.IsNullOrWhiteSpace(options.App))
        {
            browserStackOptions["app"] = options.App;
        }

        capabilities.AddAdditionalOption("bstack:options", browserStackOptions);
    }
}

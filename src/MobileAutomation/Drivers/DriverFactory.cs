using MobileAutomation.Configuration;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;

namespace MobileAutomation.Drivers;

/// <summary>Starts an Android or iOS Appium session on the selected server.</summary>
public static class DriverFactory
{
    /// <summary>Create one driver for the current test. The caller owns the session and closes it.</summary>
    /// <param name="settings">The selected platform, device, app and server settings.</param>
    /// <returns>The new Android or iOS driver for this test.</returns>
    public static AppiumDriver CreateDriver(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        TestSettingsLoader.Validate(settings);

        // Get the app and device settings for the selected operating system.
        var device = settings.Device;
        // Check whether this run uses the BrowserStack device and app settings.
        var browserStack = settings.BrowserStack;
        // Choose the requested device name from the active settings.
        var deviceName = browserStack.Enabled ? browserStack.DeviceName : device.DeviceName;
        // Choose the Android or iOS version requested for this device.
        var platformVersion = browserStack.Enabled ? browserStack.OsVersion : device.PlatformVersion;
        // Use the uploaded app reference for cloud runs or the app path for local runs.
        var app = browserStack.Enabled ? browserStack.AppUrl : device.AppPath;
        // Build the options that Appium reads when it creates a session.
        var capabilities = new AppiumOptions
        {
            PlatformName = settings.TargetPlatform == MobilePlatform.Android ? "Android" : "iOS"
        };

        // Send optional values only when they are set, so Appium can apply its defaults.
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

        // Android can launch an installed app using its package and activity.
        if (settings.TargetPlatform == MobilePlatform.Android)
        {
            AddIfPresent(capabilities, "appPackage", device.AppPackage);
            AddIfPresent(capabilities, "appActivity", device.AppActivity);
            return new AndroidDriver(new Uri(settings.EffectiveServerUrl), capabilities, TimeSpan.FromMinutes(3));
        }

        // iOS can launch an installed app using its bundle ID.
        AddIfPresent(capabilities, "bundleId", device.BundleId);
        return new IOSDriver(new Uri(settings.EffectiveServerUrl), capabilities, TimeSpan.FromMinutes(3));
    }

    /// <summary>Add an Appium option when its value has been set.</summary>
    /// <param name="settings">The Appium options to send when the driver starts.</param>
    /// <param name="key">The name that Appium expects for the option.</param>
    /// <param name="value">The configured value to send if it is set.</param>
    private static void AddIfPresent(AppiumOptions settings, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            settings.AddAdditionalAppiumOption(key, value);
        }
    }

    /// <summary>Add cloud sign-in details and reporting options for a BrowserStack run.</summary>
    /// <param name="capabilities">The Appium options that will carry the BrowserStack options.</param>
    /// <param name="settings">The BrowserStack settings for this run.</param>
    private static void AddBrowserStackCapabilities(AppiumOptions capabilities, BrowserStackSettings settings)
    {
        if (!settings.Enabled)
        {
            return;
        }

        // Group the settings under bstack:options so BrowserStack can read them.
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

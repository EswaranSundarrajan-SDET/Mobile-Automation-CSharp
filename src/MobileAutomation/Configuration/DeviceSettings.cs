namespace MobileAutomation.Configuration;

/// <summary>Identifies the device and the app that Appium should open for one platform.</summary>
public sealed class DeviceSettings
{
    /// <summary>The name Appium receives for the phone, emulator or simulator.</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>The Android or iOS version requested for the device.</summary>
    public string PlatformVersion { get; set; } = string.Empty;

    /// <summary>The Appium driver to use, such as UiAutomator2 for Android or XCUITest for iOS.</summary>
    public string AutomationName { get; set; } = string.Empty;

    /// <summary>The path to the app build on the machine running Appium.</summary>
    public string AppPath { get; set; } = string.Empty;

    /// <summary>The Android package name that identifies the app to open.</summary>
    public string AppPackage { get; set; } = string.Empty;

    /// <summary>The Android activity Appium opens when the app starts.</summary>
    public string AppActivity { get; set; } = string.Empty;

    /// <summary>The iOS bundle ID that identifies the app to open.</summary>
    public string BundleId { get; set; } = string.Empty;
}

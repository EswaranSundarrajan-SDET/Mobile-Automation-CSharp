namespace MobileAutomation.Configuration;

/// <summary>Identifies the device and the app that Appium should open for one platform.</summary>
public sealed class DeviceSettings
{
    public string DeviceName { get; set; } = string.Empty;

    public string PlatformVersion { get; set; } = string.Empty;

    public string AutomationName { get; set; } = string.Empty;

    public string AppPath { get; set; } = string.Empty;

    public string AppPackage { get; set; } = string.Empty;

    public string AppActivity { get; set; } = string.Empty;

    public string BundleId { get; set; } = string.Empty;
}

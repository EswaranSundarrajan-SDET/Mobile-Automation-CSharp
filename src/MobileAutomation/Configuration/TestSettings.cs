namespace MobileAutomation.Configuration;

/// <summary>Settings shared by a test run, including its platform, server and element wait timeout.</summary>
public sealed class TestSettings
{
    /// <summary>The operating system name from the settings file or environment.</summary>
    public string Platform { get; set; } = nameof(MobilePlatform.Android);

    /// <summary>The address of the Appium server used for local device runs.</summary>
    public string AppiumServerUrl { get; set; } = "http://127.0.0.1:4723";

    /// <summary>The maximum number of seconds a page waits for an element.</summary>
    public int WaitTimeoutSeconds { get; set; } = 15;

    /// <summary>Ask Appium to preserve app data between sessions.</summary>
    public bool NoReset { get; set; } = true;

    /// <summary>Cloud device and connection settings for BrowserStack runs.</summary>
    public BrowserStackSettings BrowserStack { get; set; } = new();

    /// <summary>Appium device and app settings for Android.</summary>
    public DeviceSettings Android { get; set; } = new();

    /// <summary>Appium device and app settings for iOS.</summary>
    public DeviceSettings Ios { get; set; } = new();

    /// <summary>The configured platform as a value the driver factory can use.</summary>
    public MobilePlatform TargetPlatform => Enum.TryParse<MobilePlatform>(Platform, ignoreCase: true, out var value)
        ? value
        : throw new InvalidOperationException($"Unsupported mobile platform '{Platform}'. Use Android or Ios.");

    /// <summary>The app and device settings for the platform selected for this run.</summary>
    public DeviceSettings Device => TargetPlatform == MobilePlatform.Android ? Android : Ios;

    /// <summary>The timeout converted from seconds to the duration type Selenium expects.</summary>
    public TimeSpan WaitTimeout => TimeSpan.FromSeconds(WaitTimeoutSeconds);

    /// <summary>The address DriverFactory connects to for this run.</summary>
    public string EffectiveServerUrl => BrowserStack.Enabled && !string.IsNullOrWhiteSpace(BrowserStack.HubUrl)
        ? BrowserStack.HubUrl
        : AppiumServerUrl;
}

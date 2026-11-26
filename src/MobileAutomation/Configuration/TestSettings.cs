namespace MobileAutomation.Configuration;

/// <summary>Settings shared by a test run, including its platform, server and element wait timeout.</summary>
public sealed class TestSettings
{
    public string Platform { get; set; } = nameof(MobilePlatform.Android);

    public string AppiumServerUrl { get; set; } = "http://127.0.0.1:4723";

    public int WaitTimeoutSeconds { get; set; } = 15;

    /// <summary>Ask Appium to preserve app data between sessions.</summary>
    public bool NoReset { get; set; } = true;

    public BrowserStackSettings BrowserStack { get; set; } = new();

    public DeviceSettings Android { get; set; } = new();

    public DeviceSettings Ios { get; set; } = new();

    public MobilePlatform TargetPlatform => Enum.TryParse<MobilePlatform>(Platform, ignoreCase: true, out var value)
        ? value
        : throw new InvalidOperationException($"Unsupported mobile platform '{Platform}'. Use Android or Ios.");

    public DeviceSettings Device => TargetPlatform == MobilePlatform.Android ? Android : Ios;

    public TimeSpan WaitTimeout => TimeSpan.FromSeconds(WaitTimeoutSeconds);

    /// <summary>The address DriverFactory connects to for this run.</summary>
    public string EffectiveServerUrl => BrowserStack.Enabled && !string.IsNullOrWhiteSpace(BrowserStack.HubUrl)
        ? BrowserStack.HubUrl
        : AppiumServerUrl;
}

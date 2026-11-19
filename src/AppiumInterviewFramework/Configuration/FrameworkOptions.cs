namespace AppiumInterviewFramework.Configuration;

public enum MobilePlatform
{
    Android,
    Ios
}

public sealed class FrameworkOptions
{
    public string Platform { get; set; } = nameof(MobilePlatform.Android);

    public string AppiumServerUrl { get; set; } = "http://127.0.0.1:4723";

    public int WaitTimeoutSeconds { get; set; } = 15;

    public bool NoReset { get; set; } = true;

    public BrowserStackOptions BrowserStack { get; set; } = new();

    public DeviceOptions Android { get; set; } = new();

    public DeviceOptions Ios { get; set; } = new();

    public MobilePlatform TargetPlatform => Enum.TryParse<MobilePlatform>(Platform, ignoreCase: true, out var value)
        ? value
        : throw new InvalidOperationException($"Unsupported mobile platform '{Platform}'. Use Android or Ios.");

    public DeviceOptions Device => TargetPlatform == MobilePlatform.Android ? Android : Ios;

    public TimeSpan WaitTimeout => TimeSpan.FromSeconds(WaitTimeoutSeconds);

    public string EffectiveServerUrl => BrowserStack.Enabled && !string.IsNullOrWhiteSpace(BrowserStack.HubUrl)
        ? BrowserStack.HubUrl
        : AppiumServerUrl;
}

public sealed class DeviceOptions
{
    public string DeviceName { get; set; } = string.Empty;

    public string PlatformVersion { get; set; } = string.Empty;

    public string AutomationName { get; set; } = string.Empty;

    public string AppPath { get; set; } = string.Empty;

    public string AppPackage { get; set; } = string.Empty;

    public string AppActivity { get; set; } = string.Empty;

    public string BundleId { get; set; } = string.Empty;
}

public sealed class BrowserStackOptions
{
    public bool Enabled { get; set; }

    public string HubUrl { get; set; } = "https://hub-cloud.browserstack.com/wd/hub";

    // Kept for compatibility with the first version of the sample framework.
    public string ServerUrl
    {
        get => HubUrl;
        set => HubUrl = value;
    }

    public string Username { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string AppUrl { get; set; } = string.Empty;

    public string DeviceName { get; set; } = "Google Pixel 7";

    public string OsVersion { get; set; } = "13.0";

    public string ProjectName { get; set; } = "Mobile Automation";

    public string BuildName { get; set; } = "Regression Build";

    public string SessionName { get; set; } = "Login Test";

    public bool Debug { get; set; } = true;

    public bool Video { get; set; } = true;

    public bool NetworkLogs { get; set; } = true;
}

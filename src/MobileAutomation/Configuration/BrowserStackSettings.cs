namespace MobileAutomation.Configuration;

/// <summary>Connection details, device choice and reporting options for BrowserStack App Automate.</summary>
public sealed class BrowserStackSettings
{
    /// <summary>Use a BrowserStack device when true.</summary>
    public bool Enabled { get; set; }

    /// <summary>The BrowserStack server address where the driver starts a session.</summary>
    public string HubUrl { get; set; } = "https://hub-cloud.browserstack.com/wd/hub";

    /// <summary>Older settings files may use ServerUrl as another name for HubUrl.</summary>
    public string ServerUrl
    {
        get => HubUrl;
        set => HubUrl = value;
    }

    /// <summary>The BrowserStack account name used to sign in to the service.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The secret key that allows the driver to use the BrowserStack account.</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>The reference returned by BrowserStack after you upload an app, such as bs:// followed by an app ID.</summary>
    public string AppUrl { get; set; } = string.Empty;

    /// <summary>The model of the phone or tablet to use in BrowserStack.</summary>
    public string DeviceName { get; set; } = "Google Pixel 7";

    /// <summary>The Android or iOS version to use on the selected cloud device.</summary>
    public string OsVersion { get; set; } = "13.0";

    /// <summary>The project label used to group runs in the BrowserStack dashboard.</summary>
    public string ProjectName { get; set; } = "Mobile Automation";

    /// <summary>The label used to group sessions from the same test run in BrowserStack.</summary>
    public string BuildName { get; set; } = "Regression Build";

    /// <summary>The name shown for this individual session in the BrowserStack dashboard.</summary>
    public string SessionName { get; set; } = "Login Test";

    /// <summary>Ask BrowserStack to capture debugging screenshots.</summary>
    public bool Debug { get; set; } = true;

    /// <summary>Ask BrowserStack to record a video of the test session.</summary>
    public bool Video { get; set; } = true;

    /// <summary>Ask BrowserStack to record the app's network traffic for the session.</summary>
    public bool NetworkLogs { get; set; } = true;
}

namespace MobileAutomation.Configuration;

/// <summary>Connection details, device choice and reporting options for BrowserStack App Automate.</summary>
public sealed class BrowserStackSettings
{
    public bool Enabled { get; set; }

    public string HubUrl { get; set; } = "https://hub-cloud.browserstack.com/wd/hub";

    /// <summary>Older settings files may use ServerUrl as another name for HubUrl.</summary>
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

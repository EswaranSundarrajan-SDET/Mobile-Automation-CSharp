using Microsoft.Extensions.Configuration;

namespace MobileAutomation.Configuration;

/// <summary>Reads appsettings.json and applies overrides from the current environment.</summary>
public static class TestSettingsLoader
{
    /// <summary>Load the settings used by both local and BrowserStack tests.</summary>
    public static TestSettings Load(string fileName = "appsettings.json", string? basePath = null)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath ?? AppContext.BaseDirectory)
            .AddJsonFile(fileName, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var settings = configuration.Get<TestSettings>();
        if (settings is null)
        {
            throw new InvalidOperationException("Could not read the mobile test settings.");
        }

        ApplyEnvironmentAliases(settings);
        Validate(settings);
        return settings;
    }

    /// <summary>Check the platform, wait timeout and required BrowserStack settings before starting a session.</summary>
    public static void Validate(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.WaitTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("WaitTimeoutSeconds must be greater than zero.");
        }

        _ = settings.TargetPlatform;

        if (!settings.BrowserStack.Enabled)
        {
            return;
        }

        var missing = new List<string>();
        AddMissing(missing, "BrowserStack.HubUrl", settings.BrowserStack.HubUrl);
        AddMissing(missing, "BrowserStack.Username", settings.BrowserStack.Username);
        AddMissing(missing, "BrowserStack.AccessKey", settings.BrowserStack.AccessKey);
        AddMissing(missing, "BrowserStack.AppUrl", settings.BrowserStack.AppUrl);
        AddMissing(missing, "BrowserStack.DeviceName", settings.BrowserStack.DeviceName);
        AddMissing(missing, "BrowserStack.OsVersion", settings.BrowserStack.OsVersion);
        AddMissing(missing, "BrowserStack.ProjectName", settings.BrowserStack.ProjectName);
        AddMissing(missing, "BrowserStack.BuildName", settings.BrowserStack.BuildName);
        AddMissing(missing, "BrowserStack.SessionName", settings.BrowserStack.SessionName);

        if (!Uri.TryCreate(settings.BrowserStack.HubUrl, UriKind.Absolute, out var hubUri) ||
            (hubUri.Scheme != Uri.UriSchemeHttps && hubUri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                $"BrowserStack.HubUrl must be an absolute HTTP(S) URL. Received '{settings.BrowserStack.HubUrl}'.");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"BrowserStack App Automate is enabled but required settings are missing: {string.Join(", ", missing)}. " +
                "Use appsettings.json, .NET nested variables such as BrowserStack__AccessKey, " +
                "or the documented BROWSERSTACK_* environment aliases. Never commit credentials.");
        }
    }

    private static void ApplyEnvironmentAliases(TestSettings settings)
    {
        var browserStack = settings.BrowserStack;
        browserStack.Enabled = ReadBooleanAlias("BROWSERSTACK_ENABLED", browserStack.Enabled);
        browserStack.HubUrl = ReadStringAlias("BROWSERSTACK_HUB_URL", browserStack.HubUrl);
        browserStack.Username = ReadStringAlias("BROWSERSTACK_USERNAME", browserStack.Username);
        browserStack.AccessKey = ReadStringAlias("BROWSERSTACK_ACCESS_KEY", browserStack.AccessKey);
        browserStack.AppUrl = ReadStringAlias("BROWSERSTACK_APP_URL", browserStack.AppUrl);
        browserStack.DeviceName = ReadStringAlias("BROWSERSTACK_DEVICE_NAME", browserStack.DeviceName);
        browserStack.OsVersion = ReadStringAlias("BROWSERSTACK_OS_VERSION", browserStack.OsVersion);
        browserStack.ProjectName = ReadStringAlias("BROWSERSTACK_PROJECT_NAME", browserStack.ProjectName);
        browserStack.BuildName = ReadStringAlias("BROWSERSTACK_BUILD_NAME", browserStack.BuildName);
        browserStack.SessionName = ReadStringAlias("BROWSERSTACK_SESSION_NAME", browserStack.SessionName);
        browserStack.Debug = ReadBooleanAlias("BROWSERSTACK_DEBUG", browserStack.Debug);
        browserStack.Video = ReadBooleanAlias("BROWSERSTACK_VIDEO", browserStack.Video);
        browserStack.NetworkLogs = ReadBooleanAlias("BROWSERSTACK_NETWORK_LOGS", browserStack.NetworkLogs);
    }

    private static string ReadStringAlias(string name, string currentValue) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : currentValue;

    private static bool ReadBooleanAlias(string name, bool currentValue)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return currentValue;
        }

        if (bool.TryParse(rawValue, out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException($"{name} must be 'true' or 'false'. Received '{rawValue}'.");
    }

    private static void AddMissing(ICollection<string> missing, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            missing.Add(name);
        }
    }
}

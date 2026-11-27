using Microsoft.Extensions.Configuration;

namespace MobileAutomation.Configuration;

/// <summary>Reads appsettings.json and applies overrides from the current environment.</summary>
public static class TestSettingsLoader
{
    /// <summary>Load the settings used by both local and BrowserStack tests.</summary>
    /// <param name="fileName">The JSON settings file to read.</param>
    /// <param name="basePath">The folder containing the file. If omitted, use the test output folder.</param>
    /// <returns>The checked settings after applying the environment variables.</returns>
    public static TestSettings Load(string fileName = "appsettings.json", string? basePath = null)
    {
        // Read the JSON file first so environment values can override it.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath ?? AppContext.BaseDirectory)
            .AddJsonFile(fileName, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Fill a settings object with the values read from both sources.
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
    /// <param name="settings">The settings to check before opening a device session.</param>
    public static void Validate(TestSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.WaitTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("WaitTimeoutSeconds must be greater than zero.");
        }

        // Read the platform now so an unrecognized name fails before Appium starts.
        _ = settings.TargetPlatform;

        if (!settings.BrowserStack.Enabled)
        {
            return;
        }

        // Collect the names of all blank BrowserStack settings for one error message.
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

        // Parse the hub address as an absolute URL and check that it uses HTTP or HTTPS.
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

    /// <summary>Apply the short environment variable names used by the shell and CI.</summary>
    /// <param name="settings">The settings object to update.</param>
    private static void ApplyEnvironmentAliases(TestSettings settings)
    {
        // Platform is also an MSBuild property. Give shell users a separate name for the device platform.
        settings.Platform = ReadStringAlias("MOBILE_PLATFORM", settings.Platform);

        // Use the BrowserStack section from the settings object so the overrides are saved there.
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

    /// <summary>Read a text override when the environment variable has a value.</summary>
    /// <param name="name">The environment variable to look up.</param>
    /// <param name="currentValue">The value to keep if the variable is missing or empty.</param>
    /// <returns>The override text, or the current value.</returns>
    private static string ReadStringAlias(string name, string currentValue) =>
        // The pattern captures nonempty text as the override value.
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : currentValue;

    /// <summary>Read an environment override for an on or off setting.</summary>
    /// <param name="name">The environment variable to look up.</param>
    /// <param name="currentValue">The value to keep if the variable is missing or blank.</param>
    /// <returns>The parsed true or false value, or the current value.</returns>
    private static bool ReadBooleanAlias(string name, bool currentValue)
    {
        // Read the text before trying to convert it to true or false.
        var rawValue = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return currentValue;
        }

        // Store the converted value in parsedValue only when the text is valid.
        if (bool.TryParse(rawValue, out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException($"{name} must be 'true' or 'false'. Received '{rawValue}'.");
    }

    /// <summary>Record a required setting when its value is blank.</summary>
    /// <param name="missing">The list of missing setting names collected so far.</param>
    /// <param name="name">The setting name to include in the error message.</param>
    /// <param name="value">The configured value to check.</param>
    private static void AddMissing(ICollection<string> missing, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            missing.Add(name);
        }
    }
}

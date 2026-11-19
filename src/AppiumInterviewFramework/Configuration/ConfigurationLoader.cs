using Microsoft.Extensions.Configuration;

namespace AppiumInterviewFramework.Configuration;

public static class ConfigurationLoader
{
    public static FrameworkOptions Load(string fileName = "appsettings.json", string? basePath = null)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath ?? AppContext.BaseDirectory)
            .AddJsonFile(fileName, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var options = configuration.Get<FrameworkOptions>();
        if (options is null)
        {
            throw new InvalidOperationException("Could not bind FrameworkOptions from configuration.");
        }

        ApplyEnvironmentAliases(options);
        Validate(options);
        return options;
    }

    public static void Validate(FrameworkOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.WaitTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("WaitTimeoutSeconds must be greater than zero.");
        }

        _ = options.TargetPlatform;

        if (!options.BrowserStack.Enabled)
        {
            return;
        }

        var missing = new List<string>();
        AddMissing(missing, "BrowserStack.HubUrl", options.BrowserStack.HubUrl);
        AddMissing(missing, "BrowserStack.Username", options.BrowserStack.Username);
        AddMissing(missing, "BrowserStack.AccessKey", options.BrowserStack.AccessKey);
        AddMissing(missing, "BrowserStack.AppUrl", options.BrowserStack.AppUrl);
        AddMissing(missing, "BrowserStack.DeviceName", options.BrowserStack.DeviceName);
        AddMissing(missing, "BrowserStack.OsVersion", options.BrowserStack.OsVersion);
        AddMissing(missing, "BrowserStack.ProjectName", options.BrowserStack.ProjectName);
        AddMissing(missing, "BrowserStack.BuildName", options.BrowserStack.BuildName);
        AddMissing(missing, "BrowserStack.SessionName", options.BrowserStack.SessionName);

        if (!Uri.TryCreate(options.BrowserStack.HubUrl, UriKind.Absolute, out var hubUri) ||
            (hubUri.Scheme != Uri.UriSchemeHttps && hubUri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                $"BrowserStack.HubUrl must be an absolute HTTP(S) URL. Received '{options.BrowserStack.HubUrl}'.");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"BrowserStack App Automate is enabled but required settings are missing: {string.Join(", ", missing)}. " +
                "Use appsettings.json, .NET nested variables such as BrowserStack__AccessKey, " +
                "or the documented BROWSERSTACK_* environment aliases. Never commit credentials.");
        }
    }

    private static void ApplyEnvironmentAliases(FrameworkOptions options)
    {
        var browserStack = options.BrowserStack;
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

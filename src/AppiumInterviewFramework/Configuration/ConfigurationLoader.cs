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

        if (options.WaitTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("WaitTimeoutSeconds must be greater than zero.");
        }

        _ = options.TargetPlatform;

        if (options.BrowserStack.Enabled &&
            (string.IsNullOrWhiteSpace(options.BrowserStack.Username) ||
             string.IsNullOrWhiteSpace(options.BrowserStack.AccessKey)))
        {
            throw new InvalidOperationException(
                "BrowserStack is enabled but BrowserStack username/access key are missing. Supply them through protected environment variables.");
        }

        return options;
    }
}

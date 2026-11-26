using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace MobileAutomation.Reporting;

/// <summary>Paths to the files that NUnit can attach to a failed test.</summary>
public sealed record FailureReportFiles(string LogPath, string? ScreenshotPath);

/// <summary>Saves the test name, time and available driver details before the device session closes.</summary>
public static class FailureReport
{
    public static FailureReportFiles Save(IWebDriver driver, string directory, string testName)
    {
        ArgumentNullException.ThrowIfNull(driver);
        Directory.CreateDirectory(directory);

        var safeName = Regex.Replace(testName, "[^a-zA-Z0-9_.-]", "_");
        var filePrefix = Path.Combine(directory, $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}");
        var logPath = filePrefix + ".log";
        var details = new List<string>
        {
            $"UTC: {DateTime.UtcNow:O}",
            $"Test: {testName}"
        };

        // Web views can supply a URL and title. Some native screens support neither command.
        AddDriverDetail(details, "Current URL", () => driver.Url);
        AddDriverDetail(details, "Title", () => driver.Title);

        string? screenshotPath = null;
        if (driver is ITakesScreenshot camera)
        {
            try
            {
                camera.GetScreenshot().SaveAsFile(filePrefix + ".png");
                screenshotPath = filePrefix + ".png";
            }
            catch (WebDriverException error)
            {
                details.Add($"Screenshot unavailable: {error.Message}");
            }
        }

        File.WriteAllLines(logPath, details);
        return new FailureReportFiles(logPath, screenshotPath);
    }

    private static void AddDriverDetail(List<string> details, string name, Func<string> readValue)
    {
        try
        {
            details.Add($"{name}: {readValue()}");
        }
        catch (Exception error) when (error is WebDriverException or NotImplementedException or NotSupportedException)
        {
            details.Add($"{name}: unavailable");
        }
    }
}

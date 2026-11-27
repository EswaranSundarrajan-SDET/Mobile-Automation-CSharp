using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace MobileAutomation.Reporting;

/// <summary>Paths to the files that NUnit can attach to a failed test.</summary>
/// <param name="LogPath">The path to the text log saved for the failed test.</param>
/// <param name="ScreenshotPath">The path to the saved screenshot, or null when no screenshot was saved.</param>
public sealed record FailureReportFiles(string LogPath, string? ScreenshotPath);

/// <summary>Saves the test name, time and available driver details before the device session closes.</summary>
public static class FailureReport
{
    /// <summary>Save the details available from the driver after a test fails.</summary>
    /// <param name="driver">The driver for the current Appium session.</param>
    /// <param name="directory">The folder where the log and screenshot should be saved.</param>
    /// <param name="testName">The name that identifies the failed test in the log and filenames.</param>
    /// <returns>The log path and, when available, the screenshot path.</returns>
    public static FailureReportFiles Save(IWebDriver driver, string directory, string testName)
    {
        ArgumentNullException.ThrowIfNull(driver);
        Directory.CreateDirectory(directory);

        // Replace characters in the test name that are unsuitable for filenames.
        var safeName = Regex.Replace(testName, "[^a-zA-Z0-9_.-]", "_");
        // Use the same test name and UTC timestamp for both report files.
        var filePrefix = Path.Combine(directory, $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}");
        // Add the log extension to get the path for the text report.
        var logPath = filePrefix + ".log";
        // Start the report with the current UTC time and the failed test's name.
        var details = new List<string>
        {
            $"UTC: {DateTime.UtcNow:O}",
            $"Test: {testName}"
        };

        // Web views can supply a URL and title. Some native screens support neither command.
        AddDriverDetail(details, "Current URL", () => driver.Url);
        AddDriverDetail(details, "Title", () => driver.Title);

        // Leave the path empty until a screenshot is saved.
        string? screenshotPath = null;
        // Use camera to take a screenshot when the driver supports it.
        if (driver is ITakesScreenshot camera)
        {
            try
            {
                camera.GetScreenshot().SaveAsFile(filePrefix + ".png");
                screenshotPath = filePrefix + ".png";
            }
            // Include the driver's screenshot error in the text log so the reason is visible.
            catch (WebDriverException error)
            {
                details.Add($"Screenshot unavailable: {error.Message}");
            }
        }

        File.WriteAllLines(logPath, details);
        return new FailureReportFiles(logPath, screenshotPath);
    }

    /// <summary>Try to add a driver detail to the log. Record that it is unavailable if the driver cannot provide it.</summary>
    /// <param name="details">The list of lines that will be written to the log.</param>
    /// <param name="name">The label to place next to the detail.</param>
    /// <param name="readValue">The function that asks the driver for the current value.</param>
    private static void AddDriverDetail(List<string> details, string name, Func<string> readValue)
    {
        try
        {
            details.Add($"{name}: {readValue()}");
        }
        // Treat commands that fail or are unsupported as unavailable details.
        catch (Exception error) when (error is WebDriverException or NotImplementedException or NotSupportedException)
        {
            details.Add($"{name}: unavailable");
        }
    }
}

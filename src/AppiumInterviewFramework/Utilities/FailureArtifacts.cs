using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace AppiumInterviewFramework.Utilities;

public sealed record FailureArtifactResult(string? ScreenshotPath, string LogPath);

public static class FailureArtifacts
{
    public static FailureArtifactResult Capture(IWebDriver driver, string directory, string testName)
    {
        ArgumentNullException.ThrowIfNull(driver);
        Directory.CreateDirectory(directory);

        var safeName = Regex.Replace(testName, "[^a-zA-Z0-9_.-]", "_");
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");
        var screenshotPath = Path.Combine(directory, $"{safeName}_{stamp}.png");
        var logPath = Path.Combine(directory, $"{safeName}_{stamp}.log");

        string? savedScreenshot = null;
        if (driver is ITakesScreenshot screenshotDriver)
        {
            screenshotDriver.GetScreenshot().SaveAsFile(screenshotPath);
            savedScreenshot = screenshotPath;
        }

        var log = $"UTC: {DateTime.UtcNow:O}{Environment.NewLine}" +
                  $"Test: {testName}{Environment.NewLine}" +
                  $"Current URL: {driver.Url}{Environment.NewLine}" +
                  $"Title: {driver.Title}{Environment.NewLine}";
        File.WriteAllText(logPath, log);

        return new FailureArtifactResult(savedScreenshot, logPath);
    }
}

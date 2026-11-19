using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace AppiumInterviewFramework.Utilities;

public static class GestureActions
{
    public static void Swipe(AppiumDriver driver, ScreenPoint start, ScreenPoint end, TimeSpan? duration = null)
    {
        ArgumentNullException.ThrowIfNull(driver);

        var touch = new PointerInputDevice(PointerKind.Touch);
        var sequence = new ActionSequence(touch, 0);
        var swipeDuration = duration ?? TimeSpan.FromMilliseconds(500);

        sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, start.X, start.Y, TimeSpan.Zero));
        sequence.AddAction(touch.CreatePointerDown(MouseButton.Touch));
        sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, end.X, end.Y, swipeDuration));
        sequence.AddAction(touch.CreatePointerUp(MouseButton.Touch));

        driver.PerformActions(new[] { sequence });
    }
}

public readonly record struct ScreenPoint(int X, int Y);

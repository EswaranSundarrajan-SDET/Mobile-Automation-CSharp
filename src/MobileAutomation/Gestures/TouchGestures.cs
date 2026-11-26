using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace MobileAutomation.Gestures;

/// <summary>Performs touch gestures using Selenium pointer actions.</summary>
public static class TouchGestures
{
    /// <summary>Press at the start point, move to the end point, then lift the finger. The default swipe takes half a second.</summary>
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

/// <summary>A point in the viewport measured from its top-left corner.</summary>
public readonly record struct ScreenPoint(int X, int Y);

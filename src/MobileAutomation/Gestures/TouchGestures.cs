using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace MobileAutomation.Gestures;

/// <summary>Performs touch gestures using Selenium pointer actions.</summary>
public static class TouchGestures
{
    /// <summary>Press at the start point, move to the end point, then lift the finger. The default swipe takes half a second.</summary>
    /// <param name="driver">The active Appium driver that sends the touch actions to the device.</param>
    /// <param name="start">The screen position where the finger first touches down.</param>
    /// <param name="end">The screen position where the finger lifts.</param>
    /// <param name="duration">The time the finger spends moving. Leave it empty for a 500 ms swipe.</param>
    public static void Swipe(AppiumDriver driver, ScreenPoint start, ScreenPoint end, TimeSpan? duration = null)
    {
        ArgumentNullException.ThrowIfNull(driver);

        // Create a virtual finger for the touch actions.
        var touch = new PointerInputDevice(PointerKind.Touch);
        // Keep each finger movement in the order Appium should perform it.
        var sequence = new ActionSequence(touch, 0);
        // Use the default movement time if the caller leaves it empty.
        var swipeDuration = duration ?? TimeSpan.FromMilliseconds(500);

        // Position the finger, press, move while pressed, then lift.
        sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, start.X, start.Y, TimeSpan.Zero));
        sequence.AddAction(touch.CreatePointerDown(MouseButton.Touch));
        sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, end.X, end.Y, swipeDuration));
        sequence.AddAction(touch.CreatePointerUp(MouseButton.Touch));

        driver.PerformActions(new[] { sequence });
    }
}

/// <summary>A point in the viewport measured from its top-left corner.</summary>
/// <param name="X">The horizontal distance from the left edge.</param>
/// <param name="Y">The vertical distance from the top edge.</param>
public readonly record struct ScreenPoint(int X, int Y);

# Mobile Automation in C#

This .NET 8 framework uses NUnit and Appium to test native Android and iOS apps. If you have used Selenium, start with the login test. Each screen has a page object, each test gets a driver, and the test owns the assertion.

## Follow one test

The example signs in and checks that the home screen appears. These three lines come from `LoginTests`.

```csharp
var loginPage = new LoginPage(Driver, WaitTimeout);
var homePage = loginPage.SignIn(username, password);

Assert.That(homePage.IsDisplayed(), Is.True, "The home screen should appear after sign-in.");
```

1. NUnit calls `BaseTest.StartDriver`. It reads the settings and asks `DriverFactory` to start an Android or iOS session.
2. `LoginPage.SignIn` fills in the username and password and taps the sign-in button. The shared actions in `BasePage` use Selenium's `WebDriverWait`.
3. `SignIn` returns a `HomePage` using the same driver. `HomePage.IsDisplayed()` waits for an element unique to that screen. The NUnit test checks the result.
4. NUnit calls `BaseTest.StopDriver`. On failure it tries to attach a screenshot and a text log, then closes the session.

## Where to make a change

| When you need to… | Open |
| --- | --- |
| Add a scenario or read the login example | [LoginTests.cs](tests/MobileAutomation.Tests/LoginTests.cs) |
| Change what happens before or after each mobile test | [BaseTest.cs](tests/MobileAutomation.Tests/BaseTest.cs) |
| Update a field, button or screen identifier | [LoginPage.cs](src/MobileAutomation/Pages/LoginPage.cs) or [HomePage.cs](src/MobileAutomation/Pages/HomePage.cs) |
| Reuse a click, text entry or explicit wait | [BasePage.cs](src/MobileAutomation/Pages/BasePage.cs) |
| Change how Android, iOS or BrowserStack sessions start | [DriverFactory.cs](src/MobileAutomation/Drivers/DriverFactory.cs) |
| Set the platform, app, device, server or element timeout | [appsettings.json](tests/MobileAutomation.Tests/appsettings.json) |
| See how settings and environment overrides are read | [TestSettingsLoader.cs](src/MobileAutomation/Configuration/TestSettingsLoader.cs) |
| Perform a touch swipe between two screen coordinates | [TouchGestures.cs](src/MobileAutomation/Gestures/TouchGestures.cs) |
| Change the files attached to a failed test | [FailureReport.cs](src/MobileAutomation/Reporting/FailureReport.cs) |
| See the checks that run in GitHub Actions | [ci.yml](.github/workflows/ci.yml) |

## Run the checks without a device

Open `MobileAutomation.sln` in Visual Studio, or run these commands from the repository root with the .NET 8 SDK installed.

```powershell
dotnet restore MobileAutomation.sln
dotnet build MobileAutomation.sln --configuration Release --no-restore
dotnet test MobileAutomation.sln --configuration Release --no-build --filter "Category=BuildValidation"
```

The configuration tests check local defaults, platform selection, required BrowserStack settings and environment overrides. A regular `dotnet test` also discovers the login test. It is skipped until you enable mobile execution.

## Run the login test on a local device

1. Start an Appium server with the driver for your platform. For Android, use UiAutomator2 with an emulator or connected device. For iOS, use XCUITest with Xcode on a Mac and a simulator or connected device.
2. Point the test at your app. Set the device and app in `appsettings.json` or override them in the test shell as shown below. Use a path to a build on the Appium host, or identify an installed app using `Android__AppPackage` and `Android__AppActivity`, or `Ios__BundleId`.
3. Match the sample accessibility IDs to your app. `LoginPage` uses `login_username`, `login_password` and `login_button`. `HomePage` uses `home_screen`. Start with the app signed out. The default `NoReset=true` can preserve a previous login.
4. Set `DEMO_APP_USERNAME` and `DEMO_APP_PASSWORD` in the test process using your local secret store or CI secrets.

For example, with Appium listening at the default address `http://127.0.0.1:4723`, run the following in a second PowerShell window. Replace the device name and app path with your own.

```powershell
$env:MOBILE_PLATFORM = "Android"
$env:Android__DeviceName = "emulator-5554"
$env:Android__AppPath = "C:\apps\your-app.apk"
$env:RUN_APPIUM_TESTS = "true"
dotnet test MobileAutomation.sln --filter "Category=Mobile"
```

Use `MOBILE_PLATFORM` in the shell so the mobile platform choice does not interfere with the .NET build. For iOS, set `MOBILE_PLATFORM=Ios`, `Ios__DeviceName` and `Ios__AppPath` or `Ios__BundleId`. Set `Ios__PlatformVersion` if needed. `AppiumServerUrl` overrides the local server address. `WaitTimeoutSeconds` controls how long a page will wait for an element (15 seconds by default).

## Run the same test on BrowserStack

Upload your app to BrowserStack App Automate and copy the `app_url` from the upload response. The driver factory uses it to open the app on the cloud device.

| Setting | Value |
| --- | --- |
| `RUN_APPIUM_TESTS`, `BROWSERSTACK_ENABLED` | Set both to `true` for a cloud run from your shell. |
| `BROWSERSTACK_USERNAME`, `BROWSERSTACK_ACCESS_KEY` | BrowserStack credentials from your secret store. |
| `BROWSERSTACK_APP_URL` | Uploaded app reference, such as `bs://...`. |
| `BROWSERSTACK_DEVICE_NAME`, `BROWSERSTACK_OS_VERSION` | Choose an available device and OS. The defaults are Google Pixel 7 and Android 13.0. For iOS, also set `MOBILE_PLATFORM=Ios`. |
| `DEMO_APP_USERNAME`, `DEMO_APP_PASSWORD` | Credentials for the app's login screen. |

Run `dotnet test MobileAutomation.sln --filter "Category=Mobile"`. You can override the hub, project, build and session names with `BROWSERSTACK_HUB_URL`, `BROWSERSTACK_PROJECT_NAME`, `BROWSERSTACK_BUILD_NAME` and `BROWSERSTACK_SESSION_NAME`. The capture switches are `BROWSERSTACK_DEBUG`, `BROWSERSTACK_VIDEO` and `BROWSERSTACK_NETWORK_LOGS`. Each defaults to `true`.

The loader reads JSON first, then .NET environment variables such as `BrowserStack__AccessKey`, then the corresponding nonempty `BROWSERSTACK_*` overrides. Existing settings files that use `BrowserStack.ServerUrl` are also supported.

## Add a screen and a test

1. Add a class under `src/MobileAutomation/Pages` named after the screen, such as `ProfilePage`. Inherit from `BasePage` and pass the driver and timeout to its constructor as in `HomePage`.
2. Put the screen's `By` locators in that class. Expose methods such as `SaveProfile()` that use `Click` and `EnterText`. When an action opens another screen, return its page object with the same driver and timeout.
3. Add a test class under `tests/MobileAutomation.Tests`. Inherit from `BaseTest`, apply `[Category("Mobile")]`, call the page methods, and use NUnit to assert the visible result.

## GitHub Actions

| Job | When it runs | What it needs |
| --- | --- | --- |
| `build-validation` | Pull requests, pushes to main or master, and manual runs | A GitHub-hosted .NET runner. It builds the solution and runs the device-free tests. |
| `mobile-smoke` | Repository variable `RUN_APPIUM_TESTS=true` | A self-hosted runner with a running Appium server, a configured device and app, and the two app-login secrets. |
| `browserstack-smoke` | Repository variable `RUN_BROWSERSTACK_TESTS=true` or manual input `run_browserstack=true` | The BrowserStack username and access key and the two app-login values in GitHub Secrets. Put the uploaded app reference in the `BROWSERSTACK_APP_URL` repository variable. |

The BrowserStack job publishes the TRX results and available failure files as `browserstack-smoke-results`. For a local failure, look in the test run's `artifacts` directory or open the attachments in the test result.

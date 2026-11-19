# C# / .NET Appium interview framework

An intentionally small but credible mobile automation framework for demonstrating C#, .NET, NUnit, Appium, and practical test-framework design in an interview. It is runnable once a real application, device/emulator, and Appium server are supplied.

## What is included

- .NET 8 solution with a reusable class library and NUnit test project.
- Appium .NET client 5.0.0 with Android (`UiAutomator2`) and iOS (`XCUITest`) driver creation.
- `appsettings.json` configuration with environment-variable overrides.
- Page Object Model with a sample `LoginPage`.
- Centralized explicit waits for visible and clickable elements.
- W3C touch swipe helper using Selenium action sequences.
- NUnit setup/teardown with failure screenshots and a small diagnostic log.
- Build-validation test that does not require a device, plus a realistic mobile login test.
- GitHub Actions workflow for build validation and optional self-hosted mobile smoke execution.

## Architecture

```text
src/AppiumInterviewFramework
├── Configuration       FrameworkOptions and configuration binding
├── Drivers             Android/iOS Appium driver factory
├── Pages               BasePage and LoginPage objects
└── Utilities           Waits, W3C gestures, failure artifacts

tests/AppiumInterviewFramework.Tests
├── Tests/AppiumTestBase.cs       lifecycle and failure handling
├── Tests/ConfigurationTests.cs   device-free validation
├── Tests/LoginTests.cs           sample mobile login scenario
└── appsettings.json              non-secret local defaults
```

The test depends on the class library, so page objects and utilities can be reused by additional test suites without coupling them to NUnit.

## Prerequisites

For build validation:

- .NET 8 SDK or a later SDK that can target `net8.0`.

For an actual mobile run:

- Node.js and Appium 2: `npm install -g appium`.
- The matching Appium driver: `appium driver install uiautomator2` for Android or `appium driver install xcuitest` for iOS.
- Android SDK, an emulator/USB device, and `adb` for Android; or macOS/Xcode and a simulator/device for iOS.
- A testable application build and the app's accessibility IDs. The sample IDs are deliberately placeholders.

## Configure the framework

Edit `tests/AppiumInterviewFramework.Tests/appsettings.json`, or override values through environment variables. Empty app/device fields are intentional so the repository contains no app-specific paths or secrets.

Android example settings:

```json
{
  "Platform": "Android",
  "AppiumServerUrl": "http://127.0.0.1:4723",
  "Android": {
    "DeviceName": "emulator-5554",
    "PlatformVersion": "14",
    "AutomationName": "UiAutomator2",
    "AppPath": "C:/absolute/path/to/app.apk",
    "AppPackage": "com.example.app",
    "AppActivity": "com.example.app.MainActivity"
  }
}
```

For iOS, set `Platform` to `Ios`, use `XCUITest`, and provide `AppPath` or `BundleId` under `Ios`. The framework only adds non-empty capabilities, which supports either installing an app from a path or launching an already-installed app by package/bundle identifier.

The configuration loader also reads environment variables using .NET's double-underscore convention, for example `Android__DeviceName`, `Android__AppPath`, or `AppiumServerUrl`. Keep usernames and passwords out of JSON; the sample test reads `DEMO_APP_USERNAME` and `DEMO_APP_PASSWORD` from the process environment.

### Optional BrowserStack execution

The same driver factory can target BrowserStack App Automate by setting `BrowserStack__Enabled=true`. Supply `BrowserStack__Username`, `BrowserStack__AccessKey`, and `BrowserStack__App` through the CI secret/environment layer; the checked-in JSON intentionally leaves those values empty. The factory sends the standard `bstack:options` block and uses the configured BrowserStack hub URL. BrowserStack App Live is useful for manual exploratory/device debugging, while App Automate is the Appium execution service used by this framework.

This is an integration-ready path, not a claim that a cloud session was run from this repository. The actual app upload identifier, device matrix, account, and CI secrets belong in the team's protected BrowserStack configuration.

The placeholder locators in `LoginPage` are:

- `login_username`
- `login_password`
- `login_button`
- `home_screen`

Replace them with the accessibility IDs exposed by the real app under test.

## Run locally

From the repository root:

```powershell
dotnet restore AppiumInterviewFramework.sln
dotnet build AppiumInterviewFramework.sln --configuration Release
dotnet test AppiumInterviewFramework.sln --filter Category=BuildValidation
```

The mobile test is skipped unless explicitly enabled, so a clean checkout can be built and validated without a device:

```powershell
$env:RUN_APPIUM_TESTS = "true"
$env:DEMO_APP_USERNAME = "value-from-your-local-secret-store"
$env:DEMO_APP_PASSWORD = "value-from-your-local-secret-store"
appium
dotnet test AppiumInterviewFramework.sln --filter Category=Mobile
```

The Appium server, emulator/device, app path/package, and the four placeholder accessibility IDs must match the application before the login test can pass. Never commit the local credential values.

## Run in CI

The included workflow always restores, builds, and runs the device-free `BuildValidation` category on a GitHub-hosted runner. The `mobile-smoke` job is opt-in through the repository variable `RUN_APPIUM_TESTS=true` and expects a self-hosted runner that already has Appium, the platform SDK, a connected device/emulator, and the configured app.

Store `DEMO_APP_USERNAME` and `DEMO_APP_PASSWORD` as CI secrets. Put device/app configuration in runner environment variables or a protected CI configuration layer; do not place credentials in `appsettings.json` or source control.

For larger suites, the next CI improvements would be a device matrix, per-worker Appium ports, test-result publishing, and artifact upload for `TestResults` and the generated failure artifacts.

## Interview talking points

- The driver factory isolates platform-specific capabilities and returns the common `AppiumDriver` abstraction to tests.
- Configuration is strongly typed and environment-overridable, while credentials come from the environment rather than source control.
- Page objects expose business actions such as `Login`, not raw locator mechanics to the test.
- Explicit waits are centralized and ignore transient missing/stale element conditions; fixed sleeps are avoided.
- Failure handling is in teardown so screenshots and a diagnostic log are captured consistently for failed tests.
- Gestures use W3C pointer actions, which are the cross-platform interaction model supported by modern Selenium/Appium clients.
- The sample test is intentionally gated by `RUN_APPIUM_TESTS`, making build validation deterministic when no device is available.
- BrowserStack App Automate is an optional remote endpoint; App Live is a complementary manual debugging tool, not a replacement for automated CI.

## Practical questions and model answers

### Why use a driver factory instead of creating `AndroidDriver` in each test?

It gives one place to build capabilities, choose Android versus iOS, configure the Appium endpoint, and apply shared timeouts. Tests depend on the common `AppiumDriver` contract and do not repeat platform setup.

### Why is `MobileBy.AccessibilityId` preferred in the page object?

Accessibility IDs are usually more stable and faster than long XPath expressions, and they encourage the product team to expose a useful automation/accessibility contract. If an element has no suitable ID, a platform-specific selector can be isolated inside the page object.

### Why use explicit waits instead of `Thread.Sleep`?

An explicit wait polls until the condition is true or the configured timeout expires. It reduces unnecessary delay and makes failures explainable, while fixed sleeps are either too short for a slow device or waste time on a fast one.

### What is the difference between `noReset` and a clean test?

`noReset=true` preserves application state between sessions and can speed up a smoke run. It can also create test-order or stale-data problems. A clean suite should control state deliberately, using reset/clear-data or a test API where the product supports it.

### Why are gestures implemented with W3C actions?

W3C actions describe input devices and their ordered pointer events. That is more portable than relying on deprecated JSON Wire or driver-specific gesture shortcuts, and it makes the swipe duration and touch contact explicit.

### How would you make this suite parallel-safe?

Use one isolated Appium session per test worker, assign unique devices and ports, avoid shared mutable test data, and write artifacts to worker-specific directories. The current sample keeps the lifecycle simple; parallelism should be enabled only after those resources are isolated.

### How would you handle secrets in a real pipeline?

Keep secrets in the CI provider's secret store or a vault, inject them as environment variables at runtime, and avoid logging their values. This sample demonstrates that boundary with `DEMO_APP_USERNAME` and `DEMO_APP_PASSWORD`.

### How would you diagnose a failure on a remote device?

The teardown captures a screenshot and a log containing the test name, current URL, and title. In CI I would also publish those files, the NUnit/TRX result, Appium server logs, device logs, and the tested app version as artifacts.

### How would you run this on BrowserStack?

I would enable the `BrowserStack` configuration section, inject the username and access key as CI secrets, provide the uploaded app identifier, and keep the same page objects and tests. I would use a device/OS matrix for coverage and publish BrowserStack session links alongside the NUnit results. App Live would be used for interactive investigation when a cloud device needs manual inspection.

### Why does the repository have both build-validation and mobile categories?

Compilation/configuration checks should be fast and deterministic on every pull request. Device tests require external infrastructure, so the `Mobile` category is opt-in and runs on a runner that has the emulator/device and Appium dependencies.

### What would you add for production scale?

More screen objects, test-data builders, a reporting layer, structured logs, retry policy only for infrastructure failures, API-based state setup, device matrices, and contract tests that verify required accessibility IDs before running the full suite.

### How does this fit an Azure DevOps pipeline?

The same `dotnet restore`, `dotnet build`, and `dotnet test --filter` commands can be placed in Azure DevOps tasks. Secrets should come from variable groups or Key Vault, the mobile job should run on a self-hosted agent with Appium and a device/cloud endpoint, and screenshots, logs, and test results should be published as pipeline artifacts.

## Scope and honest limitations

This repository intentionally does not pretend to know a real application's package name, app binary path, credentials, accessibility IDs, or device inventory. Those values are placeholders to be supplied by the team that owns the app. The framework and build-validation test are verifiable locally; the end-to-end login flow requires the real external mobile environment.

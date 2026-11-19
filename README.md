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
- GitHub Actions workflow for build validation, optional self-hosted mobile smoke execution, and an opt-in BrowserStack App Automate smoke job.

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

### BrowserStack App Automate execution

The same driver factory can target BrowserStack App Automate by setting `BROWSERSTACK_ENABLED=true`. The checked-in JSON contains safe defaults only; username, access key, and the uploaded app URL must be supplied at runtime. The framework uses Android by default with Appium .NET v5 typed standard capabilities for platform, automation name, device, OS version, and app, plus the W3C `bstack:options` extension for BrowserStack metadata and optional diagnostics.

When BrowserStack is enabled, provide these required values. Device, OS version, metadata, and diagnostics have safe defaults and can be overridden when needed:

| Setting | Purpose |
| --- | --- |
| `BROWSERSTACK_USERNAME` | BrowserStack account username, stored as a secret |
| `BROWSERSTACK_ACCESS_KEY` | BrowserStack access key, stored as a secret |
| `BROWSERSTACK_APP_URL` | The `app_url` value returned by the BrowserStack upload response |
| `BROWSERSTACK_DEVICE_NAME` | Optional override; defaults to `Google Pixel 7` |
| `BROWSERSTACK_OS_VERSION` | Optional override; defaults to `13.0` |
| `BROWSERSTACK_HUB_URL` | Appium hub URL; defaults to `https://hub-cloud.browserstack.com/wd/hub` |
| `BROWSERSTACK_PROJECT_NAME` | BrowserStack project grouping |
| `BROWSERSTACK_BUILD_NAME` | Build grouping for dashboards |
| `BROWSERSTACK_SESSION_NAME` | Human-readable session name |

Diagnostic settings are `BROWSERSTACK_DEBUG`, `BROWSERSTACK_VIDEO`, and `BROWSERSTACK_NETWORK_LOGS`; they default to `true` for this interview smoke configuration and can be set to `false`. The same settings can be expressed with .NET nested names such as `BrowserStack__AccessKey`; the uppercase `BROWSERSTACK_*` aliases are convenient for local shells and CI.

#### Upload an app and obtain its app URL

Upload the APK or IPA through the BrowserStack App Automate upload API. The response contains an `app_url` value; copy that value into the `BROWSERSTACK_APP_URL` environment variable or GitHub repository variable. Do not commit the binary, app ID, or credentials.

```bash
curl -u "${BROWSERSTACK_USERNAME}:${BROWSERSTACK_ACCESS_KEY}" \
  -X POST "https://api-cloud.browserstack.com/app-automate/upload" \
  -F "file=@/absolute/path/to/app.apk"
```

For a PowerShell session, the equivalent is:

```powershell
$pair = "$env:BROWSERSTACK_USERNAME`:$env:BROWSERSTACK_ACCESS_KEY"
curl.exe --user $pair --request POST `
  --form "file=@C:\absolute\path\to\app.apk" `
  https://api-cloud.browserstack.com/app-automate/upload
```

#### Run the BrowserStack smoke test locally

```powershell
$env:RUN_APPIUM_TESTS = "true"
$env:BROWSERSTACK_ENABLED = "true"
$env:BROWSERSTACK_USERNAME = "value-from-your-secret-store"
$env:BROWSERSTACK_ACCESS_KEY = "value-from-your-secret-store"
$env:BROWSERSTACK_APP_URL = "value-returned-by-browserstack-upload"
# Device, OS, project, build, session, and diagnostics use safe defaults unless overridden.
$env:DEMO_APP_USERNAME = "value-from-your-secret-store"
$env:DEMO_APP_PASSWORD = "value-from-your-secret-store"

dotnet test AppiumInterviewFramework.sln --filter Category=Mobile
```

The app URL must be the value returned by BrowserStack's upload response, and the default device/OS must be available in the BrowserStack catalog. The sample login locators remain placeholders and must be replaced with the real app's accessibility IDs.

#### App Live versus App Automate

BrowserStack App Automate is the remote execution service used by this framework. It runs Appium tests on real devices and provides session logs, screenshots, video, and optional network logs. BrowserStack App Live is an interactive manual device session used to explore the app, reproduce a problem, inspect behavior, or confirm a locator. App Live does not replace automated CI execution.

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

The `browserstack-smoke` job is also opt-in. Enable it by setting the repository variable `RUN_BROWSERSTACK_TESTS=true` or by manually starting the workflow with the `run_browserstack` input.

In the GitHub repository, open **Settings → Secrets and variables → Actions**. Under **Repository secrets**, select **New repository secret** and add these exact names without placing their values in source control:

- `BROWSERSTACK_USERNAME`
- `BROWSERSTACK_ACCESS_KEY`
- `DEMO_APP_USERNAME`
- `DEMO_APP_PASSWORD`

Under **Repository variables**, select **New repository variable** and add:

- `BROWSERSTACK_APP_URL`
- Optional overrides: `BROWSERSTACK_DEVICE_NAME`, `BROWSERSTACK_OS_VERSION`, `BROWSERSTACK_HUB_URL`, `BROWSERSTACK_PROJECT_NAME`, `BROWSERSTACK_BUILD_NAME`, `BROWSERSTACK_SESSION_NAME`, `BROWSERSTACK_DEBUG`, `BROWSERSTACK_VIDEO`, and `BROWSERSTACK_NETWORK_LOGS`.

The workflow reads `BROWSERSTACK_USERNAME`, `BROWSERSTACK_ACCESS_KEY`, `DEMO_APP_USERNAME`, and `DEMO_APP_PASSWORD` only from GitHub Secrets. It reads the uploaded app URL and optional execution settings from GitHub Variables. Never paste a credential, access key, or real app ID into `appsettings.json`, README examples, workflow YAML, or commits.

The job validates required BrowserStack inputs before starting a remote session, runs the `Mobile` category, and publishes test results and failure artifacts when available. BrowserStack does not require a local Appium server in this job because the driver connects directly to the cloud hub.

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

I would upload the APK or IPA, set the returned `bs://` app URL, inject the username and access key as CI secrets, configure the device and OS version, and keep the same page objects and tests. The driver uses typed Appium capabilities plus a W3C `bstack:options` block for project, build, session, and diagnostic settings. I would use a device/OS matrix for coverage and publish BrowserStack session links alongside the NUnit results. App Live would be used for interactive investigation when a cloud device needs manual inspection.

### Why does the repository have both build-validation and mobile categories?

Compilation/configuration checks should be fast and deterministic on every pull request. Device tests require external infrastructure, so the `Mobile` category is opt-in and runs on a runner that has the emulator/device and Appium dependencies.

### What would you add for production scale?

More screen objects, test-data builders, a reporting layer, structured logs, retry policy only for infrastructure failures, API-based state setup, device matrices, and contract tests that verify required accessibility IDs before running the full suite.

### How does this fit an Azure DevOps pipeline?

The same `dotnet restore`, `dotnet build`, and `dotnet test --filter` commands can be placed in Azure DevOps tasks. Secrets should come from variable groups or Key Vault, the mobile job should run on a self-hosted agent with Appium and a device/cloud endpoint, and screenshots, logs, and test results should be published as pipeline artifacts.

## Scope and honest limitations

This repository intentionally does not pretend to know a real application's package name, app binary path, credentials, accessibility IDs, or device inventory. Those values are placeholders to be supplied by the team that owns the app. The framework and build-validation test are verifiable locally; the end-to-end login flow requires the real external mobile environment.

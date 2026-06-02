# BayBrain Customization Guide

BayBrain is a .NET 8 WPF desktop app. The fastest places to customize it are:

## Visual Theme

- `Themes/DarkTheme.xaml`
  - Colors, brushes, fonts, TextBox, ComboBox, Button, ListBox, and shared control styling.
  - Edit the `Color` keys near the top first, then shared styles below.

- `Themes/Animations.xaml`
  - Reusable WPF animations and motion polish.

## Screens

- `Views/MainWindow.xaml`
  - Main shell, navigation, search page, and status bar.

- `Views/ROView.xaml`
  - Repair Order Mode layout and vehicle/customer input form.

- `Views/ServiceBrowserView.xaml`
  - Service catalog screen.

- `Views/AdvisorProfileView.xaml`
  - Advisor profiles, leaderboard, and session history.

- `Views/HistoryView.xaml`
  - Saved repair order history.

- `Views/SettingsView.xaml`
  - Shop settings and data statistics.

## Screen Logic

- `ViewModels/MainViewModel.cs`
  - App navigation, global search, scripts, dashboard refresh wiring.

- `ViewModels/ROViewModel.cs`
  - RO Mode inputs, preset year/make/model choices, analyze/save/reset behavior.

- `ViewModels/QuizViewModel.cs`
  - Quiz flow and scoring.

- `ViewModels/AdvisorProfileViewModel.cs`
  - Advisor training profiles, leaderboard, and guarded active-advisor selection.

- `Services/AuthService.cs`
  - Local user accounts, roles, PIN hashing, and first-run admin bootstrap.

## Editable Data

- `Data/services.json`
  - Service catalog: names, categories, descriptions, urgency, prices, tags, intervals, failure consequences.

- `Data/quiz.json`
  - Quiz questions, answers, explanations, difficulty, categories.

Runtime files are created beside the executable:

- `users.json`
  - Local login accounts. First run creates `admin` / PIN `0000`.
  - Roles: `Admin`, `Manager`, `Advisor`.

- `advisor_profiles.json`
  - Advisor training profiles and quiz history. Advisor user accounts can be linked to these profiles in Settings.

- `repair_orders.json`
  - Saved repair orders and recommendation approval history.

- `settings.json`
  - Dealership name, behavior defaults, export folder, and app preferences.

After editing JSON, rerun the app. Catalog files are copied to `bin/Debug/net8.0-windows/Data`; runtime files are written to `bin/Debug/net8.0-windows`.

## First Login

On a fresh install, BayBrain creates one local admin account:

- User: `Administrator`
- Username: `admin`
- PIN: `0000`

Use Settings → Account Management to create manager/advisor accounts. Advisor accounts should be linked to an advisor profile so quiz sessions and ROs attach to the right person.

## Build And Run

```powershell
cd C:\Users\zackc\OneDrive\Desktop\BayBrain
& "C:\Program Files\dotnet\dotnet.exe" run --project BayBrain.csproj
```

Direct executable:

```powershell
C:\Users\zackc\OneDrive\Desktop\BayBrain\bin\Debug\net8.0-windows\BayBrain.exe
```

## Git Hygiene

`bin/` and `obj/` are generated build folders and are ignored by Git. Commit source files, XAML, project files, and JSON data.

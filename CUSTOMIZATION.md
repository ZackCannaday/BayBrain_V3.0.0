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
  - Profiles, XP, levels, leaderboard, active advisor.

## Editable Data

- `Data/services.json`
  - Service catalog: names, categories, descriptions, urgency, prices, tags, intervals, failure consequences.

- `Data/quiz.json`
  - Quiz questions, answers, explanations, difficulty, categories.

After editing JSON, rebuild or rerun the app. The files are copied to `bin/Debug/net8.0-windows/Data`.

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


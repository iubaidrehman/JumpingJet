# JumpingJet

A lightweight, native .NET 10 WinForms utility designed to prevent Windows from locking, sleeping, or triggering "Away" statuses in enterprise communication tools (Microsoft Teams, Slack, Zoom).

Unlike basic scripts that only move the mouse, this tool employs a dual-layered approach to ensure continuous active session retention.

## How It Works
1. **Kernel-Level Execution State:** Leverages the Win32 `SetThreadExecutionState` API (`ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED`) to instruct the OS to keep the display on and prevent system sleep.
2. **Hardware Input Spoofing:** Defeats Electron-based application idle-timers (which often ignore the Win32 API) by injecting a 1-pixel cursor displacement and an `F15` keystroke at user-defined intervals, resetting the `GetLastInputInfo` timer.

## Features
* **Unobtrusive UX:** Runs silently in the system tray. Minimizes to the notification area to keep your taskbar clean.
* **Configurable Intervals:** Adjust the frequency of the physical hardware jiggle from 5 to 600 seconds.
* **Execution Modes:** Toggle between strict API-level sleep prevention and physical cursor movement.
* **Self-Contained:** Distributed as a single compiled executable. No .NET 10 installation or SDK required on the target machine.

## Installation & Usage
1. Go to the [Releases](../../releases) section on the right side of this repository.
2. Download the latest `JumpingJet.exe`.
3. Run the executable (no installation required).
4. Configure your interval and click **Start System Keep-Awake**.
5. The application minimizes to the System Tray. Double-click the tray icon to restore the UI, or right-click to pause/exit.

## How to Build (For Developers)

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Windows environment (requires WinForms and Win32 API access)

### Option 1: Automated Build Script
Run the included PowerShell script to compile a highly compressed, self-contained single file:
```powershell
.\build.ps1

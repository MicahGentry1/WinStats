# WinStats

A lightweight CLI system monitor for Windows, written in C#/.NET. Reads live stats directly via native Win32 APIs — no external dependencies, no background services.

```
=== WinStats ===
Uptime:  22.6 hours
Cores:   8
Memory:  12908 / 31385 MB
Battery: 99%
```

> **Platform support:** Windows shows every statistic, including battery status through native Win32 APIs. Linux shows uptime, CPU core count, and memory usage; battery status is unavailable. macOS is not currently supported.

## Features

- **Uptime** — via `GetTickCount64`
- **CPU core count** — via `Environment.ProcessorCount`
- **Memory usage** — used/total, via `GlobalMemoryStatusEx`
- **Battery percentage** — via `GetSystemPowerStatus`, color-coded (green normally, red below 20%). Shows `0%` on desktops with no battery.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or later

Windows 10/11 or Linux is supported. The standalone executable command below targets Windows.

## Usage

Clone and run:

```powershell
git clone https://github.com/MicahGentry1/WinStats.git
cd WinStats
dotnet run
```

By default it refreshes every 2 seconds until stopped (`Ctrl+C`).

For a single snapshot instead of a live loop:

```powershell
dotnet run -- --once
```

## Building a standalone .exe

To produce a self-contained executable that doesn't require .NET installed on the target machine:

```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

## How it works

On Windows, the program calls Win32 APIs directly through C#'s P/Invoke (`[DllImport]`) mechanism. On Linux, it reads memory information from `/proc/meminfo`:

| Stat | API |
|---|---|
| Uptime | `Environment.TickCount64` |
| Memory | `GlobalMemoryStatusEx` (kernel32.dll) |
| CPU cores | `Environment.ProcessorCount` |
| Battery | `GetSystemPowerStatus` (kernel32.dll) |

## License
The MIT License (MIT)

Copyright (c) 2011-2026 The Bootstrap Authors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

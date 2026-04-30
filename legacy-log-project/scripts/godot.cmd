@echo off
setlocal enabledelayedexpansion

REM Godot headless wrapper for Windows shells (cmd / PowerShell / Explorer).
REM Counterpart of scripts/godot.sh; needed because Windows does not
REM execute .sh files natively.
REM
REM Resolution order:
REM   1. GODOT_BIN env var (used as-is)
REM   2. Auto-discover the latest Godot Mono build under the standard
REM      winget package directory.

if defined GODOT_BIN (
    "%GODOT_BIN%" %*
    exit /b %ERRORLEVEL%
)

set "BASE=%LOCALAPPDATA%\Microsoft\WinGet\Packages"
set "GODOT_EXE="

for /d %%P in ("%BASE%\GodotEngine.GodotEngine.Mono_*") do (
    for /d %%V in ("%%P\Godot_*-stable_mono_win64") do (
        if exist "%%V\%%~nxV.exe" set "GODOT_EXE=%%V\%%~nxV.exe"
    )
)

if not defined GODOT_EXE (
    echo Godot Mono build not found.>&2
    echo   1^) winget install --id GodotEngine.GodotEngine.Mono -e>&2
    echo   2^) Or set GODOT_BIN to the absolute path of the Godot exe.>&2
    exit /b 1
)

"%GODOT_EXE%" %*
exit /b %ERRORLEVEL%

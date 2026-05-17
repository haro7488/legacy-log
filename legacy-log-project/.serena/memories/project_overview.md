# LegacyLog project overview

- Working directory: `D:\Project\Godot\legacy-log\legacy-log-project`.
- This is the implementation-side Godot project for LegacyLog, a Godot C# game experiment. The repository root (`D:\Project\Godot\legacy-log`) is also an Obsidian vault for experiment docs.
- Current implementation project state is very early scaffold: Godot project config, C# solution/project, icon assets, and Godot wrapper scripts exist; there are no tracked C# gameplay source files, scenes, resources, JSON data files, or tests yet.
- Godot config: `project.godot` names the app `LegacyLog`, uses Godot 4.6 C# features, GL Compatibility renderer, D3D12 on Windows, Jolt 3D physics, and assembly name `LegacyLog`.
- C# config: `LegacyLog.csproj` uses `Godot.NET.Sdk/4.6.2`, `net8.0` normally, `net9.0` for Android target, and `EnableDynamicLoading=true`.
- Tracked implementation files: `.claude/settings.json`, `.editorconfig`, `.gitattributes`, `.gitignore`, `CLAUDE.md`, `LegacyLog.csproj`, `LegacyLog.sln`, icon files, `project.godot`, `scripts/godot.cmd`, `scripts/godot.sh`.
- Root docs currently include `docs/decisions/001-commit-convention.md` and `docs/experiment-log/2026-04-10-initial-setup.md`; `docs/current-focus.md` was not present during onboarding.
- Important local discrepancy: `AGENTS.md` was provided in chat context but no `legacy-log-project/AGENTS.md` file exists in the working directory. The implementation rules are currently in `CLAUDE.md`.
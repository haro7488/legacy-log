# Suggested commands

Use PowerShell from `D:\Project\Godot\legacy-log\legacy-log-project` unless noted.

- List files: `Get-ChildItem -Force`
- Recursive file list: `Get-ChildItem -Recurse -File`
- Read UTF-8 file: `Get-Content -Raw -Encoding UTF8 <path>`
- Git status despite sandbox dubious ownership: `git -c safe.directory=D:/Project/Godot/legacy-log status --short`
- Recent git history: `git -c safe.directory=D:/Project/Godot/legacy-log log --oneline --decorate -5`
- List tracked files: `git -c safe.directory=D:/Project/Godot/legacy-log ls-files`
- Check .NET SDK: `dotnet --version` or `dotnet --list-sdks`
- Build C# project: `dotnet build`
- Godot headless import: `.\scripts\godot.cmd --headless --import`
- Godot version through wrapper: `.\scripts\godot.cmd --headless --version`
- Open Godot editor through wrapper: `.\scripts\godot.cmd -e`

Notes observed during onboarding:
- `rg` was not installed in the current environment, so use PowerShell recursive search/listing if unavailable.
- `dotnet build` succeeded with zero warnings/errors.
- `.\scripts\godot.cmd --headless --version` failed in the Codex sandbox because Godot Mono was not found and `GODOT_BIN` was not set. This may differ on the user's normal Windows account.
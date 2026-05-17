# Task completion checklist

- For C# or project configuration changes, run `dotnet build` from `legacy-log-project`.
- After adding Godot scripts, scenes, resources, or assets, run the Godot wrapper import once: `.\scripts\godot.cmd --headless --import`, then run `dotnet build`.
- Use the Godot wrapper scripts only; do not invoke a winget Godot symlink directly.
- Check git status with `git -c safe.directory=D:/Project/Godot/legacy-log status --short` because the sandbox user may trigger dubious ownership warnings.
- Do not commit automatically unless the user asks. If committing, follow the Conventional Commits convention in `docs/decisions/001-commit-convention.md`.
- Preserve unrelated user changes. Do not revert or clean untracked files unless explicitly requested.
- If Godot wrapper fails because Godot Mono is not discoverable, report that clearly and mention `GODOT_BIN` or Godot Mono installation as the likely fix.
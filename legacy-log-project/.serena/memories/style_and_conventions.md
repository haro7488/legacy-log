# Style and conventions

- User-facing answers must be in Korean. Technical artifacts such as code, commit messages, and PR titles may be in English.
- Follow the user's AGENTS-style constraints: state assumptions, keep changes simple, make surgical edits, avoid speculative features, and verify goals.
- Implementation project rules from `CLAUDE.md`: this directory is the implementation agent's scope; the root/meta agent docs are not used for implementation work.
- Encoding: `.editorconfig` sets UTF-8 for all files. `.gitattributes` normalizes text to LF, except `*.cmd` and `*.bat` must be CRLF because Windows cmd parsing depends on it.
- Git commit convention from root docs: Conventional Commits format `<type>: <short description>`. Types include `feat`, `fix`, `docs`, `chore`, `experiment`, and `refactor`. Description may be Korean or English, no trailing period.
- Godot execution convention: never call the winget `WinGet/Links/godot.exe` symlink directly. Use `scripts/godot.cmd` in Windows shells or `scripts/godot.sh` in POSIX/Git Bash. Both prefer `GODOT_BIN` if set, otherwise auto-discover the winget Godot Mono install.
- `.gitignore` excludes Godot cache `.godot/`, Android output, Rider `.idea/`, .NET build outputs, Godot C# temp outputs, build/export directories, local `.claude/settings.local.json`, and editor backup files.
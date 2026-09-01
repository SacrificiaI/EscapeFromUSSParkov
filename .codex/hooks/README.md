# Codex and Claude lifecycle hooks

`hooks.json` uses the Git root at runtime, so this `.codex` directory can be
copied to another Git repository without path edits. Codex on Windows uses the
PowerShell handlers through `commandWindows`; Bash handlers remain available to
Claude and non-Windows environments.

| Handler | Event | Purpose |
| --- | --- | --- |
| `pre-bash-guard` | PreToolUse (`Bash`) | Blocks destructive Git commands and unsafe `rm -rf`. |
| `post-edit-format` | PostToolUse (`Edit|Write`) | Formats existing touched C# files without blocking the edit. |
| `post-scaffold-restore` | PostToolUse (`Edit|Write`) | Restores only after a `.csproj` change, without blocking the edit. |

The post-edit handlers read both `tool_input.file_path` and `apply_patch`
headers in `tool_input.command`.

#!/usr/bin/env bash
set -euo pipefail

payload="${CLAUDE_TOOL_INPUT:-}"
if [[ -z "$payload" && ! -t 0 ]]; then payload=$(cat); fi

if command -v jq >/dev/null 2>&1; then
  command=$(printf '%s' "$payload" | jq -r '.tool_input.command // empty' 2>/dev/null || true)
else
  command=$(printf '%s' "$payload" | sed -n 's/.*"command"[[:space:]]*:[[:space:]]*"\(\(\\.\|[^"\\]\)*\)".*/\1/p' | head -1 || true)
fi

if [[ "$command" =~ git[[:space:]]+push.*(--force|-f) ]] || [[ "$command" =~ git[[:space:]]+reset[[:space:]]+--hard ]] || [[ "$command" =~ git[[:space:]]+clean[[:space:]]+-[a-zA-Z]*f ]] || [[ "$command" =~ git[[:space:]]+checkout[[:space:]]+\. ]]; then
  echo 'Destructive Git command blocked; discuss it with the user first.' >&2
  exit 2
fi

if [[ "$command" =~ rm[[:space:]]+-[a-zA-Z]*r[a-zA-Z]*f|rm[[:space:]]+-[a-zA-Z]*f[a-zA-Z]*r ]] && ! [[ "$command" =~ (node_modules|bin|obj|TestResults|\.vs|/tmp) ]]; then
  echo 'Unsafe rm -rf blocked; verify the target path is intentional.' >&2
  exit 2
fi

if [[ "$command" =~ dotnet[[:space:]]+run ]]; then echo 'WARNING: dotnet run detected. Verify the launch profile.'; fi

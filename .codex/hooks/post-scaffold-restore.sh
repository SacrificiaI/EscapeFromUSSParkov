#!/usr/bin/env bash
set -euo pipefail

payload=$(cat 2>/dev/null || true)
if command -v jq >/dev/null 2>&1; then
  files=$(printf '%s' "$payload" | jq -r '(.tool_input.file_path // empty), (.tool_input.command // empty | split("\n")[] | select(test("^\\*\\*\\* (Add|Update) File: ")) | sub("^\\*\\*\\* (Add|Update) File: "; ""))' 2>/dev/null || true)
else
  files=$(printf '%s' "$payload" | sed -n -e 's/.*"file_path"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' -e 's/^\*\*\* \(Add\|Update\) File: //p')
fi

grep -q '\.csproj$' <<< "$files" || exit 0
root=$(git rev-parse --show-toplevel 2>/dev/null || true)
[[ -z "$root" ]] && exit 0
(cd "$root" && dotnet restore --verbosity quiet 2>/dev/null) || true

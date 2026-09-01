#!/usr/bin/env bash
set -euo pipefail

payload=$(cat 2>/dev/null || true)
if command -v jq >/dev/null 2>&1; then
  files=$(printf '%s' "$payload" | jq -r '(.tool_input.file_path // empty), (.tool_input.command // empty | split("\n")[] | select(test("^\\*\\*\\* (Add|Update) File: ")) | sub("^\\*\\*\\* (Add|Update) File: "; ""))' 2>/dev/null || true)
else
  files=$(printf '%s' "$payload" | sed -n -e 's/.*"file_path"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' -e 's/^\*\*\* \(Add\|Update\) File: //p')
fi

root=$(git rev-parse --show-toplevel 2>/dev/null || true)
[[ -z "$root" ]] && exit 0
while IFS= read -r file; do
  [[ "$file" == *.cs ]] || continue
  [[ "$file" = /* ]] || file="$root/$file"
  [[ -f "$file" ]] || continue
  dir=$(dirname "$file")
  while [[ "$dir" != "/" ]]; do
    scope=$(find "$dir" -maxdepth 1 -type f \( -name '*.csproj' -o -name '*.sln' -o -name '*.slnx' \) -print -quit 2>/dev/null || true)
    [[ -n "$scope" ]] && { dotnet format "$scope" --include "$file" --no-restore 2>/dev/null || true; break; }
    parent=$(dirname "$dir"); [[ "$parent" == "$dir" ]] && break; dir="$parent"
  done
done <<< "$files"

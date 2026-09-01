[CmdletBinding()]
param()

$payload = [Console]::In.ReadToEnd()

try { $command = ($payload | ConvertFrom-Json -ErrorAction Stop).tool_input.command }
catch { exit 0 }

if ([string]::IsNullOrWhiteSpace($command)) { exit 0 }

$blockedPatterns = @(
    @{ Pattern = '(?i)\bgit\s+push\s+.*(?:--force|-f)(?:\s|$)'; Reason = 'Force push detected. Use a regular push or discuss it with the user first.' },
    @{ Pattern = '(?i)\bgit\s+reset\s+--hard\b'; Reason = 'git reset --hard discards uncommitted changes. Discuss it with the user first.' },
    @{ Pattern = '(?i)\bgit\s+clean\s+-[a-z]*f'; Reason = 'git clean -f permanently deletes untracked files. Discuss it with the user first.' },
    @{ Pattern = '(?i)\bgit\s+checkout\s+\.'; Reason = 'git checkout . discards unstaged changes. Discuss it with the user first.' }
)

foreach ($blocked in $blockedPatterns) {
    if ($command -match $blocked.Pattern) {
        @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; permissionDecision = 'deny'; permissionDecisionReason = $blocked.Reason } } | ConvertTo-Json -Compress
        exit 0
    }
}

if ($command -match '(?i)\brm\s+-[a-z]*r[a-z]*f\b|\brm\s+-[a-z]*f[a-z]*r\b') {
    $safeRemoval = $command -match '(?i)\brm\s+-[a-z]+\s+(?:\./)?(?:[^\s/]+/)*(?:node_modules|bin|obj|TestResults|\.vs)/?(?:\s|$)' -or
        $command -match '(?i)\brm\s+-[a-z]+\s+/tmp(?:/[^\s]*)?(?:\s|$)'

    if (-not $safeRemoval) {
        @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; permissionDecision = 'deny'; permissionDecisionReason = 'rm -rf detected outside an approved build-artifact or /tmp path. Verify the target path is intentional.' } } | ConvertTo-Json -Compress
        exit 0
    }
}

if ($command -match '(?i)\bdotnet\s+run\b') {
    @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; additionalContext = 'dotnet run detected. Ensure launchSettings.json exists and the correct profile is selected.' } } | ConvertTo-Json -Compress
}

exit 0

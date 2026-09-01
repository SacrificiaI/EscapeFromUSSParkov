[CmdletBinding()]
param()

function Get-TouchedFilePaths {
    param([object]$ToolInput)

    $paths = [System.Collections.Generic.List[string]]::new()
    if ($ToolInput.PSObject.Properties.Name -contains 'file_path' -and $ToolInput.file_path) { $paths.Add([string]$ToolInput.file_path) }
    if ($ToolInput.PSObject.Properties.Name -contains 'command' -and $ToolInput.command) {
        [regex]::Matches([string]$ToolInput.command, '(?m)^\*\*\* (?:Add|Update) File: (.+)$') |
            ForEach-Object { $paths.Add($_.Groups[1].Value.Trim()) }
    }

    return $paths | Select-Object -Unique
}

$root = git rev-parse --show-toplevel 2>$null
if ([string]::IsNullOrWhiteSpace($root)) { exit 0 }

$payload = [Console]::In.ReadToEnd()
try { $toolInput = ($payload | ConvertFrom-Json -ErrorAction Stop).tool_input }
catch { exit 0 }

foreach ($file in (Get-TouchedFilePaths $toolInput | Where-Object { $_ -like '*.cs' })) {
    $path = if ([System.IO.Path]::IsPathRooted($file)) { $file } else { Join-Path $root $file }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }

    $directory = Split-Path -Parent $path
    while (-not [string]::IsNullOrEmpty($directory)) {
        $scope = Get-ChildItem -LiteralPath $directory -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Extension -in '.csproj', '.sln', '.slnx' } |
            Sort-Object @{ Expression = { if ($_.Extension -eq '.csproj') { 0 } else { 1 } } } |
            Select-Object -First 1
        if ($null -ne $scope) {
            try { & dotnet format $scope.FullName --include $path --no-restore 2>$null } catch { }
            break
        }

        $parent = Split-Path -Parent $directory
        if ($parent -eq $directory) { break }
        $directory = $parent
    }
}

exit 0

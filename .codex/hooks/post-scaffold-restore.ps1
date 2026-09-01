[CmdletBinding()]
param()

$payload = [Console]::In.ReadToEnd()
try { $toolInput = ($payload | ConvertFrom-Json -ErrorAction Stop).tool_input }
catch { exit 0 }

$paths = [System.Collections.Generic.List[string]]::new()
if ($toolInput.PSObject.Properties.Name -contains 'file_path' -and $toolInput.file_path) { $paths.Add([string]$toolInput.file_path) }
if ($toolInput.PSObject.Properties.Name -contains 'command' -and $toolInput.command) {
    [regex]::Matches([string]$toolInput.command, '(?m)^\*\*\* (?:Add|Update) File: (.+)$') |
        ForEach-Object { $paths.Add($_.Groups[1].Value.Trim()) }
}

if (-not ($paths | Where-Object { $_ -like '*.csproj' })) { exit 0 }

$root = git rev-parse --show-toplevel 2>$null
if ([string]::IsNullOrWhiteSpace($root)) { exit 0 }

Push-Location $root
try { & dotnet restore --verbosity quiet 2>$null } catch { }
finally { Pop-Location }

exit 0

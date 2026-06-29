param(
    [switch]$cli,
    [switch]$ui
)

# If no arguments were passed, deploy both
if (-not ($cli -or $ui)) {
    $cli = $true
    $ui = $true
}

# --- CLI Deploy ---
if ($cli) {
    Write-Host "`nDeploying CLI app" -ForegroundColor Cyan

    Remove-Item .\pub\ -Recurse -ErrorAction SilentlyContinue
    dotnet publish .\src\PersonalFinance.CLI\PersonalFinance.CLI.csproj -c Release -o pub -v quiet

    $cliTarget = "$env:APPDATA\utils\fin.exe"
    New-Item -ItemType Directory -Force -Path (Split-Path $cliTarget) | Out-Null
    Copy-Item -Path .\pub\PersonalFinance.CLI.exe -Destination $cliTarget -Verbose
}

# --- UI Deploy ---
if ($ui) {
    Write-Host "`nDeploying MAUI app" -ForegroundColor Cyan

    $localApp = "$env:LOCALAPPDATA\PersonalFinance"
    Get-Process -Name "PersonalFinance.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
    Remove-Item $localApp -Recurse -ErrorAction SilentlyContinue
    dotnet publish .\src\PersonalFinance.UI\PersonalFinance.UI.csproj -f net10.0-windows10.0.19041.0 -c Release -o $localApp -v quiet

    $desktop = [Environment]::GetFolderPath('Desktop')
    $shortcutPath = Join-Path $desktop 'PersonalFinance.lnk'
    Write-Host "Creating shortcut at $shortcutPath"

    if (-not (Test-Path $desktop)) {
        New-Item -ItemType Directory -Path $desktop | Out-Null
    }

    if (Test-Path $shortcutPath) {
        Remove-Item $shortcutPath -Force
    }

    $exePath = "$localApp\PersonalFinance.UI.exe"
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = Split-Path $exePath
    $Shortcut.IconLocation = $exePath
    $Shortcut.Description = "Launch PersonalFinance"
    $Shortcut.Save()

    Write-Host "Shortcut created successfully!" -ForegroundColor Green
}

Write-Host "`nDeployment complete!" -ForegroundColor Green

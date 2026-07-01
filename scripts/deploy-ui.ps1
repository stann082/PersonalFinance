Write-Host "`nDeploying MAUI app" -ForegroundColor Cyan

$localApp = "$env:LOCALAPPDATA\PersonalFinance"
Get-Process -Name "PersonalFinance.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
Remove-Item $localApp -Recurse -ErrorAction SilentlyContinue
dotnet publish .\src\PersonalFinance.UI\PersonalFinance.UI.csproj -f net10.0-windows10.0.19041.0 -c Release -o $localApp -v quiet

$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop 'PersonalFinance.lnk'
Write-Host "Creating shortcut at $shortcutPath"

if (Test-Path $shortcutPath) {
    Remove-Item $shortcutPath -Force
}

$exePath = "$localApp\PersonalFinance.UI.exe"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = $exePath
$Shortcut.WorkingDirectory = Split-Path $exePath
$Shortcut.IconLocation = "$exePath,0"
$Shortcut.Description = "Launch PersonalFinance"
$Shortcut.Save()

$sig = '[DllImport("shell32.dll")] public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);'
Add-Type -MemberDefinition $sig -Name WinAPI -Namespace Shell -ErrorAction SilentlyContinue
[Shell.WinAPI]::SHChangeNotify(0x8000000, 0, [IntPtr]::Zero, [IntPtr]::Zero)

Write-Host "Shortcut created successfully!" -ForegroundColor Green
Write-Host "`nDeployment complete!" -ForegroundColor Green

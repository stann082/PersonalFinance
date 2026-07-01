Write-Host "`nDeploying CLI app" -ForegroundColor Cyan

Remove-Item .\pub\ -Recurse -ErrorAction SilentlyContinue
dotnet publish .\src\PersonalFinance.CLI\PersonalFinance.CLI.csproj -c Release -o pub -v quiet

$cliTarget = "$env:APPDATA\utils\fin.exe"
New-Item -ItemType Directory -Force -Path (Split-Path $cliTarget) | Out-Null
Copy-Item -Path .\pub\PersonalFinance.CLI.exe -Destination $cliTarget -Force
Write-Host "Installed to $cliTarget" -ForegroundColor Green

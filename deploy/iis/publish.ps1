$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '../..')
$out = Join-Path $root 'artifacts/iis'
New-Item -ItemType Directory -Force -Path $out | Out-Null
foreach ($service in @('Tasks.Api', 'Skins.Api', 'Users.Api')) {
    dotnet publish (Join-Path $root "server/services/$service/$service.csproj") -c Release -r win-x64 --self-contained false -o (Join-Path $out $service)
    if ($LASTEXITCODE -ne 0) { throw "Publish failed: $service" }
}
Push-Location (Join-Path $root 'client')
npm ci
if ($LASTEXITCODE -ne 0) { throw 'npm ci failed' }
npm run build
if ($LASTEXITCODE -ne 0) { throw 'Angular build failed' }
Pop-Location
$clientOutput = Join-Path $root 'client/dist/todo-ui/browser'
if (-not (Test-Path $clientOutput)) { $clientOutput = Join-Path $root 'client/dist/todo-ui' }
Copy-Item -Path $clientOutput -Destination (Join-Path $out 'client') -Recurse -Force
Write-Host "IIS publish output: $out"

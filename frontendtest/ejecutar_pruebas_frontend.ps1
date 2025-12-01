Write-Host "=========================================="
Write-Host "    EJECUTANDO PRUEBAS DEL FRONTEND"
Write-Host "=========================================="
Write-Host ""

# Guardamos la ubicación actual
$currentDir = Get-Location
# Navegamos a la carpeta del frontend usando PSScriptRoot
$rootPath = Split-Path $PSScriptRoot -Parent
$frontendPath = Join-Path $rootPath "frontend"
Set-Location $frontendPath

try {
    # Ejecutamos ng test en modo CI (sin watch)
    # Intentamos usar ChromeHeadless si está disponible, sino Chrome normal
    npm run test -- --watch=false --browsers=ChromeHeadless
} catch {
    Write-Host "⚠️  No se pudo ejecutar ChromeHeadless, intentando con Chrome normal..."
    npm run test -- --watch=false
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Pruebas del Frontend completadas con ÉXITO." -ForegroundColor Green
} else {
    Write-Host "❌ Fallaron las pruebas del Frontend." -ForegroundColor Red
}

# Regresamos a la carpeta original
Set-Location $currentDir

Write-Host "=========================================="
Write-Host "    EJECUTANDO PRUEBAS DEL BACKEND"
Write-Host "=========================================="
Write-Host ""

# Ajustamos la ruta relativa usando PSScriptRoot para que funcione desde cualquier ubicación
$rootPath = Split-Path $PSScriptRoot -Parent
$testProject = Join-Path $rootPath "backend.Tests/backend.Tests.csproj"

if (Test-Path $testProject) {
    dotnet test $testProject
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Pruebas del Backend completadas con ÉXITO." -ForegroundColor Green
    } else {
        Write-Host "❌ Fallaron las pruebas del Backend." -ForegroundColor Red
    }
} else {
    Write-Host "❌ No se encontró el proyecto de pruebas en $testProject" -ForegroundColor Red
}

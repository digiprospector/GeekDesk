param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$msbuild = & 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' `
    -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1

if (-not $msbuild) {
    Write-Error '未找到 MSBuild，请确认已安装 Visual Studio。'
    exit 1
}

$running = Get-Process -Name 'GeekDesk' -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "正在结束运行中的 GeekDesk 进程 (PID: $($running.Id)) ..."
    $running | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

# 直接透传 MSBuild 输出（编码由控制台处理）。
$projectPath = Join-Path $PSScriptRoot 'GeekDesk.csproj'
& $msbuild $projectPath /p:Configuration=$Configuration /restore /v:m
$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Error "构建失败（退出码: $exitCode）"
}
exit $exitCode

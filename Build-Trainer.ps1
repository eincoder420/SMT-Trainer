# Build-Trainer.ps1
# Builds SMT-Trainer: the payload first (net472, against the game's Mono assemblies),
# then the injector, which embeds the payload as a resource.
# Output: .\publish\SamanthaTrainer.exe
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File Build-Trainer.ps1
#
# The game is located automatically on the first run and the assemblies the payload needs
# are copied into lib\. Every build after that is self-contained. Only pass -GameDir if
# auto-detection cannot find your install.

param(
    [string]$GameDir
)

$ErrorActionPreference = "Stop"

$PayloadProj = Join-Path $PSScriptRoot "Payload\SamanthaTrainer.Payload.csproj"
$InjectorProj = Join-Path $PSScriptRoot "Injector\SamanthaTrainer.Injector.csproj"
$OutputDir    = Join-Path $PSScriptRoot "publish"

Write-Host ""
Write-Host "  SMT TRAINER - Build Script" -ForegroundColor Magenta
Write-Host "  --------------------------" -ForegroundColor DarkGray
Write-Host ""

# The payload is compiled against the game's assemblies. They are copied into lib\ once, so
# the game only has to be found a single time - after that the build is self-contained.
$LibDir    = Join-Path $PSScriptRoot "lib"
$PropsFile = Join-Path $PSScriptRoot "GameDir.props"

$RequiredDlls = @(
    "mscorlib.dll", "System.dll", "System.Core.dll", "System.Xml.dll",
    "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.IMGUIModule.dll",
    "UnityEngine.InputLegacyModule.dll", "UnityEngine.TextRenderingModule.dll",
    "UnityEngine.UI.dll", "UnityEngine.UIModule.dll", "UnityEngine.PhysicsModule.dll",
    "UnityEngine.AnimationModule.dll", "UnityEngine.ImageConversionModule.dll",
    "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "Newtonsoft.Json.dll"
)

function Test-GameDir($dir) {
    if (-not $dir) { return $false }
    return Test-Path (Join-Path $dir "Samantha_Data\Managed\Assembly-CSharp.dll")
}

function Find-GameDir {
    # 1. explicit argument
    if (Test-GameDir $GameDir) { return $GameDir }

    # 2. remembered from a previous run
    if (Test-Path $PropsFile) {
        $saved = ([xml](Get-Content $PropsFile)).Project.PropertyGroup.GameDir
        if (Test-GameDir $saved) { return $saved }
    }

    # 3. environment override
    if (Test-GameDir $env:SAMANTHA_GAME_DIR) { return $env:SAMANTHA_GAME_DIR }

    # 4. the game is running right now - most reliable, and free
    $proc = Get-Process -Name "Samantha" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($proc) {
        try {
            $dir = Split-Path $proc.MainModule.FileName -Parent
            if (Test-GameDir $dir) { return $dir }
        } catch { }
    }

    # 5. look through the usual places
    Write-Host "  [*] Looking for the game..." -ForegroundColor DarkYellow
    $roots = @(
        (Join-Path $env:USERPROFILE "Downloads"),
        (Join-Path $env:USERPROFILE "Desktop"),
        (Join-Path $env:USERPROFILE "Documents"),
        "C:\Games", "D:\Games", "E:\Games",
        "C:\Program Files (x86)\Steam\steamapps\common",
        "D:\SteamLibrary\steamapps\common"
    ) | Where-Object { $_ -and (Test-Path $_) }

    foreach ($root in $roots) {
        $hit = Get-ChildItem -Path $root -Filter "Samantha.exe" -Recurse -Depth 4 `
                             -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($hit -and (Test-GameDir $hit.DirectoryName)) { return $hit.DirectoryName }
    }

    return $null
}

# lib\ alone is enough once it has been populated.
$LibReady = Test-Path (Join-Path $LibDir "Assembly-CSharp.dll")

if (-not $LibReady) {
    $GameDir = Find-GameDir

    if (-not $GameDir) {
        Write-Host ""
        Write-Host "  BUILD FAILED - could not find the game." -ForegroundColor Red
        Write-Host ""
        Write-Host "  The payload needs the game's assemblies to compile against. They are" -ForegroundColor DarkGray
        Write-Host "  copied into lib\ once and then never needed again." -ForegroundColor DarkGray
        Write-Host "  Start the game, or point at it explicitly:" -ForegroundColor DarkGray
        Write-Host "    .\Build-Trainer.ps1 -GameDir 'X:\path\to\Samantha_v211_beta'" -ForegroundColor White
        Write-Host ""
        exit 1
    }

    Write-Host "  [*] Game found: $GameDir" -ForegroundColor DarkGray
    Write-Host "  [*] Copying reference assemblies into lib\ ..." -ForegroundColor DarkYellow

    $GameManaged = Join-Path $GameDir "Samantha_Data\Managed"
    New-Item -ItemType Directory -Force -Path $LibDir | Out-Null

    foreach ($dll in $RequiredDlls) {
        $src = Join-Path $GameManaged $dll
        if (Test-Path $src) {
            Copy-Item $src -Destination $LibDir -Force
        } else {
            Write-Host "  [!] Missing from the game: $dll" -ForegroundColor Red
            exit 1
        }
    }

    # Remember where it came from, for refreshing lib\ after a game update.
    @"
<Project>
  <PropertyGroup>
    <GameDir>$GameDir</GameDir>
  </PropertyGroup>
</Project>
"@ | Set-Content -Path $PropsFile -Encoding UTF8

    Write-Host "  [*] lib\ ready - the game is not needed for future builds." -ForegroundColor DarkGray
}
else {
    Write-Host "  [*] Using reference assemblies from lib\" -ForegroundColor DarkGray
}

# Terminate any running trainer to release the lock on publish\SamanthaTrainer.exe.
# The trainer requests elevation, so a non-elevated shell cannot kill it - detect that and
# say so plainly, otherwise the publish step fails with an opaque GenerateBundle error.
$running = Get-Process -Name "SamanthaTrainer" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "  [*] Closing running trainer..." -ForegroundColor DarkYellow
    $running | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500

    if (Get-Process -Name "SamanthaTrainer" -ErrorAction SilentlyContinue) {
        Write-Host ""
        Write-Host "  BUILD BLOCKED - SamanthaTrainer.exe is still running." -ForegroundColor Red
        Write-Host "  It runs as Administrator, so this shell cannot close it." -ForegroundColor DarkGray
        Write-Host "  Close the trainer window and run this script again." -ForegroundColor DarkGray
        Write-Host ""
        exit 1
    }
}

# Clean old output
if (Test-Path $OutputDir) {
    Write-Host "  [*] Cleaning publish folder..." -ForegroundColor DarkYellow
    try { Remove-Item $OutputDir -Recurse -Force -ErrorAction SilentlyContinue } catch {}
}

Write-Host "  [1/2] Building payload (net472)..." -ForegroundColor Cyan
& dotnet build $PayloadProj --configuration Release -p:Platform=x64 --nologo -v minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "  BUILD FAILED - payload did not compile." -ForegroundColor Red
    exit 1
}

# The exe's file icon has to be a real .ico, so derive it from app.png each build.
$AppPng = "$PSScriptRoot/Injector/app.png"
$AppIco = "$PSScriptRoot/Injector/app.ico"
if (Test-Path $AppPng) {
    & "$PSScriptRoot/_tools/Convert-PngToIco.ps1" -PngPath $AppPng -IcoPath $AppIco
}

Write-Host ""
Write-Host "  [2/2] Publishing injector (single-file exe)..." -ForegroundColor Cyan
Write-Host ""

$publishArgs = @(
    "publish", $InjectorProj,
    "--configuration", "Release",
    "--runtime", "win-x64",
    "--self-contained", "false",
    "--output", $OutputDir,
    "--nologo",
    "/p:PublishSingleFile=true",
    "/p:SelfContained=false",
    "/p:PublishReadyToRun=false",
    "/p:DebugType=none",
    "/p:DebugSymbols=false"
)

& dotnet @publishArgs

Write-Host ""

if ($LASTEXITCODE -eq 0) {
    $exe = Get-Item (Join-Path $OutputDir "SamanthaTrainer.exe")
    $mb  = [math]::Round($exe.Length / 1MB, 1)

    Write-Host "  BUILD SUCCEEDED" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Output : $($exe.FullName)" -ForegroundColor White
    Write-Host "  Size   : ${mb} MB"         -ForegroundColor White
    Write-Host ""
    Write-Host "  * Start Samantha.exe first, then run the trainer." -ForegroundColor DarkGray
    Write-Host "  * Press INSERT in-game to open the menu."          -ForegroundColor DarkGray
} else {
    Write-Host "  BUILD FAILED - check errors above." -ForegroundColor Red
    exit 1
}

Write-Host ""

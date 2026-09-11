[CmdletBinding()]
param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$Assembly = "Assembly-CSharp",
    [string[]]$Source
)

$ErrorActionPreference = "Stop"
$ProjectRoot = (Resolve-Path $ProjectRoot).Path
$projectRootNormalized = $ProjectRoot.Replace('\', '/')

function Find-UnityEditorPath {
    param([string]$RootNormalized)

    try {
        $process = Get-CimInstance Win32_Process -Filter "name='Unity.exe'" |
            Where-Object {
                $_.CommandLine -and
                $_.CommandLine.Replace('\', '/').Contains($RootNormalized) -and
                $_.CommandLine -notmatch 'AssetImportWorker'
            } |
            Select-Object -First 1

        if ($process -and (Test-Path $process.ExecutablePath)) {
            return $process.ExecutablePath
        }
    }
    catch {
        # Fall back to the version recorded by the project.
    }

    $versionFile = Join-Path $ProjectRoot "ProjectSettings/ProjectVersion.txt"
    if (-not (Test-Path $versionFile)) {
        throw "ProjectVersion.txt was not found under '$ProjectRoot'."
    }

    $versionLine = Get-Content $versionFile | Where-Object { $_ -match '^m_EditorVersion:' } | Select-Object -First 1
    if (-not $versionLine) {
        throw "Could not read m_EditorVersion from '$versionFile'."
    }

    $version = ($versionLine -split ':', 2)[1].Trim()
    $candidate = Join-Path ${env:ProgramFiles} "Unity/Hub/Editor/$version/Editor/Unity.exe"
    if (-not (Test-Path $candidate)) {
        throw "Unity $version was not found at '$candidate'. Open the project in Unity or pass a valid project using that installation."
    }

    return $candidate
}

$rspCandidates = Get-ChildItem (Join-Path $ProjectRoot "Library/Bee/artifacts") -Recurse -File -Filter "$Assembly.rsp" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTimeUtc -Descending

$rsp = $rspCandidates | Select-Object -First 1
if (-not $rsp) {
    throw "No Bee response file named '$Assembly.rsp' was found. Let Unity compile the project once first."
}

$unityExe = Find-UnityEditorPath $projectRootNormalized
$editorData = Join-Path (Split-Path $unityExe -Parent) "Data"
$dotnet = Join-Path $editorData "DotNetSdk/dotnet.exe"
$csc = Get-ChildItem (Join-Path $editorData "DotNetSdk/sdk/*/Roslyn/bincore/csc.dll") -File |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not (Test-Path $dotnet) -or -not $csc) {
    throw "Could not locate Unity's bundled dotnet/Roslyn compiler under '$editorData'."
}

$tempDirectory = Join-Path $ProjectRoot "Temp/UnityCompileCheck/$Assembly"
New-Item -ItemType Directory -Path $tempDirectory -Force | Out-Null
$tempRsp = Join-Path $tempDirectory "$Assembly.rsp"
$outputDll = (Join-Path $tempDirectory "$Assembly.dll").Replace('\', '/')
$outputRefDll = (Join-Path $tempDirectory "$Assembly.ref.dll").Replace('\', '/')

$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
$rspText = [IO.File]::ReadAllText($rsp.FullName, $utf8WithoutBom)
$rspText = [Regex]::Replace($rspText, '(?m)^-out:.*$', "-out:`"$outputDll`"")
$rspText = [Regex]::Replace($rspText, '(?m)^-refout:.*$', "-refout:`"$outputRefDll`"")

foreach ($sourcePath in $Source) {
    $absoluteSource = if ([IO.Path]::IsPathRooted($sourcePath)) {
        [IO.Path]::GetFullPath($sourcePath)
    } else {
        [IO.Path]::GetFullPath((Join-Path $ProjectRoot $sourcePath))
    }

    if (-not (Test-Path $absoluteSource)) {
        throw "Source file '$sourcePath' does not exist."
    }

    $relativeSource = [IO.Path]::GetRelativePath($ProjectRoot, $absoluteSource).Replace('\', '/')
    if ($rspText -notmatch [Regex]::Escape('"' + $relativeSource + '"')) {
        $rspText += "`r`n`"$relativeSource`""
    }
}

[IO.File]::WriteAllText($tempRsp, $rspText, $utf8WithoutBom)

$compilerArguments = @(
    "exec",
    $csc.FullName,
    "/nostdlib",
    "/noconfig",
    "/shared",
    "@$tempRsp"
)

$rsp2 = Join-Path $rsp.DirectoryName "$Assembly.rsp2"
if (Test-Path $rsp2) {
    $compilerArguments += "@$rsp2"
}

Write-Host "Unity editor: $unityExe"
Write-Host "Bee response: $($rsp.FullName)"
Write-Host "Compile output: $outputDll"
Write-Host ""

Push-Location $ProjectRoot
try {
    & $dotnet $compilerArguments
    $compilerExitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

if ($compilerExitCode -ne 0) {
    Write-Error "Unity C# compile check failed with exit code $compilerExitCode."
    exit $compilerExitCode
}

Write-Host ""
Write-Host "Unity C# compile check passed." -ForegroundColor Green
exit 0

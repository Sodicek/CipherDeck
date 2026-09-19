[CmdletBinding()]
param(
    [string] $Version
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repositoryRoot 'CipherDeck.App\CipherDeck.App.csproj'
$setupProjectPath = Join-Path $repositoryRoot 'CipherDeck.Setup\CipherDeck.Setup.wixproj'

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml] $project = Get-Content -LiteralPath $projectPath
    $Version = [string] $project.Project.PropertyGroup.Version
}

if ($Version -notmatch '^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?$') {
    throw "Version '$Version' is not a supported semantic version."
}

$installerVersion = "$($Matches.major).$($Matches.minor).$($Matches.patch)"
$releaseDirectory = [System.IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot "artifacts\release\v$Version"))
$publishDirectory = [System.IO.Path]::GetFullPath(
    (Join-Path $releaseDirectory 'publish'))

if (-not $releaseDirectory.StartsWith(
        [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts\release')),
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'The release output directory escaped the repository artifacts directory.'
}

if (Test-Path -LiteralPath $releaseDirectory) {
    Remove-Item -LiteralPath $releaseDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null

dotnet publish $projectPath `
    --configuration Release `
    --output $publishDirectory `
    -warnaserror `
    -p:PublishProfile=win-x64 `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -p:Version=$Version
if ($LASTEXITCODE -ne 0) {
    throw 'Publishing the portable application failed.'
}

$portablePath = Join-Path $releaseDirectory "CipherDeck-v$Version-win-x64.zip"
Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $portablePath

dotnet build $setupProjectPath `
    --configuration Release `
    --no-incremental `
    -warnaserror `
    -p:ProductVersion=$installerVersion `
    -p:PackageVersionLabel=$Version `
    -p:PublishDir=$publishDirectory `
    -p:OutputPath=$releaseDirectory
if ($LASTEXITCODE -ne 0) {
    throw 'Building the Windows installer failed.'
}

$installerPath = Join-Path $releaseDirectory "CipherDeck-v$Version-win-x64-setup.msi"
if (-not (Test-Path -LiteralPath $installerPath)) {
    throw "The expected installer was not created at '$installerPath'."
}

$checksumPath = Join-Path $releaseDirectory "CipherDeck-v$Version-SHA256SUMS.txt"
$packages = @($portablePath, $installerPath)
$checksums = foreach ($package in $packages) {
    $hash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([System.IO.Path]::GetFileName($package))"
}
Set-Content -LiteralPath $checksumPath -Value $checksums -Encoding utf8NoBOM

Write-Output "Created release packages in $releaseDirectory"
$packages + $checksumPath | ForEach-Object { Write-Output $_ }

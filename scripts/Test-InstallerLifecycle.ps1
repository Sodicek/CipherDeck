[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if ($env:GITHUB_ACTIONS -ne 'true' -or $env:RUNNER_OS -ne 'Windows') {
    throw 'Installer lifecycle tests may run only on an ephemeral Windows GitHub Actions runner.'
}

$identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [System.Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Installer lifecycle tests require an elevated Windows runner.'
}

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$buildScript = Join-Path $PSScriptRoot 'Build-ReleasePackages.ps1'
$installedExe = Join-Path $env:ProgramFiles 'CipherDeck\CipherDeck.exe'
$startMenuShortcut = Join-Path $env:ProgramData 'Microsoft\Windows\Start Menu\Programs\CipherDeck\CipherDeck.lnk'
$installer = New-Object -ComObject WindowsInstaller.Installer
$baselineProductCode = $null
$candidateProductCode = $null

function Get-ProductCode([string] $MsiPath) {
    $database = $installer.OpenDatabase($MsiPath, 0)
    $view = $database.OpenView("SELECT ``Value`` FROM ``Property`` WHERE ``Property`` = 'ProductCode'")
    try {
        $view.Execute()
        $record = $view.Fetch()
        if ($null -eq $record) {
            throw "No ProductCode was found in '$MsiPath'."
        }
        return [string] $record.StringData(1)
    }
    finally {
        $view.Close()
    }
}

function Assert-ProductState([string] $ProductCode, [bool] $Installed) {
    $state = [int] $installer.ProductState($ProductCode)
    if ($Installed -and $state -ne 5) {
        throw "Expected product '$ProductCode' to be installed; Windows Installer state was $state."
    }
    if (-not $Installed -and $state -ne -1) {
        throw "Expected product '$ProductCode' to be absent; Windows Installer state was $state."
    }
}

function Invoke-Msi([string] $Action, [string] $Target, [string] $LogName) {
    $logPath = Join-Path $repositoryRoot "artifacts\release\$LogName"
    $arguments = @($Action, "`"$Target`"", '/qn', '/norestart', '/l*v', "`"$logPath`"")
    $process = Start-Process -FilePath (Join-Path $env:SystemRoot 'System32\msiexec.exe') `
        -ArgumentList $arguments -Wait -PassThru -WindowStyle Hidden
    if ($process.ExitCode -ne 0) {
        throw "Windows Installer $Action failed with exit code $($process.ExitCode). Log: $logPath"
    }
}

function Assert-InstalledPayload([string] $PublishPath) {
    if (-not (Test-Path -LiteralPath $installedExe -PathType Leaf)) {
        throw "Installed executable was not found at '$installedExe'."
    }
    if (-not (Test-Path -LiteralPath $startMenuShortcut -PathType Leaf)) {
        throw "Start menu shortcut was not found at '$startMenuShortcut'."
    }

    $expectedExe = Join-Path $PublishPath 'CipherDeck.exe'
    $expectedHash = (Get-FileHash -LiteralPath $expectedExe -Algorithm SHA256).Hash
    $installedHash = (Get-FileHash -LiteralPath $installedExe -Algorithm SHA256).Hash
    if ($expectedHash -ne $installedHash) {
        throw 'Installed executable differs from the published release payload.'
    }
}

try {
    if (Test-Path -LiteralPath $installedExe) {
        throw 'CipherDeck is already installed; refusing to modify an existing installation.'
    }

    & $buildScript -Version '0.10.0'
    $baselineDirectory = Join-Path $repositoryRoot 'artifacts\release\v0.10.0'
    $baselineMsi = Join-Path $baselineDirectory 'CipherDeck-v0.10.0-win-x64-setup.msi'
    $baselineProductCode = Get-ProductCode $baselineMsi
    Assert-ProductState $baselineProductCode $false

    Write-Output 'Installing the baseline MSI on a clean Windows runner...'
    Invoke-Msi '/i' $baselineMsi 'baseline-install.log'
    Assert-ProductState $baselineProductCode $true
    Assert-InstalledPayload (Join-Path $baselineDirectory 'publish')

    & $buildScript -Version '1.0.0-rc.1'
    $candidateDirectory = Join-Path $repositoryRoot 'artifacts\release\v1.0.0-rc.1'
    $candidateMsi = Join-Path $candidateDirectory 'CipherDeck-v1.0.0-rc.1-win-x64-setup.msi'
    $candidateProductCode = Get-ProductCode $candidateMsi
    Assert-ProductState $candidateProductCode $false
    if ($candidateProductCode -eq $baselineProductCode) {
        throw 'Upgrade packages must have distinct product codes.'
    }

    Write-Output 'Upgrading to the release-candidate MSI...'
    Invoke-Msi '/i' $candidateMsi 'candidate-upgrade.log'
    Assert-ProductState $baselineProductCode $false
    Assert-ProductState $candidateProductCode $true
    Assert-InstalledPayload (Join-Path $candidateDirectory 'publish')

    Write-Output 'Uninstalling the release-candidate MSI...'
    Invoke-Msi '/x' $candidateProductCode 'candidate-uninstall.log'
    Assert-ProductState $candidateProductCode $false
    if (Test-Path -LiteralPath $installedExe) {
        throw 'The executable remained after uninstall.'
    }
    if (Test-Path -LiteralPath $startMenuShortcut) {
        throw 'The Start menu shortcut remained after uninstall.'
    }

    Write-Output 'Clean install, upgrade, and uninstall checks passed.'
}
finally {
    foreach ($productCode in @($candidateProductCode, $baselineProductCode)) {
        if (-not [string]::IsNullOrWhiteSpace($productCode) -and
            [int] $installer.ProductState($productCode) -eq 5) {
            try {
                Invoke-Msi '/x' $productCode "cleanup-$productCode.log"
            }
            catch {
                Write-Warning "Cleanup failed for $productCode`: $_"
            }
        }
    }
}

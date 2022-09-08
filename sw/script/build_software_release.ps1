param (
    [Parameter(Mandatory)]
    [string]$version,

    [Parameter(Mandatory)]
    [string]$date,

    [Parameter(Mandatory)]
    [string]$sequence,

    [Parameter(Mandatory)]
    [string]$repoPath,

    [Parameter(Mandatory)]
    [string]$outputPath,

    [Parameter(Mandatory)]
    [string]$certThumbprint,

    [Parameter(Mandatory)]
    [string]$timestampServer,

    [Parameter(Mandatory)]
    [string]$signtoolPath,

    [Parameter(Mandatory)]
    [string]$sbomtoolPath,

    [Parameter(Mandatory)]
    [string]$nugetPath,

    [Parameter(Mandatory)]
    [string]$msbuildPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function CleanRepo
{
    . git -C $repoPath clean -dfx
}

function PackageSrc
{
    $pkgfolder = "KFDtool_sw_src_$($verMajor)-$($verMinor)-$($verPatch)_release_$($date)_$($sequence)"

    New-Item -ItemType Directory "$($outputPath)\$($pkgfolder)" > $null

    Copy-Item -Recurse "$($repoPath)\*" "$($outputPath)\$($pkgfolder)" -Exclude @(".git")

    Compress-Archive -Path "$($outputPath)\$($pkgfolder)" -DestinationPath "$($outputPath)\$($pkgfolder).zip"

    Remove-Item -Recurse "$($outputPath)\$($pkgfolder)"
}

function PackageSbom
{
    $pkgfolder = "KFDtool_sw_sbom_$($verMajor)-$($verMinor)-$($verPatch)_release_$($date)_$($sequence)"

    New-Item -ItemType Directory "$($outputPath)\$($pkgfolder)" > $null

    . $sbomtoolPath Generate -b "$($repoPath)\sw" -bc "$($repoPath)\sw" -ps "KFDtool, LLC" -nsb "https://kfdtool.com" -pn "KFDtool SW" -pv "$($verMajor).$($verMinor).$($verPatch)"

    Copy-Item "$($repoPath)\sw\_manifest\spdx_2.2\manifest.spdx.json" "$($outputPath)\$($pkgfolder)"

    Compress-Archive -Path "$($outputPath)\$($pkgfolder)" -DestinationPath "$($outputPath)\$($pkgfolder).zip"

    Remove-Item -Recurse "$($outputPath)\$($pkgfolder)"
}

function BuildBin
{
    . $nugetPath restore "$($repoPath)\sw\control\KFDtool.sln"

    if ($LastExitCode -ne 0)
    {
        Write-Error "package restore failed"
    }

    . $msbuildPath "$($repoPath)\sw\control\KFDtool.sln" /p:Configuration=$($config)

    if ($LastExitCode -ne 0)
    {
        Write-Error "bin build failed"
    }
}

function BuildMsi
{
    param([string]$Architecture)

    . $msbuildPath "$($repoPath)\sw\installer\KFDtoolSetup.sln" /p:Configuration=$($config) /p:Platform=x$($Architecture)

    if ($LastExitCode -ne 0)
    {
        Write-Error "msi build failed"
    }
}

function PackageBin
{
    $pkgfolder = "KFDtool_sw_bin_$($verMajor)-$($verMinor)-$($verPatch)_release_$($date)_$($sequence)"

    New-Item -ItemType Directory "$($outputPath)\$($pkgfolder)" > $null

    New-Item -ItemType Directory "$($outputPath)\$($pkgfolder)\driver" > $null

    foreach ($filename in 'kfdtool.cat', 'kfdtool.inf')
    {
        Copy-Item "$($repoPath)\sw\driver\$($filename)" "$($outputPath)\$($pkgfolder)\driver\"
    }

    Copy-Item "$($repoPath)\doc\KFDtool_Manual.pdf" "$($outputPath)\$($pkgfolder)\"

    foreach ($filename in 'HidLibrary.dll', 'KFDtool.Adapter.dll', 'KFDtool.BSL430.dll', 'KFDtool.Container.dll', 'KFDtool.P25.dll', 'KFDtool.Shared.dll', 'KFDtoolGui.exe', 'KFDtoolGui.exe.config', 'NLog.config', 'NLog.dll')
    {
        Copy-Item "$($repoPath)\sw\control\KFDtool.Gui\bin\$($config)\$($filename)" "$($outputPath)\$($pkgfolder)"
    }

    foreach ($filename in 'KFDtoolCmd.exe', 'KFDtoolCmd.exe.config', 'Mono.Options.dll')
    {
        Copy-Item "$($repoPath)\sw\control\KFDtool.Cmd\bin\$($config)\$($filename)" "$($outputPath)\$($pkgfolder)"
    }

    foreach ($filename in 'SW_CHANGELOG.txt', 'SW_LICENSE.txt')
    {
        Copy-Item "$($repoPath)\doc\$($filename)" "$($outputPath)\$($pkgfolder)"
    }

    Compress-Archive -Path "$($outputPath)\$($pkgfolder)" -DestinationPath "$($outputPath)\$($pkgfolder).zip"

    Remove-Item -Recurse "$($outputPath)\$($pkgfolder)"
}

function PackageMsi
{
    param([string]$Architecture)

    $pkgfolder = "KFDtool_sw_msi$($Architecture)_$($verMajor)-$($verMinor)-$($verPatch)_release_$($date)_$($sequence)"

    New-Item -ItemType Directory "$($outputPath)\$($pkgfolder)" > $null

    Copy-Item "$($repoPath)\sw\installer\KFDtoolApp\bin\x$($Architecture)\$($config)\KFDtool.msi" "$($outputPath)\$($pkgfolder)\$($pkgfolder).msi"

    foreach ($filename in 'SW_CHANGELOG.txt', 'SW_LICENSE.txt')
    {
        Copy-Item "$($repoPath)\doc\$($filename)" "$($outputPath)\$($pkgfolder)"
    }

    . $signtoolPath sign /v /tr "http://$($timestampServer)" /td sha256 /fd sha256 /sha1 $($certThumbprint) /d "KFDtool Setup" "$($outputPath)\$($pkgfolder)\$($pkgfolder).msi"

    Compress-Archive -Path "$($outputPath)\$($pkgfolder)" -DestinationPath "$($outputPath)\$($pkgfolder).zip"

    Remove-Item -Recurse "$($outputPath)\$($pkgfolder)"
}

function PackageSym
{
    $pkgfolder = "KFDtool_sw_sym_$($verMajor)-$($verMinor)-$($verPatch)_release_$($date)_$($sequence)"

    New-Item -ItemType Directory "$($outputPath)\$($pkgfolder)" > $null

    foreach ($filename in 'HidLibrary.pdb', 'KFDtool.Adapter.pdb', 'KFDtool.BSL430.pdb', 'KFDtool.Container.pdb', 'KFDtool.P25.pdb', 'KFDtool.Shared.pdb', 'KFDtoolGui.pdb')
    {
        Copy-Item "$($repoPath)\sw\control\KFDtool.Gui\bin\$($config)\$($filename)" "$($outputPath)\$($pkgfolder)"
    }

    Copy-Item "$($repoPath)\sw\control\KFDtool.Cmd\bin\$($config)\KFDtoolCmd.pdb" "$($outputPath)\$($pkgfolder)"

    Compress-Archive -Path "$($outputPath)\$($pkgfolder)" -DestinationPath "$($outputPath)\$($pkgfolder).zip"

    Remove-Item -Recurse "$($outputPath)\$($pkgfolder)"
}

# print arguments to console
Write-Host "*** arguments ***"
Write-Host "version: $($version)"
Write-Host "date: $($date)"
Write-Host "sequence: $($sequence)"
Write-Host "repoPath: $($repoPath)"
Write-Host "outputPath: $($outputPath)"
Write-Host "certThumbprint: $($certThumbprint)"
Write-Host "timestampServer: $($timestampServer)"
Write-Host "signtoolPath: $($signtoolPath)"
Write-Host "sbomtoolPath: $($sbomtoolPath)"
Write-Host "msbuildPath: $($msbuildPath)"
Write-Host "nugetPath: $($nugetPath)"

$relVer = [version]$version

$verMajor = $relVer.Major
$verMinor = $relVer.Minor
$verPatch = $relVer.Build

$config = "Release"

CleanRepo

PackageSrc

PackageSbom

BuildBin

BuildMsi -Architecture "64"

BuildMsi -Architecture "86"

PackageBin

PackageMsi -Architecture "64"

PackageMsi -Architecture "86"

PackageSym

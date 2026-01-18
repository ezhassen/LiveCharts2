#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build projects, pack them to NuGet packages, and push to a selected NuGet source.

.DESCRIPTION
    This script builds specified projects in Release configuration, creates NuGet packages,
    copies them to a 'nuget' folder in the root directory, and optionally pushes them to
    a specified NuGet source.

.PARAMETER ApiKey
    Optional API key for the NuGet source. If not provided and -PromptForApiKey is used, will prompt the user.

.PARAMETER PromptForApiKey
    Switch to prompt the user for an API key if one is not provided. Default is false (no prompt).

.PARAMETER SkipPush
    Switch to skip pushing packages to NuGet source (useful for testing).

.PARAMETER Configuration
    Build configuration. Default is "Release".

.EXAMPLE
    .\build-and-publish.ps1

.EXAMPLE
    .\build-and-publish.ps1 -SkipPush

.EXAMPLE
    .\build-and-publish.ps1 -PromptForApiKey

.EXAMPLE
    .\build-and-publish.ps1 -ApiKey "your-api-key"

#>

param(
    [Parameter(Mandatory = $false)]
    [string]$ApiKey,

    [switch]$SkipPush,

    [switch]$PromptForApiKey,

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release"
)

# ============================================
# CONFIGURATION - Edit these values
# ============================================

# Projects to build and pack (relative paths from root directory)
$Projects = @(
	"src\LiveChartsCore"
	"src\skiasharp\LiveChartsCore.SkiaSharp"
	"src\skiasharp\LiveChartsCore.SkiaSharp.WinForms"
	"src\skiasharp\LiveChartsCore.SkiaSharp.WPF"
	"src\skiasharp\LiveChartsCore.SkiaSharpView.WinFormsGL"
	"src\skiasharp\LiveChartsCore.SkiaSharpView.WPFGL"
    # Add more projects here as needed:
    # "Project2\Project2.csproj",
    # "Project3\Project3.csproj"
)

# NuGet source where packages will be pushed
# Examples:
# - "https://api.nuget.org/v3/index.json" (NuGet.org)
# - "https://mycompany.pkgs.visualstudio.com/MyProject/_packaging/MyFeed/nuget/v3/index.json" (Azure DevOps)
# - "C:\LocalNugetFeed" (Local folder)
#$NuGetSource = "https://api.nuget.org/v3/index.json"
$NuGetSource = "https://nuget.pkg.github.com/ezhassen/index.json"

# ============================================
# END CONFIGURATION
# ============================================

# Color functions for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error-Custom { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning-Custom { Write-Host $args -ForegroundColor Yellow }

# Get the root directory
$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$nugetDir = Join-Path $rootDir "nuget"

Write-Info "=========================================="
Write-Info "Build and Publish Script"
Write-Info "=========================================="
Write-Info "Root Directory: $rootDir"
Write-Info "Output Directory: $nugetDir"
Write-Info "Configuration: $Configuration"
Write-Info "Projects to build: $($Projects.Count)"
Write-Info ""

# Create nuget output directory if it doesn't exist
if (-not (Test-Path $nugetDir)) {
    Write-Info "Creating NuGet output directory: $nugetDir"
    New-Item -ItemType Directory -Path $nugetDir -Force | Out-Null
}

# Clean the nuget directory
Write-Info "Cleaning NuGet output directory..."
Get-ChildItem -Path $nugetDir -Filter "*.nupkg" | Remove-Item -Force
Write-Success "NuGet directory cleaned"
Write-Info ""

# Build and pack projects
$packageFiles = @()
$buildFailed = $false

foreach ($project in $Projects) {
    $projectPath = Join-Path $rootDir $project
    
    if (-not (Test-Path $projectPath)) {
        Write-Error-Custom "Project not found: $projectPath"
        $buildFailed = $true
        continue
    }
    
    Write-Info "----------------------------------------"
    Write-Info "Processing: $project"
    Write-Info "----------------------------------------"
    
    # Restore and build
    Write-Info "Building project..."
    & dotnet build $projectPath -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Build failed for: $project"
        $buildFailed = $true
        continue
    }
    Write-Success "Build succeeded"
    
    # Pack
    Write-Info "Creating NuGet package..."
    & dotnet pack $projectPath -c $Configuration -o $nugetDir --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Pack failed for: $project"
        $buildFailed = $true
        continue
    }
    Write-Success "Package created"
    Write-Info ""
}

if ($buildFailed) {
    Write-Error-Custom ""
    Write-Error-Custom "=========================================="
    Write-Error-Custom "Build or packaging failed for one or more projects!"
    Write-Error-Custom "=========================================="
    exit 1
}

# List created packages
Write-Info "=========================================="
Write-Info "Created Packages:"
Write-Info "=========================================="
$nupkgFiles = Get-ChildItem -Path $nugetDir -Filter "*.nupkg"
if ($nupkgFiles.Count -eq 0) {
    Write-Error-Custom "No packages were created!"
    exit 1
}

foreach ($file in $nupkgFiles) {
    Write-Success "$($file.Name) - $($file.Length / 1MB)MB"
    $packageFiles += $file.FullName
}
Write-Info ""

# Push to NuGet source if not skipping
if (-not $SkipPush) {
    if ([string]::IsNullOrWhiteSpace($NuGetSource)) {
        Write-Error-Custom "NuGetSource is required when not using -SkipPush"
        exit 1
    }
    
    Write-Info "=========================================="
    Write-Info "Pushing Packages to NuGet Source"
    Write-Info "=========================================="
    Write-Info "NuGet Source: $NuGetSource"
    Write-Info ""

    # Get API key if not provided and prompted
    if ([string]::IsNullOrWhiteSpace($ApiKey) -and $PromptForApiKey) {
        $ApiKey = Read-Host "Enter NuGet API Key (or press Enter to skip authentication)"
    }
    
    $pushFailed = $false
    
    foreach ($packageFile in $packageFiles) {
        $packageName = Split-Path -Leaf $packageFile
        Write-Info "Pushing: $packageName"
        
        $pushArgs = @($packageFile, "-s", $NuGetSource)
        if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
            $pushArgs += @("-k", $ApiKey)
        }
        
        & dotnet nuget push @pushArgs
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Failed to push: $packageName"
            $pushFailed = $true
        } else {
            Write-Success "Successfully pushed: $packageName"
        }
    }
    
    Write-Info ""
    if ($pushFailed) {
        Write-Error-Custom "=========================================="
        Write-Error-Custom "Some packages failed to push!"
        Write-Error-Custom "=========================================="
        exit 1
    }
}

Write-Success ""
Write-Success "=========================================="
Write-Success "Build and Package Process Completed Successfully!"
Write-Success "=========================================="
Write-Success "Packages are available in: $nugetDir"
if ($SkipPush) {
    Write-Warning-Custom "Note: Push to NuGet source was skipped"
} else {
    Write-Success "Packages have been pushed to: $NuGetSource"
}
Write-Info ""

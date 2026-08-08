#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Cuts a release of Integrity.Client: bumps <Version>, commits, tags and pushes.

.DESCRIPTION
    Pushing the tag is what triggers .github/workflows/release.yml, which re-runs
    the full CI gate and then publishes to GitHub Packages and creates a GitHub
    Release. Published package versions are immutable, so this script refuses to
    run unless the working tree is clean, HEAD is main and in sync with origin,
    and the tag does not already exist locally or on the remote.

.PARAMETER Version
    The version to release, without a leading "v" — e.g. 1.10.4 or 1.11.0-rc.1.

.PARAMETER AllowDirty
    Skip the clean-working-tree check. For recovering from a half-finished cut;
    not for normal use.

.EXAMPLE
    ./scripts/release.ps1 1.10.4

.EXAMPLE
    ./scripts/release.ps1 1.11.0-rc.1 -WhatIf
    Shows what would happen without touching anything.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$',
        ErrorMessage = 'Version must be <major>.<minor>.<patch> with an optional -prerelease suffix, and no leading "v".')]
    [string]$Version,

    [switch]$AllowDirty
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Native commands don't throw on failure, so every git call goes through here.
function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments)][string[]]$Arguments)

    $output = & git @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed:`n$output"
    }
    return $output
}

$repoRoot = Split-Path $PSScriptRoot -Parent
$csproj = Join-Path $repoRoot 'TheBleedingDeacons.Unity.Client/TheBleedingDeacons.Unity.Client.csproj'
$tag = "v$Version"

if (-not (Test-Path $csproj)) {
    throw "Client project not found at $csproj"
}

Push-Location $repoRoot
try {
    # --- Guards. Cheap to check, expensive to get wrong: a published version
    # --- can never be reused, so a bad tag means burning a version number.

    $branch = (Invoke-Git rev-parse --abbrev-ref HEAD).Trim()
    if ($branch -ne 'main') {
        throw "Releases are cut from main; currently on '$branch'."
    }

    if (-not $AllowDirty) {
        $dirty = Invoke-Git status --porcelain
        if ($dirty) {
            throw "Working tree is not clean:`n$dirty`nCommit or stash first (or pass -AllowDirty)."
        }
    }

    Write-Host 'Fetching origin...' -ForegroundColor Cyan
    Invoke-Git fetch origin --tags --quiet | Out-Null

    $local = (Invoke-Git rev-parse HEAD).Trim()
    $remote = (Invoke-Git rev-parse origin/main).Trim()
    if ($local -ne $remote) {
        throw "main is out of sync with origin/main (local $($local.Substring(0,7)), remote $($remote.Substring(0,7))). Pull or push first."
    }

    if (Invoke-Git tag --list $tag) {
        throw "Tag $tag already exists locally. Delete it first if the release was never published."
    }
    if (Invoke-Git ls-remote --tags origin "refs/tags/$tag") {
        throw "Tag $tag already exists on origin. Pick a new version — published versions cannot be reused."
    }

    # --- Bump. Regex rather than XML round-tripping, which would reflow the
    # --- whole file and strip the comments.

    $content = Get-Content -Raw $csproj
    $pattern = '(?<prefix><Version>)(?<value>[^<]*)(?<suffix></Version>)'
    $found = [regex]::Matches($content, $pattern)
    if ($found.Count -ne 1) {
        throw "Expected exactly one <Version> element in $csproj, found $($found.Count)."
    }

    $current = $found[0].Groups['value'].Value
    $needsBump = $current -ne $Version

    if ($needsBump) {
        Write-Host "Version: $current -> $Version" -ForegroundColor Cyan
    }
    else {
        # Legitimate when the csproj was already set to this version by hand.
        Write-Host "Version already $Version; tagging without a bump commit." -ForegroundColor Yellow
    }

    if (-not $PSCmdlet.ShouldProcess("$tag at $($local.Substring(0,7))", 'Tag and push release (this publishes)')) {
        Write-Host 'Dry run — nothing changed.' -ForegroundColor Yellow
        return
    }

    if ($needsBump) {
        [regex]::Replace($content, $pattern, "`${prefix}$Version`${suffix}") |
            Set-Content -Path $csproj -NoNewline
        Invoke-Git add -- $csproj | Out-Null
        Invoke-Git commit -m "release: v$Version" | Out-Null
        Write-Host "Committed release: v$Version" -ForegroundColor Green
    }

    Invoke-Git tag -a $tag -m "Integrity.Client $Version" | Out-Null
    Write-Host "Tagged $tag" -ForegroundColor Green

    # Push the branch first: if the tag landed alone, the release workflow would
    # build a commit that isn't on main.
    if ($needsBump) {
        Invoke-Git push origin main | Out-Null
    }
    Invoke-Git push origin $tag | Out-Null
    Write-Host "Pushed $tag" -ForegroundColor Green

    Write-Host ''
    Write-Host "Release workflow: https://github.com/bleedingdeacons/integrity-sharp/actions/workflows/release.yml" -ForegroundColor Cyan
    Write-Host "Watch it with:    gh run watch (gh run list --workflow release.yml --limit 1 --json databaseId --jq '.[0].databaseId')" -ForegroundColor Cyan
}
finally {
    Pop-Location
}

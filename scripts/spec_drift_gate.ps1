param(
    [string]$BaseSha,
    [string]$HeadSha
)

$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Error $Message
    exit 1
}

function Note([string]$Message) {
    Write-Host "[spec-drift-gate] $Message"
}

if ([string]::IsNullOrWhiteSpace($HeadSha)) {
    $HeadSha = (git rev-parse HEAD).Trim()
}

if ([string]::IsNullOrWhiteSpace($BaseSha)) {
    $BaseSha = (git rev-parse "$HeadSha~1").Trim()
}

Note "Comparing changes: $BaseSha..$HeadSha"

$changedFiles = git diff --name-only $BaseSha $HeadSha | ForEach-Object { $_.Trim().Replace('\\', '/') } | Where-Object { $_ -ne '' }

if (-not $changedFiles -or $changedFiles.Count -eq 0) {
    Note "No changed files found."
    exit 0
}

Note "Changed file count: $($changedFiles.Count)"

$canonicalChanged = $false
$derivedChanged = $false

foreach ($file in $changedFiles) {
    if ($file.StartsWith('SPEC/') -and -not $file.StartsWith('SPEC/SPECKIT/')) {
        $canonicalChanged = $true
    }

    if ($file.StartsWith('SPEC/SPECKIT/')) {
        $derivedChanged = $true
    }
}

if ($derivedChanged -and -not $canonicalChanged) {
    $derivedList = ($changedFiles | Where-Object { $_.StartsWith('SPEC/SPECKIT/') }) -join "`n- "
    Fail "Derived SPECKIT files changed without canonical SPEC updates. Update SPEC/*.md first, then sync SPECKIT.`n- $derivedList"
}

$metadataTargets = @()
foreach ($file in $changedFiles) {
    if ($file -match '^SPEC/SPECKIT/specs/00[2-6]-[^/]+/(spec|plan|tasks)\.md$') {
        $metadataTargets += $file
    }
}

foreach ($target in $metadataTargets) {
    if (-not (Test-Path $target)) {
        Fail "Expected changed file not found: $target"
    }

    $content = Get-Content $target -Raw

    if ($content -notmatch '(?m)^## Derived Sync Metadata\s*$') {
        Fail "Missing '## Derived Sync Metadata' header in $target"
    }

    if ($content -notmatch '(?m)^- Status: Derived\s*$') {
        Fail "Missing '- Status: Derived' line in $target"
    }

    if ($content -notmatch '(?m)^- Canonical Sources:\s*$') {
        Fail "Missing '- Canonical Sources:' line in $target"
    }

    if ($content -notmatch '(?m)^- Last Canonical Sync Date:\s*\d{4}-\d{2}-\d{2}\s*$') {
        Fail "Missing or malformed '- Last Canonical Sync Date: YYYY-MM-DD' in $target"
    }
}

Note "Spec drift gate checks passed."

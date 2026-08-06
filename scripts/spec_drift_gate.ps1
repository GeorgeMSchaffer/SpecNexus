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

$requiredCanonicalSpecs = @(
    'SPEC/10-requirements.md',
    'SPEC/30-Contracts.md',
    'SPEC/40-test-strategy.md',
    'SPEC/90-definition-of-done.md'
)

foreach ($spec in $requiredCanonicalSpecs) {
    if (-not (Test-Path $spec -PathType Leaf)) {
        Fail "Required canonical spec not found: $spec"
    }
}

$changedCanonicalSpecs = @($changedFiles | Where-Object { $_.StartsWith('SPEC/') -and $_.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) })
Note "Canonical spec files changed: $($changedCanonicalSpecs.Count)"
Note "Canonical spec gate checks passed."

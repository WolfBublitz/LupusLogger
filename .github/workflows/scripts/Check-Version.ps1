param(
    [string]$FilePath,
    [string]$ExpectedVersion
)

if (-not (Test-Path -Path $FilePath)) {
    Write-Output "File not found: $FilePath"
    exit 1
}

[xml]$xml = Get-Content -Path $FilePath
$node = $xml.SelectSingleNode("//Project/PropertyGroup/Version")

if ($null -ne $node) {
    $version = $node.InnerText

    if ($null -ne $ExpectedVersion -and $version -ne $ExpectedVersion) {
        Write-Output "❌ Version mismatch:"
        Write-Output "  Expected: $ExpectedVersion"
        Write-Output "  Found:    $version"
        exit 1
    } else {
        Write-Output "✅ Version found: $version"
    }
} else {
    Write-Output "❌ Version not found in $FilePath"
    exit 1
}
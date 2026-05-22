# Heq çelësat e dyfishtë në .resx — mban vetëm hyrjen e parë
param(
    [string[]]$Files = @(
        (Join-Path $PSScriptRoot '..\Resources\SharedResource.sq.resx'),
        (Join-Path $PSScriptRoot '..\Resources\SharedResource.resx')
    )
)

foreach ($path in $Files) {
    if (-not (Test-Path $path)) { continue }
    $lines = [IO.File]::ReadAllLines($path, [Text.UTF8Encoding]::new($false))
    $seen = @{}
    $out = New-Object System.Collections.Generic.List[string]
    $skipUntilClose = $false
    foreach ($line in $lines) {
        if ($line -match '<data name="([^"]+)"') {
            $name = $Matches[1]
            if ($seen.ContainsKey($name)) {
                $skipUntilClose = $true
                continue
            }
            $seen[$name] = $true
        }
        if ($skipUntilClose) {
            if ($line -match '</data>') { $skipUntilClose = $false }
            continue
        }
        $out.Add($line)
    }
    # Fix broken key from bulk replace
    $text = ($out -join "`r`n") -replace 'Page_NoProfileNxënës', 'Page_NoProfileStudent'
    [IO.File]::WriteAllText($path, $text, [Text.UTF8Encoding]::new($false))
    Write-Host "Deduped $path ($($seen.Count) unique keys)"
}

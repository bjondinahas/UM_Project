$ctrlPath = Join-Path $PSScriptRoot '..\Controllers'
$utf8 = New-Object System.Text.UTF8Encoding $true

Get-ChildItem $ctrlPath -Filter '*Controller.cs' | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName, $utf8)
    $orig = $c
    # Remove all stray _ui = ui assignments injected by bad script
    $c = [regex]::Replace($c, '(?m)^\s*_ui = ui;\r?\n', '')
    # Remove orphaned lines before attributes/class
    $c = $c -replace '(?s)(namespace UM_Project\.Controllers\r?\n\{\r?\n)\s*\[Authorize', '$1    [Authorize'
    if ($c -match 'TempData\["' -and $c -notmatch 'private readonly IUiText _ui') {
        if ($c -notmatch 'using UM_Project\.Services;') {
            if ($c -match 'using UM_Project\.Services\.Interfaces;') {
                $c = $c -replace '(using UM_Project\.Services\.Interfaces;)', "`$1`r`nusing UM_Project.Services;"
            } elseif ($c -match 'using UM_Project\.Models;') {
                $c = $c -replace '(using UM_Project\.Models;)', "`$1`r`nusing UM_Project.Services;"
            } else {
                $c = $c -replace '(using UM_Project\.Data;)', "`$1`r`nusing UM_Project.Services;"
            }
        }
        if ($c -match 'public class (\w+Controller)') {
            $cn = $Matches[1]
            $c = $c -replace "(public class $cn\s*\{)", "`$1`r`n        private readonly IUiText _ui;`r`n"
            # Add IUiText ui to constructor and assign once
            if ($c -match "public $cn\(([^)]*)\)\s*\{") {
                $params = $Matches[1]
                if ($params -notmatch 'IUiText ui') {
                    $newParams = if ([string]::IsNullOrWhiteSpace($params)) { 'IUiText ui' } else { "$params, IUiText ui" }
                    $c = $c -replace "public $cn\($([regex]::Escape($params))\)", "public $cn($newParams)"
                }
                $c = $c -replace "(public $cn\([^)]*\)\s*\{)", "`$1`r`n            _ui = ui;`r`n"
            }
        }
    }
    if ($c -ne $orig) {
        [IO.File]::WriteAllText($_.FullName, $c, $utf8)
        Write-Host "Fixed $($_.Name)"
    }
}

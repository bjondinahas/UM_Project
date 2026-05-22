$ctrlPath = Join-Path $PSScriptRoot '..\Controllers'
$utf8 = New-Object System.Text.UTF8Encoding $true

Get-ChildItem $ctrlPath -Filter '*Controller.cs' | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName, $utf8)
    if ($c -notmatch '_ui\[' -and $c -notmatch '_ui\.Format') { return }
    if ($c -match 'private readonly IUiText _ui') { return }
    if ($c -notmatch 'using UM_Project\.Services') {
        $c = $c -replace '(using UM_Project\.Models;)', "`$1`r`nusing UM_Project.Services;"
    }
    $c = $c -replace '(public class \w+Controller\s*:\s*Controller\s*\{\s*\r?\n)', "`$1        private readonly IUiText _ui;`r`n"
    [IO.File]::WriteAllText($_.FullName, $c, $utf8)
    Write-Host "Added field to $($_.Name)"
}

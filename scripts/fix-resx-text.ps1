# Fix corrupted Albanian text in .resx (replacement chars, broken dashes, truncated strings).

$utf8Bom = New-Object System.Text.UTF8Encoding $true
$emDash = [char]0x2014

$fixes = @{
    'Page_Parents_Subtitle' = "Prindërit me email hyrjeje $emDash lidhni fëmijët"
    'Page_Parents_Create' = 'Prind i ri'
    'Msg_AddToCourse' = 'Shto në lëndë…'
    'Pub_About_Value3' = 'Akses gjithëpërfshirës — shqip'
}

function Fix-ResxFile([string]$path) {
    $xml = [xml]([System.IO.File]::ReadAllText($path, $utf8Bom))
    $count = 0
    foreach ($d in $xml.root.data) {
        if (-not $d.name) { continue }
        $val = [string]$d.value
        $changed = $false

        if ($fixes.ContainsKey($d.name)) {
            $d.value = $fixes[$d.name]
            $changed = $true
        }
        elseif ($val -match '[\uFFFD]' -or $val -match 'hyrjeje\s*\?') {
            $val = $val -replace '[\uFFFD]+', $emDash
            if ($d.name -eq 'Page_Parents_Subtitle') { $val = $fixes['Page_Parents_Subtitle'] }
            $d.value = $val
            $changed = $true
        }

        if ($changed) { $count++ }
    }
    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Encoding = $utf8Bom
    $settings.Indent = $true
    $settings.IndentChars = '  '
    $w = [System.Xml.XmlWriter]::Create($path, $settings)
    $xml.Save($w)
    $w.Close()
    Write-Host "Fixed $count entries in $([IO.Path]::GetFileName($path))"
}

$root = Join-Path $PSScriptRoot '..'
Fix-ResxFile (Join-Path $root 'Resources\SharedResource.sq.resx')
Fix-ResxFile (Join-Path $root 'Resources\SharedResource.resx')

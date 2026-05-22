# Fixes UTF-8 mojibake (e.g. FÃ«mijÃ«t -> Fëmijët) in .resx files; saves UTF-8 with BOM.

function Test-Mojibake([string]$s) {
    if ([string]::IsNullOrEmpty($s)) { return $false }
    return $s.IndexOf([char]0x00C3) -ge 0 -or $s.IndexOf([char]0x00E2) -ge 0
}

function Fix-Mojibake([string]$s) {
    if ([string]::IsNullOrEmpty($s) -or -not (Test-Mojibake $s)) { return $s }
    try {
        $latin1 = [System.Text.Encoding]::GetEncoding('ISO-8859-1')
        $bytes = $latin1.GetBytes($s)
        $fixed = [System.Text.Encoding]::UTF8.GetString($bytes)
        if (Test-Mojibake $fixed) { return $s }
        return $fixed
    } catch {
        return $s
    }
}

function Repair-ResxFile([string]$path) {
    $utf8Bom = New-Object System.Text.UTF8Encoding $true
    $xml = [xml]([System.IO.File]::ReadAllText($path, $utf8Bom))
    $fixed = 0
    foreach ($d in $xml.root.data) {
        if (-not $d.name -or $null -eq $d.value) { continue }
        $original = [string]$d.value
        $repaired = Fix-Mojibake $original
        if ($repaired -ne $original) {
            $d.value = $repaired
            $fixed++
        }
    }
    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Encoding = $utf8Bom
    $settings.Indent = $true
    $settings.IndentChars = '  '
    $writer = [System.Xml.XmlWriter]::Create($path, $settings)
    $xml.Save($writer)
    $writer.Close()
    Write-Host "  $([System.IO.Path]::GetFileName($path)): repaired $fixed values"
}

$root = Join-Path $PSScriptRoot '..'
Repair-ResxFile (Join-Path $root 'Resources\SharedResource.sq.resx')
Repair-ResxFile (Join-Path $root 'Resources\SharedResource.resx')
Write-Host 'Encoding repair complete.'

$sqPath = Join-Path $PSScriptRoot '..\Resources\SharedResource.sq.resx'
$defPath = Join-Path $PSScriptRoot '..\Resources\SharedResource.resx'
$utf8 = New-Object System.Text.UTF8Encoding $true
[xml]$sq = [xml]([System.IO.File]::ReadAllText($sqPath, $utf8))
[xml]$def = [xml]([System.IO.File]::ReadAllText($defPath, $utf8))
$map = @{}
foreach ($d in $sq.root.data) {
    if ($d.name) { $map[$d.name] = $d.value }
}
$updated = 0
foreach ($d in $def.root.data) {
    if ($d.name -and $map.ContainsKey($d.name)) {
        $d.value = $map[$d.name]
        $updated++
    }
}
function Set-Key($name, $value) {
    $node = $def.root.data | Where-Object { $_.name -eq $name }
    if ($node) { $node.value = $value }
}
Set-Key 'Brand_Title' 'Editari'
Set-Key 'Brand_Subtitle' 'Platforma e Menaxhimit Shkollor'
Set-Key 'App_Title' 'Editari'
Set-Key 'Login_SignIn' 'Kyçu në llogarinë tënde'
Set-Key 'Login_Subtitle' 'Platforma e Menaxhimit Shkollor'
Set-Key 'Login_Submit' 'Kyçu'
Set-Key 'Login_Email' 'Email'
Set-Key 'Login_Password' 'Fjalëkalimi'
Set-Key 'Login_DemoTitle' 'Llogaritë demo'
Set-Key 'Role_SuperAdmin' 'Super Admin'
Set-Key 'Role_Admin' 'Admin Shkolle'
Set-Key 'Role_Professor' 'Profesor'
Set-Key 'Role_Student' 'Nxënës'
Set-Key 'Role_Parent' 'Prind'
$settings = New-Object System.Xml.XmlWriterSettings
$settings.Encoding = $utf8
$settings.Indent = $true
$settings.IndentChars = '  '
$writer = [System.Xml.XmlWriter]::Create($defPath, $settings)
$def.Save($writer)
$writer.Close()
Write-Host "Updated $updated resource keys to Albanian."

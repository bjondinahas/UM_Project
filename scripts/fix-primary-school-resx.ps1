# Zëvendëson tekstet e universitetit me arsim fillor (shqip)
$files = @(
    (Join-Path $PSScriptRoot '..\Resources\SharedResource.sq.resx'),
    (Join-Path $PSScriptRoot '..\Resources\SharedResource.resx')
)
$map = @{
    'UM Project' = 'Menaxhimi Shkollor'
    'Menaxhoni Universitetin Tuaj' = 'Menaxhoni Shkollën Tuaj'
    'Menaxheri Universitar' = 'Menaxhimi Shkollor'
    'Menaxherin Universitar' = 'Menaxhimin Shkollor'
    'Menaxherit Universitar' = 'Menaxhimit Shkollor'
    'universitete moderne' = 'shkollat fillore'
    'universitete' = 'shkolla fillore'
    'universitetet' = 'shkollat'
    'arsimin e lartë' = 'arsimin fillor'
    'fakultetin' = 'stafin mësimor'
    'sistemit universitar' = 'sistemit shkollor'
    'të dhënat universitare' = 'të dhënat e shkollës'
    'rrjedhës së punës akademike' = 'punës ditore në shkollë'
    'departamente' = 'klasa'
    'Departamentet' = 'Klasat'
    'Departamenti' = 'Klasa'
    'departamentet' = 'klasat'
    'departamentin' = 'klasën'
    'Profesor' = 'Mësues'
    'profesorët' = 'mësuesit'
    'profesor' = 'mësues'
    'Profesorët' = 'Mësuesit'
    'profesorin' = 'mësuesin'
    'Student' = 'Nxënës'
    'studentët' = 'nxënësit'
    'studentëve' = 'nxënësve'
    'student' = 'nxënës'
    'Akademik' = 'Shkolla'
    'the university administrator' = 'administratorin e shkollës'
    'university administrator' = 'administratorin e shkollës'
    'Zgjidh profesorin' = 'Zgjidh mësuesin'
    'Kërko departamente' = 'Kërko klasa'
    'Lëndët dhe profesorët' = 'Lëndët dhe mësuesit'
    'Profesori' = 'Mësuesi'
    'Profesorët' = 'Mësuesit'
    'Vjeshtë 2026' = 'Viti shkollor 2025-2026'
    'Default_ProfessorTitle' = 'skip'
}
foreach ($path in $files) {
    if (-not (Test-Path $path)) { continue }
    $c = [IO.File]::ReadAllText($path, [Text.UTF8Encoding]::new($false))
    foreach ($k in $map.Keys) {
        if ($k -eq 'Default_ProfessorTitle') { continue }
        $c = $c.Replace($k, $map[$k])
    }
    # Default professor title value
    $c = $c -replace '(<data name="Default_ProfessorTitle"[^>]*>\s*<value>)[^<]*(</value>)', '${1}Mësues${2}'
    [IO.File]::WriteAllText($path, $c, [Text.UTF8Encoding]::new($false))
    Write-Host "Updated $path"
}

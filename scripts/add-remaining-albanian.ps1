$keys = @'
  <data name="Common_Form" xml:space="preserve"><value>Formular</value></data>
  <data name="Common_Role" xml:space="preserve"><value>Roli</value></data>
  <data name="Common_Password" xml:space="preserve"><value>Fjalëkalimi</value></data>
  <data name="Common_Reset" xml:space="preserve"><value>Rivendos</value></data>
  <data name="Common_Add" xml:space="preserve"><value>Shto</value></data>
  <data name="Common_Enroll" xml:space="preserve"><value>Regjistro</value></data>
  <data name="Common_AllStudents" xml:space="preserve"><value>Të gjithë studentët</value></data>
  <data name="Label_Semester" xml:space="preserve"><value>Semestri</value></data>
  <data name="Label_MaxEnrollment" xml:space="preserve"><value>Regjistrimi maksimal</value></data>
  <data name="Label_LearningOutcomes" xml:space="preserve"><value>Rezultatet e të nxënit</value></data>
  <data name="Label_Prerequisites" xml:space="preserve"><value>Kushtet paraprake</value></data>
  <data name="Label_AdditionalNotes" xml:space="preserve"><value>Shënime shtesë</value></data>
  <data name="Label_Overview" xml:space="preserve"><value>Përmbledhje</value></data>
  <data name="Label_ExistingDocuments" xml:space="preserve"><value>Dokumentet ekzistuese</value></data>
  <data name="Label_Title" xml:space="preserve"><value>Titulli</value></data>
  <data name="Label_EmailLogin" xml:space="preserve"><value>Email (hyrje)</value></data>
  <data name="Label_ThisLinkStudent" xml:space="preserve"><value>Kjo lidhje — studenti</value></data>
  <data name="Label_SetGrade" xml:space="preserve"><value>Cakto notën</value></data>
  <data name="Label_Number" xml:space="preserve"><value>Numri</value></data>
  <data name="Label_NewAssignment" xml:space="preserve"><value>Detyrë / provim i ri</value></data>
  <data name="Label_AddEditPeriod" xml:space="preserve"><value>Shto ose ndrysho orën</value></data>
  <data name="Label_EventsThisMonth" xml:space="preserve"><value>Ngjarjet e këtij muaji</value></data>
  <data name="Label_AvgGrade" xml:space="preserve"><value>Nota mesatare</value></data>
  <data name="Label_Passed" xml:space="preserve"><value>Kaluan</value></data>
  <data name="Label_Failed" xml:space="preserve"><value>Dështuan</value></data>
  <data name="Label_NewPasswordBlank" xml:space="preserve"><value>Fjalëkalimi i ri (bosh = {0})</value></data>
  <data name="Label_NewPasswordDefault" xml:space="preserve"><value>Fjalëkalimi i ri (bosh = parazgjedhur)</value></data>
  <data name="Label_ForceChangeLogin" xml:space="preserve"><value>Detyro ndryshimin në hyrje</value></data>
  <data name="Label_MustChangeLogin" xml:space="preserve"><value>Duhet ndryshuar në hyrje</value></data>
  <data name="Label_SaveAttendance" xml:space="preserve"><value>Ruaj prezencën</value></data>
  <data name="Table_TimeUtc" xml:space="preserve"><value>Koha (UTC)</value></data>
  <data name="Table_Event" xml:space="preserve"><value>Ngjarja</value></data>
  <data name="Table_Location" xml:space="preserve"><value>Vendndodhja</value></data>
  <data name="Label_NoRole" xml:space="preserve"><value>Pa rol</value></data>
  <data name="Placeholder_SearchStudents" xml:space="preserve"><value>Kërko studentë...</value></data>
  <data name="Placeholder_CourseSummary" xml:space="preserve"><value>Përmbledhje e lëndës, tema...</value></data>
  <data name="Placeholder_FullName" xml:space="preserve"><value>Emri i plotë</value></data>
  <data name="Placeholder_ParentEmailAuto" xml:space="preserve"><value>Lëreni bosh për gjenerim automatik</value></data>
  <data name="Placeholder_ProfessorTitle" xml:space="preserve"><value>Profesor</value></data>
  <data name="Msg_NoDocumentsEdit" xml:space="preserve"><value>Nuk ka dokumente. Shtoni skedarë te Ndrysho.</value></data>
  <data name="Msg_NotEnrolledYet" xml:space="preserve"><value>Nuk është regjistruar në asnjë lëndë ende.</value></data>
  <data name="Msg_AssignExistingParent" xml:space="preserve"><value>Cakto te prind ekzistues</value></data>
  <data name="Msg_ParentEmailHint2" xml:space="preserve"><value>Lëreni email bosh për adresë automatik par.*, ose përdorni email ekzistues të prindit.</value></data>
  <data name="Msg_ForgotPasswordDefault" xml:space="preserve"><value>Për fjalëkalime të harruara. Parazgjedhur:</value></data>
  <data name="Msg_NoActivityLogs" xml:space="preserve"><value>Nuk ka aktivitet të regjistruar ende.</value></data>
  <data name="Msg_AdminAuditHint" xml:space="preserve"><value>Menaxhimi i plotë i përdoruesve dhe roleve.</value></data>
  <data name="Msg_ParentEditNameEmail" xml:space="preserve"><value>Emri dhe emaili vlejnë për të gjithë fëmijët e këtij prindi.</value></data>
  <data name="Msg_ParentCreateRequired" xml:space="preserve"><value>Emri i plotë kërkohet. Email opsional:</value></data>
  <data name="Confirm_LinkStudentParent" xml:space="preserve"><value>Lidh këtë student me këtë prind?</value></data>
  <data name="Flash_ScheduleDuplicate" xml:space="preserve"><value>Kjo lëndë ka tashmë orar në të njëjtën ditë dhe kohë.</value></data>
  <data name="Flash_CourseNotFound" xml:space="preserve"><value>Lënda nuk u gjet.</value></data>
  <data name="Flash_ProfessorBusy" xml:space="preserve"><value>Profesori jep tashmë lëndë tjetër në këtë ditë dhe kohë.</value></data>
  <data name="Flash_ParentLinked" xml:space="preserve"><value>Prindi u lidh. Hyrja: {0}</value></data>
  <data name="Flash_ParentLinkedPwd" xml:space="preserve"><value>Prindi u lidh. Hyrja: {0} · Fjalëkalimi: {1}</value></data>
  <data name="Flash_PasswordReset" xml:space="preserve"><value>Fjalëkalimi u rivendos. I ri: {0}</value></data>
  <data name="Flash_PasswordResetForce" xml:space="preserve"><value>Fjalëkalimi u rivendos. I ri: {0} (duhet ndryshuar në hyrjen e ardhshme).</value></data>
  <data name="Flash_PasswordResetUser" xml:space="preserve"><value>Fjalëkalimi u rivendos për {0}. I ri: {1}</value></data>
  <data name="Flash_ParentRequiredFields" xml:space="preserve"><value>Emri i prindit dhe studenti kërkohen.</value></data>
  <data name="Flash_StudentNotFound" xml:space="preserve"><value>Studenti nuk u gjet.</value></data>
  <data name="Flash_AssignChildFailed" xml:space="preserve"><value>Nuk u caktua fëmija.</value></data>
  <data name="Flash_NameEmailRequired" xml:space="preserve"><value>Emri dhe emaili kërkohen.</value></data>
  <data name="Flash_EmailInUse" xml:space="preserve"><value>Emaili përdoret nga llogari tjetër.</value></data>
  <data name="Flash_CouldNotCreateStudent" xml:space="preserve"><value>Nuk u krijua llogaria e studentit.</value></data>
  <data name="Flash_CouldNotCreateProfessor" xml:space="preserve"><value>Nuk u krijua llogaria e profesorit.</value></data>
  <data name="Flash_ParentWhenAdding" xml:space="preserve"><value>Emri i prindit kërkohet kur shtoni prind.</value></data>
  <data name="Flash_ParentCreatedLinked" xml:space="preserve"><value>Prindi u krijua: {0}, fjalëkalimi {1}</value></data>
  <data name="Flash_ParentNotLinked" xml:space="preserve"><value>Prindi nuk u lidh: {0}</value></data>
  <data name="Label_Tba" xml:space="preserve"><value>N/A</value></data>
  <data name="Label_Average" xml:space="preserve"><value>Mesatarja</value></data>
  <data name="Stat_Pass" xml:space="preserve"><value>Kalon</value></data>
  <data name="Stat_Fail" xml:space="preserve"><value>Dështon</value></data>
'@

function Add-Keys($path) {
    $utf8Bom = New-Object System.Text.UTF8Encoding $true
    $xml = [xml]([System.IO.File]::ReadAllText($path, $utf8Bom))
    $existing = @{}
    foreach ($d in $xml.root.data) { if ($d.name) { $existing[$d.name] = $true } }
    $toAdd = @()
    foreach ($line in $keys -split "`n") {
        if ($line -match 'name="([^"]+)"' -and $line -match '<value>(.*)</value>') {
            $n = $Matches[1]
            if (-not $existing.ContainsKey($n)) { $toAdd += "  $line" }
        }
    }
    if ($toAdd.Count -gt 0) {
        $raw = [IO.File]::ReadAllText($path, $utf8Bom)
        $raw = $raw -replace '</root>', (($toAdd -join "`n") + "`n</root>")
        [IO.File]::WriteAllText($path, $raw, $utf8Bom)
    }
    Write-Host "$([IO.Path]::GetFileName($path)): +$($toAdd.Count)"
}

$root = Join-Path $PSScriptRoot '..'
Add-Keys (Join-Path $root 'Resources\SharedResource.sq.resx')
$sq = [IO.File]::ReadAllText((Join-Path $root 'Resources\SharedResource.sq.resx'), (New-Object System.Text.UTF8Encoding $true))
[IO.File]::WriteAllText((Join-Path $root 'Resources\SharedResource.resx'), $sq, (New-Object System.Text.UTF8Encoding $true))
Write-Host 'Synced resx from sq.'

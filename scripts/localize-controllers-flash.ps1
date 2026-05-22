# Replace TempData English strings with IStringLocalizer lookups (requires controller DI update separately).

$ctrlPath = Join-Path $PSScriptRoot '..\Controllers'
$utf8 = New-Object System.Text.UTF8Encoding $true

$flashMap = @{
    '"Course created."' = '_localizer["Flash_CourseCreated"]'
    '"Course updated."' = '_localizer["Flash_CourseUpdated"]'
    '"Course deleted."' = '_localizer["Flash_CourseDeleted"]'
    '"Document removed."' = '_localizer["Flash_DocumentRemoved"]'
    '"Student is already enrolled in this course!"' = '_localizer["Flash_AlreadyEnrolled"]'
    '"Cannot enroll: the student has another course at the same day/time as this course''s schedule."' = '_localizer["Flash_ScheduleConflict"]'
    '"Student enrolled successfully!"' = '_localizer["Flash_EnrolledSuccess"]'
    '"Enrollment deleted successfully!"' = '_localizer["Flash_EnrollmentDeleted"]'
    '"User and role updated."' = '_localizer["Flash_UserUpdated"]'
    '"User deleted."' = '_localizer["Flash_UserDeleted"]'
    '"Cannot delete the primary SuperAdmin account."' = '_localizer["Flash_CannotDeleteSuperAdmin"]'
    '"Title required; weight must be 0–100%."' = '_localizer["Flash_AssignmentInvalid"]'
    '"Assignment added."' = '_localizer["Flash_AssignmentAdded"]'
    '"Assignment removed."' = '_localizer["Flash_AssignmentRemoved"]'
    '"Schedule created successfully!"' = '_localizer["Flash_ScheduleCreated"]'
    '"Schedule updated!"' = '_localizer["Flash_ScheduleUpdated"]'
    '"Schedule deleted successfully!"' = '_localizer["Flash_ScheduleDeleted"]'
    '"You can only request documents for your linked children."' = '_localizer["Flash_OnlyLinkedChildren"]'
    '"A pending transcript request already exists for this student."' = '_localizer["Flash_PendingTranscript"]'
    '"Transcript request submitted. An administrator will review it."' = '_localizer["Flash_TranscriptSubmitted"]'
    '"Request approved. You can download the PDF."' = '_localizer["Flash_RequestApproved"]'
    '"Request rejected."' = '_localizer["Flash_RequestRejected"]'
    '"Approve the request before generating PDF."' = '_localizer["Flash_ApproveBeforePdf"]'
    '"Parent updated."' = '_localizer["Flash_ParentUpdated"]'
    '"Parent has no login account."' = '_localizer["Flash_ParentNoLogin"]'
    '"Parent link removed."' = '_localizer["Flash_ParentLinkRemoved"]'
    '"Professor has no login account."' = '_localizer["Flash_ProfessorNoLogin"]'
    '"Professor updated!"' = '_localizer["Flash_ProfessorUpdated"]'
    '"Professor and login account deleted."' = '_localizer["Flash_ProfessorDeleted"]'
    '"Term name is required."' = '_localizer["Flash_TermNameRequired"]'
    '"Term added."' = '_localizer["Flash_TermAdded"]'
    '"Event title is required."' = '_localizer["Flash_EventTitleRequired"]'
    '"Calendar event added."' = '_localizer["Flash_EventAdded"]'
    '"Event removed."' = '_localizer["Flash_EventRemoved"]'
    '"Academic settings saved."' = '_localizer["Flash_SettingsSaved"]'
    '"Title and times are required."' = '_localizer["Flash_TitleTimesRequired"]'
    '"Lesson slot saved."' = '_localizer["Flash_LessonSlotSaved"]'
    '"Slot has attendance history — marked inactive instead of deleted."' = '_localizer["Flash_LessonSlotInactive"]'
    '"Lesson slot removed."' = '_localizer["Flash_LessonSlotRemoved"]'
    '"Grade already assigned. Contact admin to change it."' = '_localizer["Flash_GradeAlreadyAssigned"]'
    '"Grade saved successfully."' = '_localizer["Flash_GradeSaved"]'
    '"Student is already enrolled in this course."' = '_localizer["Flash_AlreadyEnrolled"]'
    '"Student enrolled in course."' = '_localizer["Flash_Enrolled"]'
    '"Enrollment removed."' = '_localizer["Flash_EnrollmentRemoved"]'
    '"Parent name is required."' = '_localizer["Flash_ParentNameRequired"]'
    '"Student updated!"' = '_localizer["Flash_StudentUpdated"]'
    '"Student and related accounts deleted."' = '_localizer["Flash_StudentDeleted"]'
    '"Grade already exists! Use Edit."' = '_localizer["Flash_GradeExists"]'
    '"Grade updated!"' = '_localizer["Flash_GradeUpdated"]'
    '"Grade deleted!"' = '_localizer["Flash_GradeDeleted"]'
    '"Department created!"' = '_localizer["Flash_DepartmentCreated"]'
    '"Department updated!"' = '_localizer["Flash_DepartmentUpdated"]'
    '"Department deleted successfully!"' = '_localizer["Flash_DepartmentDeleted"]'
    '"Note is required."' = '_localizer["Flash_NoteRequired"]'
    '"Intervention note saved."' = '_localizer["Flash_InterventionSaved"]'
}

$injectUsings = @'
using Microsoft.Extensions.Localization;
using UM_Project.Resources;
'@

Get-ChildItem $ctrlPath -Filter '*Controller.cs' | ForEach-Object {
    $path = $_.FullName
    $c = [IO.File]::ReadAllText($path, $utf8)
    if ($c -notmatch 'TempData\["') { return }
    $changed = $false
    foreach ($k in $flashMap.Keys) {
        if ($c.Contains($k)) {
            $c = $c.Replace("TempData[`"Success`"] = $k", "TempData[`"Success`"] = $($flashMap[$k]).Value")
            $c = $c.Replace("TempData[`"Error`"] = $k", "TempData[`"Error`"] = $($flashMap[$k]).Value")
            $changed = $true
        }
    }
    if (-not $changed) { return }

    if ($c -notmatch 'IStringLocalizer<SharedResource>') {
        if ($c -notmatch 'using Microsoft.Extensions.Localization') {
            $c = $c -replace '(using UM_Project\.Models;)', "`$1`nusing Microsoft.Extensions.Localization;`nusing UM_Project.Resources;"
        }
        $className = if ($c -match 'public class (\w+Controller)') { $Matches[1] } else { $null }
        if ($className) {
            $c = $c -replace "(public class $className\s*\{)", "`$1`n        private readonly IStringLocalizer<SharedResource> _localizer;`n"
            $c = $c -replace "(public $className\([^)]*\)\s*\{)", "`$1`n            IStringLocalizer<SharedResource> localizer,"
            $c = $c -replace "_localizer\[", "_localizer["
            # inject constructor param - simpler: field assign in existing ctor
        }
    }
    [IO.File]::WriteAllText($path, $c, $utf8)
    Write-Host "Patched $($_.Name)"
}

Write-Host 'Note: Controllers need IStringLocalizer injected manually if not present.'

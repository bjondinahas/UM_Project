$ctrlPath = Join-Path $PSScriptRoot '..\Controllers'
$utf8 = New-Object System.Text.UTF8Encoding $true

$replacements = @{
    'TempData["Success"] = "Course created."' = 'TempData["Success"] = _ui["Flash_CourseCreated"]'
    'TempData["Success"] = "Course updated."' = 'TempData["Success"] = _ui["Flash_CourseUpdated"]'
    'TempData["Success"] = "Course deleted."' = 'TempData["Success"] = _ui["Flash_CourseDeleted"]'
    'TempData["Success"] = "Document removed."' = 'TempData["Success"] = _ui["Flash_DocumentRemoved"]'
    'TempData["Error"] = "Note is required."' = 'TempData["Error"] = _ui["Flash_NoteRequired"]'
    'TempData["Success"] = "Intervention note saved."' = 'TempData["Success"] = _ui["Flash_InterventionSaved"]'
    'TempData["Error"] = "Title required; weight must be 0–100%."' = 'TempData["Error"] = _ui["Flash_AssignmentInvalid"]'
    'TempData["Success"] = "Assignment added."' = 'TempData["Success"] = _ui["Flash_AssignmentAdded"]'
    'TempData["Success"] = "Assignment removed."' = 'TempData["Success"] = _ui["Flash_AssignmentRemoved"]'
    'TempData["Error"] = "Student is already enrolled in this course!";' = 'TempData["Error"] = _ui["Flash_AlreadyEnrolled"];'
    'TempData["Error"] = "Student is already enrolled in this course.";' = 'TempData["Error"] = _ui["Flash_AlreadyEnrolled"];'
    'TempData["Error"] = "Cannot enroll: the student has another course at the same day/time as this course''s schedule.";' = 'TempData["Error"] = _ui["Flash_ScheduleConflict"];'
    'TempData["Success"] = "Student enrolled successfully!";' = 'TempData["Success"] = _ui["Flash_EnrolledSuccess"];'
    'TempData["Success"] = "Enrollment deleted successfully!";' = 'TempData["Success"] = _ui["Flash_EnrollmentDeleted"];'
    'TempData["Success"] = "Student enrolled in course.";' = 'TempData["Success"] = _ui["Flash_Enrolled"];'
    'TempData["Success"] = "Enrollment removed.";' = 'TempData["Success"] = _ui["Flash_EnrollmentRemoved"];'
    'TempData["Error"] = "Parent name is required.";' = 'TempData["Error"] = _ui["Flash_ParentNameRequired"];'
    'TempData["Error"] = result.Error ?? "Could not create parent.";' = 'TempData["Error"] = result.Error ?? _ui["Flash_CouldNotCreateParent"];'
    'TempData["Success"] = "Student updated!";' = 'TempData["Success"] = _ui["Flash_StudentUpdated"];'
    'TempData["Success"] = "Student and related accounts deleted.";' = 'TempData["Success"] = _ui["Flash_StudentDeleted"];'
    'TempData["Success"] = "User and role updated.";' = 'TempData["Success"] = _ui["Flash_UserUpdated"];'
    'TempData["Success"] = "User deleted.";' = 'TempData["Success"] = _ui["Flash_UserDeleted"];'
    'TempData["Error"] = "Cannot delete the primary SuperAdmin account.";' = 'TempData["Error"] = _ui["Flash_CannotDeleteSuperAdmin"];'
    'TempData["Success"] = "Academic settings saved.";' = 'TempData["Success"] = _ui["Flash_SettingsSaved"];'
    'TempData["Error"] = "Title and times are required.";' = 'TempData["Error"] = _ui["Flash_TitleTimesRequired"];'
    'TempData["Success"] = "Lesson slot saved.";' = 'TempData["Success"] = _ui["Flash_LessonSlotSaved"];'
    'TempData["Success"] = "Slot has attendance history — marked inactive instead of deleted.";' = 'TempData["Success"] = _ui["Flash_LessonSlotInactive"];'
    'TempData["Success"] = "Lesson slot removed.";' = 'TempData["Success"] = _ui["Flash_LessonSlotRemoved"];'
    'TempData["Error"] = "Grade already exists! Use Edit.";' = 'TempData["Error"] = _ui["Flash_GradeExists"];'
    'TempData["Success"] = "Grade updated!";' = 'TempData["Success"] = _ui["Flash_GradeUpdated"];'
    'TempData["Success"] = "Grade deleted!";' = 'TempData["Success"] = _ui["Flash_GradeDeleted"];'
    'TempData["Success"] = "Parent updated.";' = 'TempData["Success"] = _ui["Flash_ParentUpdated"];'
    'TempData["Error"] = "Parent has no login account.";' = 'TempData["Error"] = _ui["Flash_ParentNoLogin"];'
    'TempData["Success"] = "Parent link removed.";' = 'TempData["Success"] = _ui["Flash_ParentLinkRemoved"];'
    'TempData["Error"] = "You can only request documents for your linked children.";' = 'TempData["Error"] = _ui["Flash_OnlyLinkedChildren"];'
    'TempData["Error"] = "A pending transcript request already exists for this student.";' = 'TempData["Error"] = _ui["Flash_PendingTranscript"];'
    'TempData["Success"] = "Transcript request submitted. An administrator will review it.";' = 'TempData["Success"] = _ui["Flash_TranscriptSubmitted"];'
    'TempData["Success"] = "Request approved. You can download the PDF.";' = 'TempData["Success"] = _ui["Flash_RequestApproved"];'
    'TempData["Success"] = "Request rejected.";' = 'TempData["Success"] = _ui["Flash_RequestRejected"];'
    'TempData["Error"] = "Approve the request before generating PDF.";' = 'TempData["Error"] = _ui["Flash_ApproveBeforePdf"];'
    'TempData["Error"] = "Professor has no login account.";' = 'TempData["Error"] = _ui["Flash_ProfessorNoLogin"];'
    'TempData["Success"] = "Professor updated!";' = 'TempData["Success"] = _ui["Flash_ProfessorUpdated"];'
    'TempData["Success"] = "Professor and login account deleted.";' = 'TempData["Success"] = _ui["Flash_ProfessorDeleted"];'
    'TempData["Error"] = "Term name is required.";' = 'TempData["Error"] = _ui["Flash_TermNameRequired"];'
    'TempData["Success"] = "Term added.";' = 'TempData["Success"] = _ui["Flash_TermAdded"];'
    'TempData["Error"] = "Event title is required.";' = 'TempData["Error"] = _ui["Flash_EventTitleRequired"];'
    'TempData["Success"] = "Calendar event added.";' = 'TempData["Success"] = _ui["Flash_EventAdded"];'
    'TempData["Success"] = "Event removed.";' = 'TempData["Success"] = _ui["Flash_EventRemoved"];'
    'TempData["Success"] = "Schedule created successfully!";' = 'TempData["Success"] = _ui["Flash_ScheduleCreated"];'
    'TempData["Success"] = "Schedule updated!";' = 'TempData["Success"] = _ui["Flash_ScheduleUpdated"];'
    'TempData["Success"] = "Schedule deleted successfully!";' = 'TempData["Success"] = _ui["Flash_ScheduleDeleted"];'
    'TempData["Error"] = "Grade already assigned. Contact admin to change it.";' = 'TempData["Error"] = _ui["Flash_GradeAlreadyAssigned"];'
    'TempData["Success"] = "Grade saved successfully.";' = 'TempData["Success"] = _ui["Flash_GradeSaved"];'
    'TempData["Error"] = $"Skipped {file.FileName}: file too large (max 15 MB).";' = 'TempData["Error"] = _ui.Format("Flash_FileTooLarge", file.FileName);'
    'TempData["Error"] = $"Skipped {file.FileName}: type not allowed.";' = 'TempData["Error"] = _ui.Format("Flash_FileTypeNotAllowed", file.FileName);'
    'TempData["Error"] = $"Grade must be between {a.GradeMinimum} and {a.GradeMaximum}!";' = 'TempData["Error"] = _ui.Format("Flash_GradeRange", a.GradeMinimum, a.GradeMaximum);'
    'TempData["Error"] = $"Grade must be between {academic.GradeMinimum} and {academic.GradeMaximum}.";' = 'TempData["Error"] = _ui.Format("Flash_GradeRange", academic.GradeMinimum, academic.GradeMaximum);'
    'TempData["Success"] = $"Grade {value} assigned!";' = 'TempData["Success"] = _ui.Format("Flash_GradeAssigned", value);'
    'TempData["Error"] = $"Slot number {slotNumber} already exists.";' = 'TempData["Error"] = _ui.Format("Flash_SlotExists", slotNumber);'
    'TempData["Success"] = $"Linked {student.StudentNumber} ({student.FullName}) to parent.";' = 'TempData["Success"] = _ui.Format("Flash_LinkedStudent", student.StudentNumber, student.FullName);'
    'TempData["Success"] = $"User {user.Email} created with role {role}.";' = 'TempData["Success"] = _ui.Format("Flash_UserCreated", user.Email, role);'
    'TempData["Success"] = $"Professor created. Login: {result.Email} · Staff ID: {result.GeneratedId} · Temp password: {result.TemporaryPassword} (change on first login).";' = 'TempData["Success"] = _ui.Format("Flash_ProfessorCreated", result.Email, result.GeneratedId, result.TemporaryPassword);'
    'TempData["Error"] = await _calendar.GetBlockReasonAsync(attendanceDate) ?? "Attendance blocked for this date.";' = 'TempData["Error"] = await _calendar.GetBlockReasonAsync(attendanceDate) ?? _ui["Flash_AttendanceBlocked"];'
    'TempData["Success"] = $"Attendance saved for {attendanceDate:dd MMM yyyy} ({absentCount} absence marks).";' = 'TempData["Success"] = _ui.Format("Flash_AttendanceSaved", attendanceDate, absentCount);'
}

function Inject-UiText([string]$content, [string]$className) {
    if ($content -match 'IUiText _ui') { return $content }
    if ($content -notmatch 'using UM_Project\.Services') {
        if ($content -match 'using UM_Project\.Services\.Interfaces') {
            $content = $content -replace '(using UM_Project\.Services\.Interfaces;)', "`$1`nusing UM_Project.Services;"
        } elseif ($content -match 'using UM_Project\.Models;') {
            $content = $content -replace '(using UM_Project\.Models;)', "`$1`nusing UM_Project.Services;"
        } else {
            $content = $content -replace '(using UM_Project\.Data;)', "`$1`nusing UM_Project.Services;"
        }
    }
    $content = $content -replace "(public class $className\s*\{)", "`$1`n        private readonly IUiText _ui;`n"
    if ($content -match "public $className\(\)\s*=>\s*_context\s*=\s*context;") {
        $content = $content -replace "public $className\(ApplicationDbContext context\)\s*=>\s*_context\s*=\s*context;",
            "public $className(ApplicationDbContext context, IUiText ui)`n        {`n            _context = context;`n            _ui = ui;`n        }"
    } elseif ($content -match "public $className\(ApplicationDbContext context\) => _context = context;") {
        $content = $content -replace "public $className\(ApplicationDbContext context\) => _context = context;",
            "public $className(ApplicationDbContext context, IUiText ui)`n        {`n            _context = context;`n            _ui = ui;`n        }"
    } elseif ($content -match "public $className\(") {
        $content = $content -replace "(public $className\([^)]*)\)", '$1, IUiText ui)'
        if ($content -notmatch '_ui = ui') {
            $content = $content -replace "(\{\s*\n)", "`$1            _ui = ui;`n"
        }
    }
    return $content
}

Get-ChildItem $ctrlPath -Filter '*Controller.cs' | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName, $utf8)
    if ($c -notmatch 'TempData\["') { return }
    $orig = $c
    foreach ($k in $replacements.Keys) {
        if ($c.Contains($k)) { $c = $c.Replace($k, $replacements[$k]) }
    }
    if ($c -match 'public class (\w+Controller)') {
        $c = Inject-UiText $c $Matches[1]
    }
    if ($c -ne $orig) {
        [IO.File]::WriteAllText($_.FullName, $c, $utf8)
        Write-Host "Patched $($_.Name)"
    }
}

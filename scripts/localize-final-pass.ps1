$viewsPath = Join-Path $PSScriptRoot '..\Views'
$utf8 = New-Object System.Text.UTF8Encoding $true
$r = [ordered]@{
    'One-step registration' = '@Localizer["Page_OneStepRegistration"]'
    'Enter student name and department — account is created automatically.' = '@Localizer["Msg_StudentCreateStep1"]'
    'Optionally add a parent on the same screen (or link later from the student profile).' = '@Localizer["Msg_StudentCreateStep2"]'
    'After save, open the student profile for enrollments and credentials.' = '@Localizer["Msg_StudentCreateStep3"]'
    'Use an existing parent email to link without creating a duplicate account.' = '@Localizer["Msg_ParentEmailLinkHint"]'
    '<strong>Login email:</strong>' = '<strong>@Localizer["Label_LoginEmail"]</strong>'
    '<strong>Also linked to:</strong>' = '<strong>@Localizer["Label_AlsoLinkedTo"]</strong>'
    '<label class="form-label">This link — student</label>' = '<label class="form-label">@Localizer["Label_ThisLinkStudent"]</label>'
    'Changes which student this row points to. Use <a asp-action="AssignChild">Assign child</a> to add more children.</div>' = '@Localizer["Msg_ParentEditLinkHint"] <a asp-action="AssignChild">@Localizer["Page_AssignChild"]</a>.</div>'
    '<button type="submit" class="um-btn-primary">Save</button>' = '<button type="submit" class="um-btn-primary">@Localizer["Common_Save"]</button>'
    '<label class="form-label">Grade (5–10)</label>' = '<label class="form-label">@Localizer["Form_SelectGradeRange"]</label>'
    '<label class="form-label">Grade (5-10)</label>' = '<label class="form-label">@Localizer["Form_SelectGradeRange"]</label>'
    '<label class="form-label">Documents (PDF, Word, PPT, TXT — max 15 MB each)</label>' = '<label class="form-label">@Localizer["Label_DocumentsHint"]</label>'
    'View details</a>' = '@Localizer["Common_ViewDetails"]</a>'
    '<dt class="col-sm-3">Max enrollment</dt>' = '<dt class="col-sm-3">@Localizer["Label_MaxEnrollmentShort"]</dt>'
    'Daily lesson schedule</h1>' = '@Localizer["Page_LessonSchedule"]</h1>'
    'Up to @ViewBag.LessonsPerDay slots used for attendance. Inactive slots are hidden from professors.</p>' = '@Localizer.Format("Msg_LessonSlotsHint", ViewBag.LessonsPerDay)</p>'
    'These limits apply to professors and admin grade entry.</p>' = '@Localizer["Msg_AcademicSettingsHint"]</p>'
    'Attendance grid columns for professors (e.g. 6).</div>' = '@Localizer["Msg_AttendanceColumns"]</div>'
    '<strong>Attendance blocked:</strong>' = '<strong>@Localizer["Msg_AttendanceBlocked"]</strong>'
    '?? "Non-school day")' = '?? Localizer["Msg_NonSchoolDay"])'
    '>Load day</button>' = '>@Localizer["Btn_LoadDay"]</button>'
    '<label class="small">Due date</label>' = '<label class="small">@Localizer["Label_DueDate"]</label>'
    '<label class="small">Max points (optional)</label>' = '<label class="small">@Localizer["Label_MaxPoints"]</label>'
    '<h2 class="text-lg font-bold text-gray-600 mb-2">Past due</h2>' = '<h2 class="text-lg font-bold text-gray-600 mb-2">@Localizer["Page_PastDue"]</h2>'
    'Your account is not linked to a student yet. Please contact the university administrator.</p>' = '@Localizer["Page_NoProfileParent"]</p>'
    'Your email is not associated with any student profile. Contact admin.</p>' = '@Localizer["Page_NoProfileStudent"]</p>'
    'Your email is not associated with any professor profile. Contact admin.</p>' = '@Localizer["Page_NoProfileProfessor"]</p>'
    'An error occurred while processing your request.</h2>' = '@Localizer["Error_RequestFailed"]</h2>'
    'Leave email empty' = '@Localizer["Msg_ParentCreateEmailEmpty"]'
    'Enter existing parent email' = '@Localizer["Msg_ParentCreateEmailExisting"]'
    'Must match an existing parent login to link; otherwise a new account is created.</div>' = '@Localizer["Msg_ParentEmailMatch"]</div>'
    '<li><strong>Login email</strong> — initials + number +' = '<li><strong>@Localizer["Label_LoginEmail"]</strong> @Localizer["Msg_ProfCreateEmail"]'
    '<li><strong>Default password</strong> — must be changed on first login</li>' = '<li>@Localizer["Msg_ProfCreatePwd"]</li>'
}
$n = 0
Get-ChildItem $viewsPath -Recurse -Filter '*.cshtml' | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName, $utf8)
    $o = $c
    foreach ($k in $r.Keys) { $c = $c.Replace($k, $r[$k]) }
    if ($c -ne $o) { [IO.File]::WriteAllText($_.FullName, $c, $utf8); $n++; Write-Host $_.Name }
}
Write-Host "Done: $n files"

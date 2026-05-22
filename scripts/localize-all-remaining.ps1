$viewsPath = Join-Path $PSScriptRoot '..\Views'
$utf8 = New-Object System.Text.UTF8Encoding $true

$formTitleMap = @{
    'ViewData["FormTitle"] = "Create Course";' = 'ViewData["FormTitle"] = Localizer["Page_CreateCourse"];'
    'ViewData["FormTitle"] = "Edit Course";' = 'ViewData["FormTitle"] = Localizer["Page_EditCourse"];'
    'ViewData["FormTitle"] = "Create Department";' = 'ViewData["FormTitle"] = Localizer["Page_CreateDepartment"];'
    'ViewData["FormTitle"] = "Edit Department";' = 'ViewData["FormTitle"] = Localizer["Page_EditDepartment"];'
    'ViewData["FormTitle"] = "Create Professor";' = 'ViewData["FormTitle"] = Localizer["Page_CreateProfessor"];'
    'ViewData["FormTitle"] = "Edit Professor";' = 'ViewData["FormTitle"] = Localizer["Page_EditProfessor"];'
    'ViewData["FormTitle"] = "Register student";' = 'ViewData["FormTitle"] = Localizer["Page_CreateStudent"];'
    'ViewData["FormTitle"] = "Edit Student";' = 'ViewData["FormTitle"] = Localizer["Page_EditStudent"];'
    'ViewData["FormTitle"] = "Create Schedule";' = 'ViewData["FormTitle"] = Localizer["Page_CreateSchedule"];'
    'ViewData["FormTitle"] = "Edit Schedule";' = 'ViewData["FormTitle"] = Localizer["Page_EditSchedule"];'
    'ViewData["FormTitle"] = "New Enrollment";' = 'ViewData["FormTitle"] = Localizer["Page_NewEnrollment"];'
    'ViewData["FormTitle"] = "Edit Enrollment";' = 'ViewData["FormTitle"] = Localizer["Page_EditEnrollment"];'
    'ViewData["FormTitle"] = "Assign Grade";' = 'ViewData["FormTitle"] = Localizer["Page_AssignGrade"];'
    'ViewData["FormTitle"] = "Edit Grade";' = 'ViewData["FormTitle"] = Localizer["Page_EditGrade"];'
    'ViewData["FormTitle"] = "Create User";' = 'ViewData["FormTitle"] = Localizer["Page_CreateUser"];'
    'ViewData["FormTitle"] = "Edit User & Role";' = 'ViewData["FormTitle"] = Localizer["Page_EditUser"];'
    'ViewData["FormTitle"] = "Link Parent to Student";' = 'ViewData["FormTitle"] = Localizer["Page_LinkParent"];'
    'ViewData["FormTitle"] = "Edit parent";' = 'ViewData["FormTitle"] = Localizer["Page_EditParent"];'
    'ViewData["FormTitle"] = "Assign child to existing parent";' = 'ViewData["FormTitle"] = Localizer["Page_AssignChild"];'
}

$replacements = [ordered]@{
    '?? "Form";' = '?? Localizer["Common_Form"];'
    '<th>Name</th>' = '<th>@Localizer["Table_Name"]</th>'
    '<th>Email</th>' = '<th>@Localizer["Table_Email"]</th>'
    '<th>Number</th>' = '<th>@Localizer["Label_Number"]</th>'
    '<th>Grade</th>' = '<th>@Localizer["Table_Grade"]</th>'
    '<th>Set grade</th>' = '<th>@Localizer["Label_SetGrade"]</th>'
    '<label class="form-label">Role</label>' = '<label class="form-label">@Localizer["Common_Role"]</label>'
    '<label class="form-label">Password</label>' = '<label class="form-label">@Localizer["Common_Password"]</label>'
    '<label asp-for="Semester" class="form-label">Semester</label>' = '<label asp-for="Semester" class="form-label">@Localizer["Label_Semester"]</label>'
    '<label asp-for="MaxEnrollment" class="form-label">Max enrollment</label>' = '<label asp-for="MaxEnrollment" class="form-label">@Localizer["Label_MaxEnrollment"]</label>'
    '<label asp-for="Description" class="form-label">Description</label>' = '<label asp-for="Description" class="form-label">@Localizer["Label_Description"]</label>'
    '<label asp-for="LearningOutcomes" class="form-label">Learning outcomes</label>' = '<label asp-for="LearningOutcomes" class="form-label">@Localizer["Label_LearningOutcomes"]</label>'
    '<label asp-for="Prerequisites" class="form-label">Prerequisites</label>' = '<label asp-for="Prerequisites" class="form-label">@Localizer["Label_Prerequisites"]</label>'
    '<label asp-for="AdditionalNotes" class="form-label">Additional notes</label>' = '<label asp-for="AdditionalNotes" class="form-label">@Localizer["Label_AdditionalNotes"]</label>'
    '<label asp-for="Title" class="form-label">Title</label>' = '<label asp-for="Title" class="form-label">@Localizer["Label_Title"]</label>'
    '<label class="form-label">Documents (PDF, Word, PPT, TXT — max 15 MB each)</label>' = '<label class="form-label">@Localizer["Label_DocumentsHint"]</label>'
    '<label class="form-label">Existing documents</label>' = '<label class="form-label">@Localizer["Label_ExistingDocuments"]</label>'
    '<label class="form-label">Email (login)</label>' = '<label class="form-label">@Localizer["Label_EmailLogin"]</label>'
    '<label class="form-label">This link — student</label>' = '<label class="form-label">@Localizer["Label_ThisLinkStudent"]</label>'
    '<label class="form-label">Grade (5–10)</label>' = '<label class="form-label">@Localizer["Form_SelectGradeRange"]</label>'
    '<h3 class="h6 fw-bold text-danger mb-3">Overview</h3>' = '<h3 class="h6 fw-bold text-danger mb-3">@Localizer["Label_Overview"]</h3>'
    '<h3 class="h6 fw-bold text-danger mb-2">Description</h3>' = '<h3 class="h6 fw-bold text-danger mb-2">@Localizer["Label_Description"]</h3>'
    '<h3 class="h6 fw-bold text-danger mb-2">Learning outcomes</h3>' = '<h3 class="h6 fw-bold text-danger mb-2">@Localizer["Label_LearningOutcomes"]</h3>'
    '<h3 class="h6 fw-bold text-danger mb-2">Prerequisites</h3>' = '<h3 class="h6 fw-bold text-danger mb-2">@Localizer["Label_Prerequisites"]</h3>'
    '<h3 class="h6 fw-bold text-danger mb-2">Additional notes</h3>' = '<h3 class="h6 fw-bold text-danger mb-2">@Localizer["Label_AdditionalNotes"]</h3>'
    '<h3 class="h6 fw-bold text-danger mb-3">New assignment / exam</h3>' = '<h3 class="h6 fw-bold text-danger mb-3">@Localizer["Label_NewAssignment"]</h3>'
    '<h3 class="h6 fw-bold mb-3">Add or edit period</h3>' = '<h3 class="h6 fw-bold mb-3">@Localizer["Label_AddEditPeriod"]</h3>'
    '<h3 class="h6 fw-bold mb-3">Events this month</h3>' = '<h3 class="h6 fw-bold mb-3">@Localizer["Label_EventsThisMonth"]</h3>'
    'All students</a>' = '@Localizer["Common_AllStudents"]</a>'
    '<i class="fas fa-chart-line"></i> Analytics</a>' = '<i class="fas fa-chart-line"></i> @Localizer["Nav_Analytics"]</a>'
    'Avg grade</div>' = '@Localizer["Label_AvgGrade"]</div>'
    'Passed</div>' = '@Localizer["Label_Passed"]</div>'
    'Failed</div>' = '@Localizer["Label_Failed"]</div>'
    'Not enrolled in any courses yet.</p>' = '@Localizer["Msg_NotEnrolledYet"]</p>'
    '?? "TBA"))' = '?? Localizer["Label_Tba"]))'
    '>Enroll</button>' = '>@Localizer["Common_Enroll"]</button>'
    'Assign to an existing parent</a>' = '@Localizer["Msg_AssignExistingParent"]</a>'
    'placeholder="Parent full name"' = 'placeholder="@Localizer["Label_ParentFullName"]"'
    'placeholder="Email (optional)"' = 'placeholder="@Localizer["Label_ParentEmailOptional"]"'
    '>Add</button>' = '>@Localizer["Common_Add"]</button>'
    'Leave email empty for auto' = '@Localizer["Msg_ParentEmailHint2"]'
    'For forgotten passwords. Default:' = '@Localizer["Msg_ForgotPasswordDefault"]'
    'New password (blank = default)</label>' = '@Localizer["Label_NewPasswordDefault"]</label>'
    'Must change on login</label>' = '@Localizer["Label_MustChangeLogin"]</label>'
    '<i class="fas fa-key"></i> Reset password</button>' = '<i class="fas fa-key"></i> @Localizer["Label_ResetPassword"]</button>'
    'Email and student number cannot be changed here (tied to login account).</p>' = '@Localizer["Msg_StudentEmailLocked"]</p>'
    'Name and email apply to all children for this parent account.</div>' = '@Localizer["Msg_ParentEditNameEmail"]</div>'
    '<p class="mb-1"><strong>Full name</strong> is required. <strong>Email</strong> is optional:</p>' = '<p class="mb-1">@Localizer["Msg_ParentCreateRequired"]</p>'
    'No documents uploaded. Add files on Edit.</p>' = '@Localizer["Msg_NoDocumentsEdit"]</p>'
    '<tr><th>Time (UTC)</th>' = '<tr><th>@Localizer["Table_TimeUtc"]</th>'
    '<tr><th>Event</th>' = '<tr><th>@Localizer["Table_Event"]</th>'
    '<tr><th>Location</th>' = '<tr><th>@Localizer["Table_Location"]</th>'
    '<tr><th>Details</th>' = '<tr><th>@Localizer["Title_Details"]</th>'
    'No activity logged yet. Run admin_audit.sql if the table is missing.</td>' = '@Localizer["Msg_NoActivityLogs"]</td>'
    'placeholder="Search students..."' = 'placeholder="@Localizer["Placeholder_SearchStudents"]"'
    'placeholder="Course summary, topics covered..."' = 'placeholder="@Localizer["Placeholder_CourseSummary"]"'
    'placeholder="John Smith"' = 'placeholder="@Localizer["Placeholder_FullName"]"'
    'placeholder="Jane Doe"' = 'placeholder="@Localizer["Placeholder_FullName"]"'
    'placeholder="Leave empty to auto-generate"' = 'placeholder="@Localizer["Placeholder_ParentEmailAuto"]"'
    'placeholder="Professor"' = 'placeholder="@Localizer["Placeholder_ProfessorTitle"]"'
    'New password (blank = @defaultPwd)</label>' = '@Localizer.Format("Label_NewPasswordBlank", defaultPwd)</label>'
    'Force change on login</label>' = '@Localizer["Label_ForceChangeLogin"]</label>'
    '>Reset</button>' = '>@Localizer["Common_Reset"]</button>'
    '<td class="fw-bold">Average</td>' = '<td class="fw-bold">@Localizer["Label_Average"]</td>'
    '?? "Pass" : "Fail")' = '?? (g.Value >= 6 ? Localizer["Status_Pass"] : Localizer["Status_Fail"])'
    '<i class="fas fa-save"></i> Save attendance</button>' = '<i class="fas fa-save"></i> @Localizer["Label_SaveAttendance"]</button>'
    "confirm('Link this student to this parent?" = 'confirm(''@Localizer["Confirm_LinkStudentParent"]'
}

$n = 0
Get-ChildItem $viewsPath -Recurse -Filter '*.cshtml' | ForEach-Object {
    $c = [IO.File]::ReadAllText($_.FullName, $utf8)
    $o = $c
    foreach ($k in $formTitleMap.Keys) { $c = $c.Replace($k, $formTitleMap[$k]) }
    foreach ($k in $replacements.Keys) { $c = $c.Replace($k, $replacements[$k]) }
    if ($c -ne $o) {
        [IO.File]::WriteAllText($_.FullName, $c, $utf8)
        $n++
        Write-Host $_.Name
    }
}

Write-Host "Updated $n files."

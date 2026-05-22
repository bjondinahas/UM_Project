$viewsPath = Join-Path $PSScriptRoot '..\Views'
$replacements = @{
    'ViewData["Title"] = "Students"' = 'ViewData["Title"] = Localizer["Nav_Students"]'
    'ViewData["PageTitle"] = "Students"' = 'ViewData["PageTitle"] = Localizer["Nav_Students"]'
    'ViewData["Title"] = "Professors"' = 'ViewData["Title"] = Localizer["Nav_Professors"]'
    'ViewData["PageTitle"] = "Professors"' = 'ViewData["PageTitle"] = Localizer["Nav_Professors"]'
    'ViewData["Title"] = "Parents"' = 'ViewData["Title"] = Localizer["Nav_Parents"]'
    'ViewData["Title"] = "Departments"' = 'ViewData["Title"] = Localizer["Nav_Departments"]'
    'ViewData["Title"] = "Courses"' = 'ViewData["Title"] = Localizer["Nav_Courses"]'
    'ViewData["Title"] = "Enrollments"' = 'ViewData["Title"] = Localizer["Nav_Enrollments"]'
    'ViewData["Title"] = "Schedules"' = 'ViewData["Title"] = Localizer["Nav_Schedules"]'
    'ViewData["Title"] = "Grades"' = 'ViewData["Title"] = Localizer["Nav_Grades"]'
    'ViewData["Title"] = "Users & Roles"' = 'ViewData["Title"] = Localizer["Nav_UsersRoles"]'
    'ViewData["Title"] = "Document requests"' = 'ViewData["Title"] = Localizer["Nav_DocumentRequests"]'
    'ViewData["Title"] = "Academic Calendar"' = 'ViewData["Title"] = Localizer["Nav_AcademicCalendar"]'
    'ViewData["Title"] = "How It Works"' = 'ViewData["Title"] = Localizer["Nav_HowItWorks"]'
    'ViewData["Title"] = "My Grades"' = 'ViewData["Title"] = Localizer["Nav_MyGrades"]'
    'ViewData["Title"] = "My Schedules"' = 'ViewData["Title"] = Localizer["Nav_MySchedules"]'
    'ViewData["Title"] = "Attendance"' = 'ViewData["Title"] = Localizer["Nav_Attendance"]'
    'ViewData["Title"] = "Assignments"' = 'ViewData["Title"] = Localizer["Nav_Assignments"]'
    'ViewData["Title"] = "Lesson slots"' = 'ViewData["Title"] = Localizer["Nav_LessonSlots"]'
    'ViewData["Title"] = "Academic Settings"' = 'ViewData["Title"] = Localizer["Nav_AcademicSettings"]'
    'ViewData["Title"] = "Error"' = 'ViewData["Title"] = Localizer["Common_Error"]'
}
$files = Get-ChildItem $viewsPath -Recurse -Filter '*.cshtml'
$count = 0
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $orig = $content
    foreach ($k in $replacements.Keys) {
        $content = $content.Replace($k, $replacements[$k])
    }
    if ($content -ne $orig) {
        Set-Content $file.FullName $content -Encoding UTF8 -NoNewline
        $count++
    }
}
Write-Host "Updated $count view files."

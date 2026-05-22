using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UM_Project.Models;

namespace UM_Project.Data
{
    public static class DemoDataSeeder
    {
        public const string DefaultPassword = "Admin@123";

        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            bool reseedDemoData,
            CancellationToken cancellationToken = default)
        {
            foreach (var role in RoleNames.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await EnsureUserAsync(userManager, "superadmin@umproject.com", "Super Administrator", "SA001", RoleNames.SuperAdmin, DefaultPassword);
            await EnsureUserAsync(userManager, "admin@umproject.com", "System Administrator", "ADMIN001", RoleNames.Admin, DefaultPassword);

            if (!reseedDemoData && await context.Departments.AnyAsync(cancellationToken))
            {
                Console.WriteLine("  Demo data already present — skipping bulk seed (set Database:ReseedDemoData = true to replace).");
                return;
            }

            if (reseedDemoData)
            {
                Console.WriteLine("  ReseedDemoData: clearing demo tables...");
                await ClearDemoDataAsync(context, cancellationToken);
            }

            Console.WriteLine("  Seeding demo departments, users, courses, enrollments...");

            var deptCs = new Department { DepartmentName = "Computer Science" };
            var deptBus = new Department { DepartmentName = "Business Administration" };
            var deptEng = new Department { DepartmentName = "Engineering" };
            var deptLaw = new Department { DepartmentName = "Law" };
            context.Departments.AddRange(deptCs, deptBus, deptEng, deptLaw);
            await context.SaveChangesAsync(cancellationToken);

            var profElena = await SeedProfessorAsync(context, userManager, "prof.elena.marku@umproject.edu", "Dr. Elena Marku", "Associate Professor", "PROF20260001", deptCs.DepartmentId);
            var profDritan = await SeedProfessorAsync(context, userManager, "prof.dritan.hoxha@umproject.edu", "Dr. Dritan Hoxha", "Professor", "PROF20260002", deptCs.DepartmentId);
            var profArtan = await SeedProfessorAsync(context, userManager, "prof.artan.shehu@umproject.edu", "Dr. Artan Shehu", "Senior Lecturer", "PROF20260003", deptBus.DepartmentId);
            var profMira = await SeedProfessorAsync(context, userManager, "prof.mira.krasniqi@umproject.edu", "Dr. Mira Krasniqi", "Lecturer", "PROF20260004", deptEng.DepartmentId);
            var profJon = await SeedProfessorAsync(context, userManager, "prof.jon.berisha@umproject.edu", "Dr. Jon Berisha", "Associate Professor", "PROF20260005", deptLaw.DepartmentId);

            var students = new List<Student>();
            var studentSpecs = new[]
            {
                ("stu.ana.gashi@umproject.edu", "Ana Gashi", "STU20260001", deptCs.DepartmentId),
                ("stu.besnik.krasniqi@umproject.edu", "Besnik Krasniqi", "STU20260002", deptCs.DepartmentId),
                ("stu.elira.berisha@umproject.edu", "Elira Berisha", "STU20260003", deptBus.DepartmentId),
                ("stu.florian.meta@umproject.edu", "Florian Meta", "STU20260004", deptEng.DepartmentId),
                ("stu.gentiana.hoxha@umproject.edu", "Gentiana Hoxha", "STU20260005", deptCs.DepartmentId),
                ("stu.ilir.rama@umproject.edu", "Ilir Rama", "STU20260006", deptLaw.DepartmentId),
                ("stu.klea.meta@umproject.edu", "Klea Meta", "STU20260007", deptCs.DepartmentId),
                ("stu.luan.berisha@umproject.edu", "Luan Berisha", "STU20260008", deptBus.DepartmentId),
                ("stu.mira.gashi@umproject.edu", "Mira Gashi", "STU20260009", deptEng.DepartmentId),
                ("stu.nora.rama@umproject.edu", "Nora Rama", "STU20260010", deptLaw.DepartmentId),
                ("stu.olen.hoxha@umproject.edu", "Olen Hoxha", "STU20260011", deptCs.DepartmentId),
                ("stu.petra.krasniqi@umproject.edu", "Petra Krasniqi", "STU20260012", deptBus.DepartmentId),
            };
            foreach (var (email, name, number, deptId) in studentSpecs)
                students.Add(await SeedStudentAsync(context, userManager, email, name, number, deptId));

            var parentUser = await EnsureUserAsync(userManager, "par.artan.gashi@umproject.edu", "Artan Gashi", "PAR001", RoleNames.Parent, DefaultPassword);
            context.ParentGuardians.Add(new ParentGuardian
            {
                FullName = "Artan Gashi",
                Email = "par.artan.gashi@umproject.edu",
                UserId = parentUser.Id,
                StudentId = students[0].StudentId
            });
            context.ParentGuardians.Add(new ParentGuardian
            {
                FullName = "Luljeta Hoxha",
                Email = "par.luljeta.hoxha@umproject.edu",
                UserId = (await EnsureUserAsync(userManager, "par.luljeta.hoxha@umproject.edu", "Luljeta Hoxha", "PAR002", RoleNames.Parent, DefaultPassword)).Id,
                StudentId = students[4].StudentId
            });
            await context.SaveChangesAsync(cancellationToken);

            var courses = new List<(Course course, string day, string time, string room)>
            {
                (new Course { CourseCode = "CS101", CourseName = "Introduction to Programming", Credits = 6, DepartmentId = deptCs.DepartmentId, ProfessorId = profElena.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 40, Description = "Fundamentals of programming with C#." }, "Monday", "09:00", "A-101"),
                (new Course { CourseCode = "CS201", CourseName = "Data Structures", Credits = 6, DepartmentId = deptCs.DepartmentId, ProfessorId = profDritan.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 35, Description = "Lists, trees, graphs, and algorithms." }, "Wednesday", "10:00", "B-204"),
                (new Course { CourseCode = "CS301", CourseName = "Web Development", Credits = 5, DepartmentId = deptCs.DepartmentId, ProfessorId = profElena.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 30 }, "Friday", "11:00", "Lab-3"),
                (new Course { CourseCode = "BUS101", CourseName = "Introduction to Business", Credits = 5, DepartmentId = deptBus.DepartmentId, ProfessorId = profArtan.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 50 }, "Tuesday", "09:00", "C-110"),
                (new Course { CourseCode = "BUS201", CourseName = "Marketing Principles", Credits = 5, DepartmentId = deptBus.DepartmentId, ProfessorId = profArtan.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 40 }, "Thursday", "14:00", "C-112"),
                (new Course { CourseCode = "ENG101", CourseName = "Engineering Mathematics", Credits = 6, DepartmentId = deptEng.DepartmentId, ProfessorId = profDritan.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 45 }, "Monday", "14:00", "D-201"),
                (new Course { CourseCode = "LAW101", CourseName = "Introduction to Law", Credits = 5, DepartmentId = deptLaw.DepartmentId, ProfessorId = profJon.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 40 }, "Tuesday", "11:00", "E-101"),
                (new Course { CourseCode = "CS401", CourseName = "Software Engineering", Credits = 6, DepartmentId = deptCs.DepartmentId, ProfessorId = profElena.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 28 }, "Thursday", "09:00", "Lab-5"),
                (new Course { CourseCode = "BUS301", CourseName = "Financial Management", Credits = 5, DepartmentId = deptBus.DepartmentId, ProfessorId = profArtan.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 35 }, "Wednesday", "15:00", "C-115"),
                (new Course { CourseCode = "ENG201", CourseName = "Thermodynamics", Credits = 6, DepartmentId = deptEng.DepartmentId, ProfessorId = profMira.ProfessorId, Semester = "Fall 2026", MaxEnrollment = 32 }, "Friday", "13:00", "D-305"),
            };

            var courseEntities = new List<Course>();
            foreach (var (course, day, time, room) in courses)
            {
                context.Courses.Add(course);
                await context.SaveChangesAsync(cancellationToken);
                context.Schedules.Add(new Schedule { CourseId = course.CourseId, Day = day, Time = time, Room = room });
                courseEntities.Add(course);
            }
            await context.SaveChangesAsync(cancellationToken);

            var enrollmentMap = new Dictionary<string, string[]>
            {
                [students[0].Email] = ["CS101", "CS201", "BUS101", "CS401"],
                [students[1].Email] = ["CS101", "CS201", "CS301", "CS401"],
                [students[2].Email] = ["BUS101", "BUS201", "BUS301"],
                [students[3].Email] = ["ENG101", "ENG201", "BUS101"],
                [students[4].Email] = ["CS101", "CS301", "CS201"],
                [students[5].Email] = ["LAW101", "BUS101"],
                [students[6].Email] = ["CS101", "CS201", "CS301"],
                [students[7].Email] = ["BUS101", "BUS201", "BUS301"],
                [students[8].Email] = ["ENG101", "ENG201"],
                [students[9].Email] = ["LAW101"],
                [students[10].Email] = ["CS101", "CS401", "BUS101"],
                [students[11].Email] = ["BUS101", "BUS201", "BUS301"],
            };

            var courseByCode = courseEntities.ToDictionary(c => c.CourseCode, c => c);
            var enrollments = new List<Enrollment>();
            foreach (var student in students)
            {
                if (!enrollmentMap.TryGetValue(student.Email, out var codes))
                    continue;
                foreach (var code in codes)
                {
                    enrollments.Add(new Enrollment
                    {
                        StudentId = student.StudentId,
                        CourseId = courseByCode[code].CourseId,
                        EnrollmentDate = DateTime.UtcNow.AddDays(-30)
                    });
                }
            }
            context.Enrollments.AddRange(enrollments);
            await context.SaveChangesAsync(cancellationToken);

            if (!await context.LessonSlots.AnyAsync(cancellationToken))
            {
                var slotDefs = new[]
                {
                    (1, "Lesson 1", "08:00", "08:45"),
                    (2, "Lesson 2", "08:50", "09:35"),
                    (3, "Lesson 3", "09:40", "10:25"),
                    (4, "Lesson 4", "10:30", "11:15"),
                    (5, "Lesson 5", "11:20", "12:05"),
                    (6, "Lesson 6", "12:10", "12:55"),
                };
                foreach (var (num, title, start, end) in slotDefs)
                {
                    context.LessonSlots.Add(new LessonSlot
                    {
                        SlotNumber = num,
                        Title = title,
                        StartTime = start,
                        EndTime = end,
                        IsActive = true
                    });
                }
            }

            await EnsureSystemSettingsAsync(context, cancellationToken);

            var term = new AcademicTerm
            {
                Name = "Fall 2026",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 12, 20),
                IsActive = true
            };
            context.AcademicTerms.Add(term);
            await context.SaveChangesAsync(cancellationToken);

            var gradeValues = new[] { 5, 5, 4, 4, 4, 3, 3, 3, 3, 2, 2, 2, 1, 1, 1 };
            var gradeIndex = 0;
            foreach (var enrollment in enrollments)
            {
                context.Grades.Add(new Grade
                {
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    Value = gradeValues[gradeIndex % gradeValues.Length],
                    DateRecorded = term.StartDate.AddDays(20 + (gradeIndex % 45))
                });
                gradeIndex++;
            }
            await context.SaveChangesAsync(cancellationToken);

            context.SchoolCalendarEvents.AddRange(
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 10, 28), Title = "National Holiday", EventType = CalendarEventTypes.Holiday, BlocksAttendance = true },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 12, 8), Title = "Final Exam Week", EventType = CalendarEventTypes.ExamWeek, BlocksAttendance = false },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 11, 15), Title = "Mid-term Break", EventType = CalendarEventTypes.Break, BlocksAttendance = true },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 9, 15), Title = "Orientation Week", EventType = CalendarEventTypes.NoSchool, BlocksAttendance = false },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 11, 1), Title = "Career Fair", EventType = CalendarEventTypes.NoSchool, BlocksAttendance = false }
            );

            var cs101 = courseByCode["CS101"];
            var cs201 = courseByCode["CS201"];
            context.CourseAssignments.AddRange(
                new CourseAssignment { CourseId = cs101.CourseId, Title = "Hello World Lab", Description = "First console application.", DueDate = DateTime.UtcNow.AddDays(14), WeightPercent = 15, MaxPoints = 100 },
                new CourseAssignment { CourseId = cs101.CourseId, Title = "Midterm Project", DueDate = DateTime.UtcNow.AddDays(45), WeightPercent = 35, MaxPoints = 100 },
                new CourseAssignment { CourseId = cs201.CourseId, Title = "Binary Tree Exercise", DueDate = DateTime.UtcNow.AddDays(21), WeightPercent = 20, MaxPoints = 50 }
            );

            var slots = await context.LessonSlots.OrderBy(l => l.SlotNumber).Take(3).ToListAsync(cancellationToken);
            if (slots.Count >= 2)
            {
                for (var dayOffset = 1; dayOffset <= 60; dayOffset++)
                {
                    var attendanceDate = DateTime.Today.AddDays(-dayOffset);
                    if (attendanceDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                        continue;
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[0].StudentId, CourseId = cs101.CourseId, LessonSlotId = slots[0].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = dayOffset % 4 != 0, ProfessorId = profElena.ProfessorId });
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[1].StudentId, CourseId = cs101.CourseId, LessonSlotId = slots[0].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = true, ProfessorId = profElena.ProfessorId });
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[4].StudentId, CourseId = cs101.CourseId, LessonSlotId = slots[1].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = dayOffset % 3 != 0, ProfessorId = profElena.ProfessorId, Notes = dayOffset % 3 == 0 ? "Absent" : null });
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[6].StudentId, CourseId = cs101.CourseId, LessonSlotId = slots[0].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = dayOffset % 5 != 0, ProfessorId = profElena.ProfessorId });
                }
            }

            await SeedAuthAuditLogsAsync(context, students, new[] { profElena, profDritan, profArtan, profMira, profJon }, cancellationToken);

            var adminUser = await userManager.FindByEmailAsync("admin@umproject.com");
            if (adminUser != null)
            {
                context.DocumentRequests.AddRange(
                    new DocumentRequest
                    {
                        RequestedByUserId = parentUser.Id,
                        StudentId = students[0].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Pending,
                        ParentNotes = "Needed for scholarship application.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-3)
                    },
                    new DocumentRequest
                    {
                        RequestedByUserId = parentUser.Id,
                        StudentId = students[0].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Approved,
                        ParentNotes = "For embassy.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-12),
                        ProcessedAtUtc = DateTime.UtcNow.AddDays(-10)
                    },
                    new DocumentRequest
                    {
                        RequestedByUserId = parentUser.Id,
                        StudentId = students[4].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Pending,
                        ParentNotes = "University transfer.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-1)
                    });

                context.InterventionNotes.Add(new InterventionNote
                {
                    StudentId = students[4].StudentId,
                    Note = "Monitor attendance — two absences in CS101 this week.",
                    CreatedByUserId = adminUser.Id,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
                });
            }

            await context.SaveChangesAsync(cancellationToken);
            Console.WriteLine("  Demo data seed complete.");
        }

        private static async Task ClearDemoDataAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            context.AuthAuditLogs.RemoveRange(await context.AuthAuditLogs.ToListAsync(cancellationToken));
            context.AttendanceRecords.RemoveRange(await context.AttendanceRecords.ToListAsync(cancellationToken));
            context.InterventionNotes.RemoveRange(await context.InterventionNotes.ToListAsync(cancellationToken));
            context.DocumentRequests.RemoveRange(await context.DocumentRequests.ToListAsync(cancellationToken));
            context.CourseAssignments.RemoveRange(await context.CourseAssignments.ToListAsync(cancellationToken));
            context.SchoolCalendarEvents.RemoveRange(await context.SchoolCalendarEvents.ToListAsync(cancellationToken));
            context.AcademicTerms.RemoveRange(await context.AcademicTerms.ToListAsync(cancellationToken));
            context.Grades.RemoveRange(await context.Grades.ToListAsync(cancellationToken));
            context.Enrollments.RemoveRange(await context.Enrollments.ToListAsync(cancellationToken));
            context.Schedules.RemoveRange(await context.Schedules.ToListAsync(cancellationToken));
            context.CourseDocuments.RemoveRange(await context.CourseDocuments.ToListAsync(cancellationToken));
            context.Courses.RemoveRange(await context.Courses.ToListAsync(cancellationToken));
            context.ParentGuardians.RemoveRange(await context.ParentGuardians.ToListAsync(cancellationToken));
            context.Students.RemoveRange(await context.Students.ToListAsync(cancellationToken));
            context.Professors.RemoveRange(await context.Professors.ToListAsync(cancellationToken));
            context.Departments.RemoveRange(await context.Departments.ToListAsync(cancellationToken));
            await context.SaveChangesAsync(cancellationToken);

            var demoEmails = new[]
            {
                "prof.elena.marku@umproject.edu", "prof.dritan.hoxha@umproject.edu", "prof.artan.shehu@umproject.edu",
                "prof.mira.krasniqi@umproject.edu", "prof.jon.berisha@umproject.edu",
                "stu.ana.gashi@umproject.edu", "stu.besnik.krasniqi@umproject.edu", "stu.elira.berisha@umproject.edu",
                "stu.florian.meta@umproject.edu", "stu.gentiana.hoxha@umproject.edu", "stu.ilir.rama@umproject.edu",
                "stu.klea.meta@umproject.edu", "stu.luan.berisha@umproject.edu", "stu.mira.gashi@umproject.edu",
                "stu.nora.rama@umproject.edu", "stu.olen.hoxha@umproject.edu", "stu.petra.krasniqi@umproject.edu",
                "par.artan.gashi@umproject.edu", "par.luljeta.hoxha@umproject.edu"
            };
            var demoEmailSet = new HashSet<string>(demoEmails, StringComparer.OrdinalIgnoreCase);
            var users = (await context.Users.ToListAsync(cancellationToken))
                .Where(u => u.Email != null && demoEmailSet.Contains(u.Email))
                .ToList();
            foreach (var user in users)
                await context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUserRoles WHERE UserId = {0}", user.Id);
            context.Users.RemoveRange(users);
            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedAuthAuditLogsAsync(
            ApplicationDbContext context,
            List<Student> students,
            Professor[] professors,
            CancellationToken cancellationToken)
        {
            if (await context.AuthAuditLogs.AnyAsync(cancellationToken))
                return;

            var rnd = new Random(42);
            var emails = students.Select(s => s.Email)
                .Concat(professors.Select(p => p.Email))
                .Append("admin@umproject.com")
                .Append("superadmin@umproject.com")
                .ToList();

            for (var day = 6; day >= 0; day--)
            {
                var dayStart = DateTime.UtcNow.Date.AddDays(-day);
                var loginCount = rnd.Next(4, 14);
                for (var i = 0; i < loginCount; i++)
                {
                    context.AuthAuditLogs.Add(new AuthAuditLog
                    {
                        Email = emails[rnd.Next(emails.Count)],
                        EventType = AuthEventTypes.LoginSuccess,
                        Success = true,
                        IpAddress = "127.0.0.1",
                        Country = "Kosovo",
                        City = "Pristina",
                        CreatedAtUtc = dayStart.AddHours(8 + rnd.Next(0, 10)).AddMinutes(rnd.Next(0, 59))
                    });
                }
                if (day % 2 == 0)
                {
                    context.AuthAuditLogs.Add(new AuthAuditLog
                    {
                        Email = "unknown@example.com",
                        EventType = AuthEventTypes.LoginFailed,
                        Success = false,
                        FailureReason = "Invalid password",
                        IpAddress = "203.0.113." + rnd.Next(1, 50),
                        CreatedAtUtc = dayStart.AddHours(12)
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureSystemSettingsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            async Task Upsert(string key, string value, string? desc)
            {
                var row = await context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key, cancellationToken);
                if (row == null)
                    context.SystemSettings.Add(new SystemSetting { SettingKey = key, SettingValue = value, Description = desc });
                else
                {
                    row.SettingValue = value;
                    if (desc != null) row.Description = desc;
                }
            }

            async Task Set(string key, string value, string? desc)
            {
                if (await context.SystemSettings.AnyAsync(s => s.SettingKey == key, cancellationToken))
                    return;
                context.SystemSettings.Add(new SystemSetting { SettingKey = key, SettingValue = value, Description = desc });
            }

            await Upsert(SystemSettingKeys.GradeMinimum, "1", "Lowest allowed grade value");
            await Upsert(SystemSettingKeys.GradeMaximum, "5", "Highest allowed grade value");
            await Upsert(SystemSettingKeys.GradePassingMinimum, "3", "Minimum grade to pass");
            await Set(SystemSettingKeys.LessonsPerDay, "6", "Lesson slots per school day");
            await Set(SystemSettingKeys.AtRiskAbsenceDays, "14", "Days window for at-risk absence rule");
            await Set(SystemSettingKeys.AtRiskAbsenceCount, "3", "Absences in window to flag at-risk");
            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task<Professor> SeedProfessorAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string title,
            string customId,
            int departmentId)
        {
            var user = await EnsureUserAsync(userManager, email, fullName, customId, RoleNames.Professor, DefaultPassword);
            var professor = new Professor
            {
                FullName = fullName,
                Email = email,
                Title = title,
                DepartmentId = departmentId,
                UserId = user.Id
            };
            context.Professors.Add(professor);
            await context.SaveChangesAsync();
            return professor;
        }

        private static async Task<Student> SeedStudentAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string studentNumber,
            int departmentId)
        {
            var user = await EnsureUserAsync(userManager, email, fullName, studentNumber, RoleNames.Student, DefaultPassword);
            var student = new Student
            {
                FullName = fullName,
                Email = email,
                StudentNumber = studentNumber,
                DepartmentId = departmentId,
                UserId = user.Id
            };
            context.Students.Add(student);
            await context.SaveChangesAsync();
            return student;
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string customId,
            string role,
            string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    CustomId = customId,
                    Address = "Main Campus",
                    MustChangePassword = false
                };
                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new InvalidOperationException($"Could not create {email}: {string.Join(", ", create.Errors.Select(e => e.Description))}");
                await userManager.AddToRoleAsync(user, role);
                return user;
            }

            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);
            return user;
        }
    }
}

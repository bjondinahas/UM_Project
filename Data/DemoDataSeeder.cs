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

            await EnsureUserAsync(userManager, "superadmin@shkollademo.edu", "Administrator Kryesor", "SA001", RoleNames.SuperAdmin, DefaultPassword);
            await EnsureUserAsync(userManager, "admin@shkollademo.edu", "Administrator Sistemi", "ADMIN001", RoleNames.Admin, DefaultPassword);

            if (!reseedDemoData && await context.Departments.AnyAsync(cancellationToken))
            {
                Console.WriteLine("  Demo data already present — skipping bulk seed (set Database:ReseedDemoData = true to replace).");
                await SyncAllDemoPasswordsAsync(userManager);
                return;
            }

            if (reseedDemoData)
            {
                Console.WriteLine("  ReseedDemoData: clearing demo tables...");
                await ClearDemoDataAsync(context, userManager, cancellationToken);
            }

            Console.WriteLine("  Seeding demo klasa, mësues, nxënës, lëndë...");

            var klasa1 = new Department { DepartmentName = "Klasa 1" };
            var klasa2 = new Department { DepartmentName = "Klasa 2" };
            var klasa3 = new Department { DepartmentName = "Klasa 3" };
            var klasa4 = new Department { DepartmentName = "Klasa 4" };
            var klasa5 = new Department { DepartmentName = "Klasa 5" };
            context.Departments.AddRange(klasa1, klasa2, klasa3, klasa4, klasa5);
            await context.SaveChangesAsync(cancellationToken);

            var mesuesElena = await SeedProfessorAsync(context, userManager, "mesues.elena@shkollademo.edu", "Elena Marku", "Mësuese", "MES001", klasa1.DepartmentId);
            var mesuesDritan = await SeedProfessorAsync(context, userManager, "mesues.dritan@shkollademo.edu", "Dritan Hoxha", "Mësues", "MES002", klasa2.DepartmentId);
            var mesuesArtan = await SeedProfessorAsync(context, userManager, "mesues.artan@shkollademo.edu", "Artan Shehu", "Mësues", "MES003", klasa3.DepartmentId);
            var mesuesMira = await SeedProfessorAsync(context, userManager, "mesues.mira@shkollademo.edu", "Mira Krasniqi", "Mësuese", "MES004", klasa4.DepartmentId);
            var mesuesJon = await SeedProfessorAsync(context, userManager, "mesues.jon@shkollademo.edu", "Jon Berisha", "Mësues", "MES005", klasa5.DepartmentId);

            var students = new List<Student>();
            var studentSpecs = new[]
            {
                ("nx.ana.gashi@shkollademo.edu", "Ana Gashi", "NX20250001", klasa1.DepartmentId),
                ("nx.besnik.krasniqi@shkollademo.edu", "Besnik Krasniqi", "NX20250002", klasa1.DepartmentId),
                ("nx.elira.berisha@shkollademo.edu", "Elira Berisha", "NX20250003", klasa2.DepartmentId),
                ("nx.florian.meta@shkollademo.edu", "Florian Meta", "NX20250004", klasa2.DepartmentId),
                ("nx.gentiana.hoxha@shkollademo.edu", "Gentiana Hoxha", "NX20250005", klasa3.DepartmentId),
                ("nx.ilir.rama@shkollademo.edu", "Ilir Rama", "NX20250006", klasa3.DepartmentId),
                ("nx.klea.meta@shkollademo.edu", "Klea Meta", "NX20250007", klasa4.DepartmentId),
                ("nx.luan.berisha@shkollademo.edu", "Luan Berisha", "NX20250008", klasa4.DepartmentId),
                ("nx.mira.gashi@shkollademo.edu", "Mira Gashi", "NX20250009", klasa5.DepartmentId),
                ("nx.nora.rama@shkollademo.edu", "Nora Rama", "NX20250010", klasa5.DepartmentId),
                ("nx.olen.hoxha@shkollademo.edu", "Olen Hoxha", "NX20250011", klasa3.DepartmentId),
                ("nx.petra.krasniqi@shkollademo.edu", "Petra Krasniqi", "NX20250012", klasa2.DepartmentId),
            };
            foreach (var (email, name, number, deptId) in studentSpecs)
                students.Add(await SeedStudentAsync(context, userManager, email, name, number, deptId));

            var parentUser = await EnsureUserAsync(userManager, "pr.artan.gashi@shkollademo.edu", "Artan Gashi", "PR001", RoleNames.Parent, DefaultPassword);
            context.ParentGuardians.Add(new ParentGuardian
            {
                FullName = "Artan Gashi",
                Email = "pr.artan.gashi@shkollademo.edu",
                UserId = parentUser.Id,
                StudentId = students[0].StudentId
            });
            context.ParentGuardians.Add(new ParentGuardian
            {
                FullName = "Luljeta Hoxha",
                Email = "pr.luljeta.hoxha@shkollademo.edu",
                UserId = (await EnsureUserAsync(userManager, "pr.luljeta.hoxha@shkollademo.edu", "Luljeta Hoxha", "PR002", RoleNames.Parent, DefaultPassword)).Id,
                StudentId = students[4].StudentId
            });
            await context.SaveChangesAsync(cancellationToken);

            const string vitiShkollor = "Viti shkollor 2025-2026";
            var courses = new List<(Course course, string day, string time, string room)>
            {
                (new Course { CourseCode = "MAT1", CourseName = "Matematikë", Credits = 2, DepartmentId = klasa1.DepartmentId, ProfessorId = mesuesElena.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28, Description = "Numërimi dhe operacionet bazë." }, "Monday", "08:00", "Dhoma 101"),
                (new Course { CourseCode = "GJU1", CourseName = "Gjuhë Shqipe", Credits = 2, DepartmentId = klasa1.DepartmentId, ProfessorId = mesuesElena.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28, Description = "Lexim dhe shkrim." }, "Monday", "09:00", "Dhoma 101"),
                (new Course { CourseCode = "MAT2", CourseName = "Matematikë", Credits = 2, DepartmentId = klasa2.DepartmentId, ProfessorId = mesuesDritan.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28, Description = "Shumëzimi dhe pjesëtimi." }, "Tuesday", "08:00", "Dhoma 202"),
                (new Course { CourseCode = "GJU2", CourseName = "Gjuhë Shqipe", Credits = 2, DepartmentId = klasa2.DepartmentId, ProfessorId = mesuesDritan.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Tuesday", "09:00", "Dhoma 202"),
                (new Course { CourseCode = "SHK3", CourseName = "Shkenca", Credits = 2, DepartmentId = klasa3.DepartmentId, ProfessorId = mesuesArtan.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28, Description = "Mjedisi dhe eksperimente të thjeshta." }, "Wednesday", "08:00", "Dhoma 303"),
                (new Course { CourseCode = "ANG3", CourseName = "Anglisht", Credits = 1, DepartmentId = klasa3.DepartmentId, ProfessorId = mesuesArtan.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Wednesday", "10:00", "Dhoma 303"),
                (new Course { CourseCode = "MAT4", CourseName = "Matematikë", Credits = 2, DepartmentId = klasa4.DepartmentId, ProfessorId = mesuesMira.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Thursday", "08:00", "Dhoma 404"),
                (new Course { CourseCode = "MUS4", CourseName = "Muzikë", Credits = 1, DepartmentId = klasa4.DepartmentId, ProfessorId = mesuesMira.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Thursday", "11:00", "Salla e muzikës"),
                (new Course { CourseCode = "EDF5", CourseName = "Edukatë Fizike", Credits = 1, DepartmentId = klasa5.DepartmentId, ProfessorId = mesuesJon.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Friday", "08:00", "Palestra"),
                (new Course { CourseCode = "GJU5", CourseName = "Gjuhë Shqipe", Credits = 2, DepartmentId = klasa5.DepartmentId, ProfessorId = mesuesJon.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Friday", "09:00", "Dhoma 505"),
                (new Course { CourseCode = "ART3", CourseName = "Art", Credits = 1, DepartmentId = klasa3.DepartmentId, ProfessorId = mesuesArtan.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Wednesday", "14:00", "Salla e artit"),
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
                [students[0].Email] = ["MAT1", "GJU1"],
                [students[1].Email] = ["MAT1", "GJU1"],
                [students[2].Email] = ["MAT2", "GJU2"],
                [students[3].Email] = ["MAT2", "GJU2"],
                [students[4].Email] = ["SHK3", "ANG3", "ART3"],
                [students[5].Email] = ["SHK3", "ANG3"],
                [students[6].Email] = ["MAT4", "MUS4"],
                [students[7].Email] = ["MAT4", "MUS4"],
                [students[8].Email] = ["EDF5", "GJU5"],
                [students[9].Email] = ["EDF5", "GJU5"],
                [students[10].Email] = ["SHK3", "ART3"],
                [students[11].Email] = ["MAT2", "GJU2"],
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
                    (1, "Ora 1", "08:00", "08:45"),
                    (2, "Ora 2", "08:50", "09:35"),
                    (3, "Ora 3", "09:40", "10:25"),
                    (4, "Ora 4", "10:30", "11:15"),
                    (5, "Ora 5", "11:20", "12:05"),
                    (6, "Ora 6", "12:10", "12:55"),
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
                Name = "Viti shkollor 2025-2026",
                StartDate = new DateTime(2025, 9, 1),
                EndDate = new DateTime(2026, 6, 15),
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
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2025, 11, 28), Title = "Dita e Flamurit", EventType = CalendarEventTypes.Holiday, BlocksAttendance = true },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 1, 10), Title = "Pushimi i dimrit", EventType = CalendarEventTypes.Break, BlocksAttendance = true },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 3, 1), Title = "Java e dytë e vjeshtës", EventType = CalendarEventTypes.ExamWeek, BlocksAttendance = false },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2025, 9, 8), Title = "Java e parë e shkollës", EventType = CalendarEventTypes.NoSchool, BlocksAttendance = false },
                new SchoolCalendarEvent { AcademicTermId = term.AcademicTermId, EventDate = new DateTime(2026, 4, 15), Title = "Dita e Tokës", EventType = CalendarEventTypes.NoSchool, BlocksAttendance = false }
            );

            var mat1 = courseByCode["MAT1"];
            var gju2 = courseByCode["GJU2"];
            context.CourseAssignments.AddRange(
                new CourseAssignment { CourseId = mat1.CourseId, Title = "Detyrë: Numërimi deri në 100", Description = "Ushtrime në shtëpi.", DueDate = DateTime.UtcNow.AddDays(7), WeightPercent = 20, MaxPoints = 10 },
                new CourseAssignment { CourseId = mat1.CourseId, Title = "Test i shkurtër — mbledhja", DueDate = DateTime.UtcNow.AddDays(21), WeightPercent = 30, MaxPoints = 10 },
                new CourseAssignment { CourseId = gju2.CourseId, Title = "Ese: Familja ime", DueDate = DateTime.UtcNow.AddDays(14), WeightPercent = 25, MaxPoints = 10 }
            );

            var slots = await context.LessonSlots.OrderBy(l => l.SlotNumber).Take(3).ToListAsync(cancellationToken);
            if (slots.Count >= 2)
            {
                for (var dayOffset = 1; dayOffset <= 60; dayOffset++)
                {
                    var attendanceDate = DateTime.Today.AddDays(-dayOffset);
                    if (attendanceDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                        continue;
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[0].StudentId, CourseId = mat1.CourseId, LessonSlotId = slots[0].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = dayOffset % 4 != 0, ProfessorId = mesuesElena.ProfessorId });
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[1].StudentId, CourseId = mat1.CourseId, LessonSlotId = slots[0].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = true, ProfessorId = mesuesElena.ProfessorId });
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[4].StudentId, CourseId = courseByCode["SHK3"].CourseId, LessonSlotId = slots[1].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = dayOffset % 3 != 0, ProfessorId = mesuesArtan.ProfessorId, Notes = dayOffset % 3 == 0 ? "Mungesë" : null });
                    context.AttendanceRecords.Add(new AttendanceRecord { StudentId = students[6].StudentId, CourseId = courseByCode["MAT4"].CourseId, LessonSlotId = slots[0].LessonSlotId, AttendanceDate = attendanceDate, IsPresent = dayOffset % 5 != 0, ProfessorId = mesuesMira.ProfessorId });
                }
            }

            await SeedAuthAuditLogsAsync(context, students, new[] { mesuesElena, mesuesDritan, mesuesArtan, mesuesMira, mesuesJon }, cancellationToken);

            var adminUser = await userManager.FindByEmailAsync("admin@shkollademo.edu");
            if (adminUser != null)
            {
                context.DocumentRequests.AddRange(
                    new DocumentRequest
                    {
                        RequestedByUserId = parentUser.Id,
                        StudentId = students[0].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Pending,
                        ParentNotes = "Nevojitet për aplikim bursë.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-3)
                    },
                    new DocumentRequest
                    {
                        RequestedByUserId = parentUser.Id,
                        StudentId = students[0].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Approved,
                        ParentNotes = "Për dokumentacion zyrtar.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-12),
                        ProcessedAtUtc = DateTime.UtcNow.AddDays(-10)
                    },
                    new DocumentRequest
                    {
                        RequestedByUserId = parentUser.Id,
                        StudentId = students[4].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Pending,
                        ParentNotes = "Transferim në shkollë tjetër.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-1)
                    });

                context.InterventionNotes.Add(new InterventionNote
                {
                    StudentId = students[4].StudentId,
                    Note = "Vëzhgo mungesat — dy mungesa në Matematikë këtë javë.",
                    CreatedByUserId = adminUser.Id,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
                });
            }

            await context.SaveChangesAsync(cancellationToken);
            await SyncAllDemoPasswordsAsync(userManager);
            Console.WriteLine("  Demo data seed complete.");
        }

        private static async Task ClearDemoDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            CancellationToken cancellationToken)
        {
            context.ChangeTracker.Clear();

            await context.AuthAuditLogs.ExecuteDeleteAsync(cancellationToken);
            await context.AdminActivityLogs.ExecuteDeleteAsync(cancellationToken);
            await context.AttendanceRecords.ExecuteDeleteAsync(cancellationToken);
            await context.InterventionNotes.ExecuteDeleteAsync(cancellationToken);
            await context.DocumentRequests.ExecuteDeleteAsync(cancellationToken);
            await context.CourseAssignments.ExecuteDeleteAsync(cancellationToken);
            await context.SchoolCalendarEvents.ExecuteDeleteAsync(cancellationToken);
            await context.Grades.ExecuteDeleteAsync(cancellationToken);
            await context.Enrollments.ExecuteDeleteAsync(cancellationToken);
            await context.Schedules.ExecuteDeleteAsync(cancellationToken);
            await context.CourseDocuments.ExecuteDeleteAsync(cancellationToken);
            await context.Courses.ExecuteDeleteAsync(cancellationToken);
            await context.ParentGuardians.ExecuteDeleteAsync(cancellationToken);
            await context.Students.ExecuteDeleteAsync(cancellationToken);
            await context.Professors.ExecuteDeleteAsync(cancellationToken);
            await context.Departments.ExecuteDeleteAsync(cancellationToken);
            await context.AcademicTerms.ExecuteDeleteAsync(cancellationToken);

            var demoEmailSet = new HashSet<string>(GetDemoEmails(), StringComparer.OrdinalIgnoreCase);
            var demoUsers = (await context.Users.ToListAsync(cancellationToken))
                .Where(u => u.Email != null && demoEmailSet.Contains(u.Email))
                .ToList();

            if (demoUsers.Count > 0)
            {
                var demoUserIds = demoUsers.Select(u => u.Id).ToHashSet();
                var tokensToRemove = (await context.RefreshTokens.ToListAsync(cancellationToken))
                    .Where(t => demoUserIds.Contains(t.UserId))
                    .ToList();
                if (tokensToRemove.Count > 0)
                {
                    context.RefreshTokens.RemoveRange(tokensToRemove);
                    await context.SaveChangesAsync(cancellationToken);
                }

                foreach (var user in demoUsers)
                {
                    var result = await userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        Console.WriteLine($"  Warning: could not delete user {user.Email}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            context.ChangeTracker.Clear();
            Console.WriteLine("  Demo tables and demo users cleared.");
        }

        private static string[] GetDemoEmails() =>
        [
            "superadmin@umproject.com", "admin@umproject.com",
            "prof.elena.marku@umproject.edu", "prof.dritan.hoxha@umproject.edu", "prof.artan.shehu@umproject.edu",
            "prof.mira.krasniqi@umproject.edu", "prof.jon.berisha@umproject.edu",
            "stu.ana.gashi@umproject.edu", "stu.besnik.krasniqi@umproject.edu", "stu.elira.berisha@umproject.edu",
            "stu.florian.meta@umproject.edu", "stu.gentiana.hoxha@umproject.edu", "stu.ilir.rama@umproject.edu",
            "stu.klea.meta@umproject.edu", "stu.luan.berisha@umproject.edu", "stu.mira.gashi@umproject.edu",
            "stu.nora.rama@umproject.edu", "stu.olen.hoxha@umproject.edu", "stu.petra.krasniqi@umproject.edu",
            "par.artan.gashi@umproject.edu", "par.luljeta.hoxha@umproject.edu",
            "superadmin@shkollademo.edu", "admin@shkollademo.edu",
            "mesues.elena@shkollademo.edu", "mesues.dritan@shkollademo.edu", "mesues.artan@shkollademo.edu",
            "mesues.mira@shkollademo.edu", "mesues.jon@shkollademo.edu",
            "nx.ana.gashi@shkollademo.edu", "nx.besnik.krasniqi@shkollademo.edu", "nx.elira.berisha@shkollademo.edu",
            "nx.florian.meta@shkollademo.edu", "nx.gentiana.hoxha@shkollademo.edu", "nx.ilir.rama@shkollademo.edu",
            "nx.klea.meta@shkollademo.edu", "nx.luan.berisha@shkollademo.edu", "nx.mira.gashi@shkollademo.edu",
            "nx.nora.rama@shkollademo.edu", "nx.olen.hoxha@shkollademo.edu", "nx.petra.krasniqi@shkollademo.edu",
            "pr.artan.gashi@shkollademo.edu", "pr.luljeta.hoxha@shkollademo.edu"
        ];

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
                .Append("admin@shkollademo.edu")
                .Append("superadmin@shkollademo.edu")
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
                        FailureReason = "Fjalëkalim i gabuar",
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
                    Address = "Shkolla Fillore \"Drita\", Prishtinë",
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

            if (!string.Equals(user.UserName, email, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                user.UserName = email;
                user.Email = email;
                user.NormalizedUserName = email.ToUpperInvariant();
                user.NormalizedEmail = email.ToUpperInvariant();
                var update = await userManager.UpdateAsync(user);
                if (!update.Succeeded)
                    throw new InvalidOperationException($"Could not update {email}: {string.Join(", ", update.Errors.Select(e => e.Description))}");
            }

            await SyncDemoPasswordAsync(userManager, user, password);
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);
            return user;
        }

        private static async Task SyncAllDemoPasswordsAsync(UserManager<ApplicationUser> userManager)
        {
            foreach (var email in GetDemoEmails())
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                    continue;
                await SyncDemoPasswordAsync(userManager, user, DefaultPassword);
                await userManager.ResetAccessFailedCountAsync(user);
                await userManager.SetLockoutEndDateAsync(user, null);
            }
            Console.WriteLine("  Demo account passwords synced to Admin@123 (where users exist).");
        }

        private static async Task SyncDemoPasswordAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string password)
        {
            var hasPassword = await userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                var add = await userManager.AddPasswordAsync(user, password);
                if (!add.Succeeded)
                    throw new InvalidOperationException($"Could not set password for {user.Email}: {string.Join(", ", add.Errors.Select(e => e.Description))}");
                return;
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, password);
            if (!reset.Succeeded)
                throw new InvalidOperationException($"Could not reset password for {user.Email}: {string.Join(", ", reset.Errors.Select(e => e.Description))}");
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UM_Project.Models;

namespace UM_Project.Data
{
    public static class DemoDataSeeder
    {
        public const string DefaultPassword = "Admin@123";
        private const string DemoEnrichmentVersionKey = "DemoDataEnrichmentVersion";
        private const string DemoEnrichmentVersion = "2";

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

            await MigrateLegacyAdminEmailsAsync(userManager);

            await EnsureUserAsync(userManager, SystemAccountEmails.SuperAdmin, "Administrator Kryesor", "SA001", RoleNames.SuperAdmin, DefaultPassword);
            await EnsureUserAsync(userManager, SystemAccountEmails.Admin, "Administrator Sistemi", "ADMIN001", RoleNames.Admin, DefaultPassword);

            if (!reseedDemoData && await context.Departments.AnyAsync(cancellationToken))
            {
                Console.WriteLine("  Demo data already present — skipping bulk seed (set Database:ReseedDemoData = true to replace).");
                await SyncAllDemoPasswordsAsync(userManager);
                await EnrichDemoDataAsync(context, userManager, cancellationToken);
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
                ("nx.arben.morina@shkollademo.edu", "Arben Morina", "NX20250013", klasa1.DepartmentId),
                ("nx.blerta.shala@shkollademo.edu", "Blerta Shala", "NX20250014", klasa2.DepartmentId),
                ("nx.dren.gashi@shkollademo.edu", "Dren Gashi", "NX20250015", klasa3.DepartmentId),
                ("nx.erza.krasniqi@shkollademo.edu", "Erza Krasniqi", "NX20250016", klasa4.DepartmentId),
                ("nx.fisnik.berisha@shkollademo.edu", "Fisnik Berisha", "NX20250017", klasa5.DepartmentId),
                ("nx.genta.hoxha@shkollademo.edu", "Genta Hoxha", "NX20250018", klasa1.DepartmentId),
            };
            foreach (var (email, name, number, deptId) in studentSpecs)
                students.Add(await SeedStudentAsync(context, userManager, email, name, number, deptId));

            await SeedParentGuardiansAsync(context, userManager, students, cancellationToken);

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
                (new Course { CourseCode = "NAT1", CourseName = "Natyra", Credits = 1, DepartmentId = klasa1.DepartmentId, ProfessorId = mesuesElena.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28, Description = "Bimët dhe kafshët." }, "Monday", "10:00", "Dhoma 101"),
                (new Course { CourseCode = "EDU2", CourseName = "Edukatë Qytetare", Credits = 1, DepartmentId = klasa2.DepartmentId, ProfessorId = mesuesDritan.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Tuesday", "10:00", "Dhoma 202"),
                (new Course { CourseCode = "INF4", CourseName = "Informatikë", Credits = 1, DepartmentId = klasa4.DepartmentId, ProfessorId = mesuesMira.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Thursday", "10:00", "Dhoma 404"),
                (new Course { CourseCode = "HIS5", CourseName = "Histori", Credits = 1, DepartmentId = klasa5.DepartmentId, ProfessorId = mesuesJon.ProfessorId, Semester = vitiShkollor, MaxEnrollment = 28 }, "Friday", "10:00", "Dhoma 505"),
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
                [students[0].Email] = ["MAT1", "GJU1", "NAT1"],
                [students[1].Email] = ["MAT1", "GJU1", "NAT1"],
                [students[2].Email] = ["MAT2", "GJU2", "EDU2"],
                [students[3].Email] = ["MAT2", "GJU2", "EDU2"],
                [students[4].Email] = ["SHK3", "ANG3", "ART3"],
                [students[5].Email] = ["SHK3", "ANG3"],
                [students[6].Email] = ["MAT4", "MUS4", "INF4"],
                [students[7].Email] = ["MAT4", "MUS4", "INF4"],
                [students[8].Email] = ["EDF5", "GJU5", "HIS5"],
                [students[9].Email] = ["EDF5", "GJU5", "HIS5"],
                [students[10].Email] = ["SHK3", "ART3"],
                [students[11].Email] = ["MAT2", "GJU2", "EDU2"],
                [students[12].Email] = ["MAT1", "GJU1", "NAT1"],
                [students[13].Email] = ["MAT2", "GJU2", "EDU2"],
                [students[14].Email] = ["SHK3", "ANG3"],
                [students[15].Email] = ["MAT4", "MUS4", "INF4"],
                [students[16].Email] = ["EDF5", "GJU5", "HIS5"],
                [students[17].Email] = ["MAT1", "GJU1", "NAT1"],
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

            await EnsureLessonSlotsAsync(context, cancellationToken);

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

            var atRiskStudentIds = new HashSet<int>
            {
                students[4].StudentId, students[5].StudentId, students[10].StudentId, students[11].StudentId
            };
            AddGradesForEnrollments(context, enrollments, term.StartDate, atRiskStudentIds, new Random(42));
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

            var slots = await context.LessonSlots.Where(l => l.IsActive).OrderBy(l => l.SlotNumber).ToListAsync(cancellationToken);
            var professors = new[] { mesuesElena, mesuesDritan, mesuesArtan, mesuesMira, mesuesJon };
            if (slots.Count >= 1)
            {
                var enrollmentsByStudent = enrollments.GroupBy(e => e.StudentId).ToDictionary(g => g.Key, g => g.ToList());
                var attRnd = new Random(99);
                for (var dayOffset = 1; dayOffset <= 45; dayOffset++)
                {
                    var attendanceDate = DateTime.Today.AddDays(-dayOffset);
                    if (attendanceDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                        continue;

                    foreach (var student in students)
                    {
                        if (!enrollmentsByStudent.TryGetValue(student.StudentId, out var studentEnrollments))
                            continue;
                        var enrollment = studentEnrollments[attRnd.Next(studentEnrollments.Count)];
                        var course = courseEntities.First(c => c.CourseId == enrollment.CourseId);
                        var professor = professors.First(p => p.ProfessorId == course.ProfessorId);
                        var isAtRisk = atRiskStudentIds.Contains(student.StudentId);
                        var present = isAtRisk ? attRnd.Next(0, 3) != 0 : attRnd.Next(0, 10) != 0;

                        context.AttendanceRecords.Add(new AttendanceRecord
                        {
                            StudentId = student.StudentId,
                            CourseId = enrollment.CourseId,
                            LessonSlotId = slots[attRnd.Next(slots.Count)].LessonSlotId,
                            AttendanceDate = attendanceDate,
                            IsPresent = present,
                            ProfessorId = professor.ProfessorId,
                            Notes = !present ? "Mungesë" : null
                        });
                    }
                }
            }

            context.CourseAssignments.AddRange(
                new CourseAssignment { CourseId = courseByCode["SHK3"].CourseId, Title = "Projekt: Mjedisi lokal", DueDate = DateTime.UtcNow.AddDays(10), WeightPercent = 25, MaxPoints = 10 },
                new CourseAssignment { CourseId = courseByCode["MAT4"].CourseId, Title = "Ushtrime: Thyesat", DueDate = DateTime.UtcNow.AddDays(5), WeightPercent = 20, MaxPoints = 10 },
                new CourseAssignment { CourseId = courseByCode["GJU5"].CourseId, Title = "Lexim: Tregim i shkurtër", DueDate = DateTime.UtcNow.AddDays(12), WeightPercent = 15, MaxPoints = 10 }
            );

            await SeedAuthAuditLogsAsync(context, students, professors, cancellationToken);

            var adminUser = await userManager.FindByEmailAsync(SystemAccountEmails.Admin);
            var primaryParent = await userManager.FindByEmailAsync("pr.artan.gashi@shkollademo.edu");
            if (adminUser != null && primaryParent != null)
            {
                context.DocumentRequests.AddRange(
                    new DocumentRequest
                    {
                        RequestedByUserId = primaryParent.Id,
                        StudentId = students[0].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Pending,
                        ParentNotes = "Nevojitet për aplikim bursë.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-3)
                    },
                    new DocumentRequest
                    {
                        RequestedByUserId = primaryParent.Id,
                        StudentId = students[0].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Approved,
                        ParentNotes = "Për dokumentacion zyrtar.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-12),
                        ProcessedAtUtc = DateTime.UtcNow.AddDays(-10)
                    },
                    new DocumentRequest
                    {
                        RequestedByUserId = primaryParent.Id,
                        StudentId = students[4].StudentId,
                        RequestType = DocumentRequestTypes.Transcript,
                        Status = DocumentRequestStatuses.Pending,
                        ParentNotes = "Transferim në shkollë tjetër.",
                        RequestedAtUtc = DateTime.UtcNow.AddDays(-1)
                    });

                var atRiskNotes = new[]
                {
                    (students[4].StudentId, "Vëzhgo mungesat — nota të ulëta në Shkencë."),
                    (students[5].StudentId, "Kontakt me prindin — mungesa të shpeshta."),
                    (students[10].StudentId, "Plan mbështetjeje në Matematikë."),
                    (students[11].StudentId, "Takim me mësuesin e klasës — performancë e dobët.")
                };
                for (var ni = 0; ni < atRiskNotes.Length; ni++)
                {
                    var (sid, note) = atRiskNotes[ni];
                    context.InterventionNotes.Add(new InterventionNote
                    {
                        StudentId = sid,
                        Note = note,
                        CreatedByUserId = adminUser.Id,
                        CreatedAtUtc = DateTime.UtcNow.AddDays(-ni - 1)
                    });
                }

                context.DocumentRequests.Add(new DocumentRequest
                {
                    RequestedByUserId = primaryParent.Id,
                    StudentId = students[10].StudentId,
                    RequestType = DocumentRequestTypes.Transcript,
                    Status = DocumentRequestStatuses.Rejected,
                    ParentNotes = "Dokument i paplotë.",
                    RequestedAtUtc = DateTime.UtcNow.AddDays(-8),
                    ProcessedAtUtc = DateTime.UtcNow.AddDays(-6)
                });

                await SeedDemoAdminActivityAsync(context, adminUser, students, professors, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
            await SyncAllDemoPasswordsAsync(userManager);
            Console.WriteLine("  Demo data seed complete.");
        }

        private static async Task EnrichDemoDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            CancellationToken cancellationToken)
        {
            var marker = await context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == DemoEnrichmentVersionKey, cancellationToken);
            if (marker?.SettingValue == DemoEnrichmentVersion)
                return;

            Console.WriteLine("  Enriching demo data (parents, lessons, grades)...");

            var students = await context.Students.OrderBy(s => s.StudentNumber).ToListAsync(cancellationToken);
            if (students.Count > 0)
                await SeedParentGuardiansAsync(context, userManager, students, cancellationToken);

            await EnsureLessonSlotsAsync(context, cancellationToken);
            await EnsureExtraCoursesAndEnrollmentsAsync(context, cancellationToken);

            var term = await context.AcademicTerms.OrderByDescending(t => t.StartDate).FirstOrDefaultAsync(cancellationToken);
            var termStart = term?.StartDate ?? new DateTime(2025, 9, 1);

            var enrollments = await context.Enrollments.ToListAsync(cancellationToken);
            if (enrollments.Count > 0)
            {
                var atRiskIds = new HashSet<int>(
                    students.Where(s => s.StudentNumber is "NX20250005" or "NX20250006" or "NX20250011" or "NX20250012")
                        .Select(s => s.StudentId));
                await AddMissingGradesAsync(context, enrollments, termStart, atRiskIds, cancellationToken);
            }

            if (marker == null)
            {
                context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = DemoEnrichmentVersionKey,
                    SettingValue = DemoEnrichmentVersion,
                    Description = "Demo enrichment batch marker"
                });
            }
            else
                marker.SettingValue = DemoEnrichmentVersion;

            await context.SaveChangesAsync(cancellationToken);
            await SyncParentDemoPasswordsAsync(userManager);
            Console.WriteLine("  Demo enrichment complete.");
        }

        private static async Task SeedParentGuardiansAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            List<Student> students,
            CancellationToken cancellationToken)
        {
            foreach (var (email, fullName, customId, studentIndex, extraStudentIndex) in GetParentSpecs())
            {
                if (studentIndex >= students.Count)
                    continue;
                if (await context.ParentGuardians.AnyAsync(
                        p => p.Email == email && p.StudentId == students[studentIndex].StudentId, cancellationToken))
                    continue;

                var user = await EnsureUserAsync(userManager, email, fullName, customId, RoleNames.Parent, DefaultPassword);
                context.ParentGuardians.Add(new ParentGuardian
                {
                    FullName = fullName,
                    Email = email,
                    UserId = user.Id,
                    StudentId = students[studentIndex].StudentId
                });

                if (extraStudentIndex.HasValue
                    && extraStudentIndex.Value < students.Count
                    && !await context.ParentGuardians.AnyAsync(
                        p => p.UserId == user.Id && p.StudentId == students[extraStudentIndex.Value].StudentId,
                        cancellationToken))
                {
                    context.ParentGuardians.Add(new ParentGuardian
                    {
                        FullName = fullName,
                        Email = email,
                        UserId = user.Id,
                        StudentId = students[extraStudentIndex.Value].StudentId
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static IEnumerable<(string Email, string FullName, string CustomId, int StudentIndex, int? ExtraStudentIndex)> GetParentSpecs() =>
        [
            ("pr.artan.gashi@shkollademo.edu", "Artan Gashi", "PR001", 0, 14),
            ("pr.naim.krasniqi@shkollademo.edu", "Naim Krasniqi", "PR002", 1, null),
            ("pr.valbona.berisha@shkollademo.edu", "Valbona Berisha", "PR003", 2, 13),
            ("pr.agim.meta@shkollademo.edu", "Agim Meta", "PR004", 3, null),
            ("pr.luljeta.hoxha@shkollademo.edu", "Luljeta Hoxha", "PR005", 4, 10),
            ("pr.fatmir.rama@shkollademo.edu", "Fatmir Rama", "PR006", 5, 9),
            ("pr.klea.meta@shkollademo.edu", "Klea Meta", "PR007", 6, null),
            ("pr.luan.berisha@shkollademo.edu", "Luan Berisha", "PR008", 7, 16),
            ("pr.mira.gashi@shkollademo.edu", "Mira Gashi", "PR009", 8, null),
            ("pr.nora.rama@shkollademo.edu", "Nora Rama", "PR010", 9, null),
            ("pr.olen.hoxha@shkollademo.edu", "Olen Hoxha", "PR011", 10, 17),
            ("pr.petra.krasniqi@shkollademo.edu", "Petra Krasniqi", "PR012", 11, 15),
            ("pr.arben.morina@shkollademo.edu", "Arben Morina", "PR013", 12, null),
            ("pr.blerta.shala@shkollademo.edu", "Blerta Shala", "PR014", 13, null),
            ("pr.dren.gashi@shkollademo.edu", "Dren Gashi", "PR015", 14, null),
            ("pr.erza.krasniqi@shkollademo.edu", "Erza Krasniqi", "PR016", 15, null),
            ("pr.fisnik.berisha@shkollademo.edu", "Fisnik Berisha", "PR017", 16, null),
            ("pr.gentiana.hoxha@shkollademo.edu", "Gentiana Hoxha", "PR018", 17, null),
        ];

        private static async Task EnsureLessonSlotsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var slotDefs = new[]
            {
                (1, "Ora 1", "08:00", "08:45"),
                (2, "Ora 2", "08:50", "09:35"),
                (3, "Ora 3", "09:40", "10:25"),
                (4, "Ora 4", "10:30", "11:15"),
                (5, "Ora 5", "11:20", "12:05"),
                (6, "Ora 6", "12:10", "12:55"),
                (7, "Ora 7", "13:00", "13:45"),
                (8, "Ora 8", "13:50", "14:35"),
            };

            foreach (var (num, title, start, end) in slotDefs)
            {
                var existing = await context.LessonSlots.FirstOrDefaultAsync(l => l.SlotNumber == num, cancellationToken);
                if (existing == null)
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
                else
                {
                    existing.Title = title;
                    existing.StartTime = start;
                    existing.EndTime = end;
                    existing.IsActive = true;
                }
            }

            var lessonsSetting = await context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == SystemSettingKeys.LessonsPerDay, cancellationToken);
            if (lessonsSetting == null)
                context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = SystemSettingKeys.LessonsPerDay,
                    SettingValue = "8",
                    Description = "Lesson slots per school day"
                });
            else
                lessonsSetting.SettingValue = "8";

            await context.SaveChangesAsync(cancellationToken);
        }

        private static void AddGradesForEnrollments(
            ApplicationDbContext context,
            List<Enrollment> enrollments,
            DateTime termStart,
            HashSet<int> atRiskStudentIds,
            Random gradeRnd)
        {
            var gradeIndex = 0;
            foreach (var enrollment in enrollments)
            {
                var baseValue = atRiskStudentIds.Contains(enrollment.StudentId)
                    ? gradeRnd.Next(1, 3)
                    : gradeRnd.Next(3, 6);

                context.Grades.Add(new Grade
                {
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    Value = baseValue,
                    DateRecorded = termStart.AddDays(20 + (gradeIndex % 45))
                });
                context.Grades.Add(new Grade
                {
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    Value = Math.Clamp(baseValue + gradeRnd.Next(-1, 2), 1, 5),
                    DateRecorded = termStart.AddDays(50 + (gradeIndex % 30))
                });
                context.Grades.Add(new Grade
                {
                    StudentId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    Value = Math.Clamp(baseValue + gradeRnd.Next(-1, 1), 1, 5),
                    DateRecorded = termStart.AddDays(75 + (gradeIndex % 25))
                });
                if (gradeRnd.Next(0, 2) == 0)
                {
                    context.Grades.Add(new Grade
                    {
                        StudentId = enrollment.StudentId,
                        CourseId = enrollment.CourseId,
                        Value = Math.Clamp(baseValue + gradeRnd.Next(0, 2), 1, 5),
                        DateRecorded = termStart.AddDays(95 + (gradeIndex % 20))
                    });
                }

                gradeIndex++;
            }
        }

        private static async Task AddMissingGradesAsync(
            ApplicationDbContext context,
            List<Enrollment> enrollments,
            DateTime termStart,
            HashSet<int> atRiskStudentIds,
            CancellationToken cancellationToken)
        {
            var existingCounts = await context.Grades
                .GroupBy(g => new { g.StudentId, g.CourseId })
                .Select(g => new { g.Key.StudentId, g.Key.CourseId, Count = g.Count() })
                .ToDictionaryAsync(x => (x.StudentId, x.CourseId), x => x.Count, cancellationToken);

            var gradeRnd = new Random(84);
            var gradeIndex = existingCounts.Count;
            foreach (var enrollment in enrollments)
            {
                var key = (enrollment.StudentId, enrollment.CourseId);
                var have = existingCounts.GetValueOrDefault(key, 0);
                if (have >= 3)
                    continue;

                var baseValue = atRiskStudentIds.Contains(enrollment.StudentId)
                    ? gradeRnd.Next(1, 3)
                    : gradeRnd.Next(3, 6);

                for (var i = have; i < 3; i++)
                {
                    context.Grades.Add(new Grade
                    {
                        StudentId = enrollment.StudentId,
                        CourseId = enrollment.CourseId,
                        Value = Math.Clamp(baseValue + gradeRnd.Next(-1, 2), 1, 5),
                        DateRecorded = termStart.AddDays(30 + (gradeIndex % 60) + i * 15)
                    });
                }

                gradeIndex++;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureExtraCoursesAndEnrollmentsAsync(
            ApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            var extraCourses = new[]
            {
                ("NAT1", "Natyra", 1, "Klasa 1", "mesues.elena@shkollademo.edu", "Monday", "10:00", "Dhoma 101"),
                ("EDU2", "Edukatë Qytetare", 1, "Klasa 2", "mesues.dritan@shkollademo.edu", "Tuesday", "10:00", "Dhoma 202"),
                ("INF4", "Informatikë", 1, "Klasa 4", "mesues.mira@shkollademo.edu", "Thursday", "10:00", "Dhoma 404"),
                ("HIS5", "Histori", 1, "Klasa 5", "mesues.jon@shkollademo.edu", "Friday", "10:00", "Dhoma 505"),
            };

            var vitiShkollor = "Viti shkollor 2025-2026";
            foreach (var (code, name, credits, className, profEmail, day, time, room) in extraCourses)
            {
                if (await context.Courses.AnyAsync(c => c.CourseCode == code, cancellationToken))
                    continue;

                var dept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == className, cancellationToken);
                var prof = await context.Professors.FirstOrDefaultAsync(p => p.Email == profEmail, cancellationToken);

                if (dept == null || prof == null)
                    continue;

                var course = new Course
                {
                    CourseCode = code,
                    CourseName = name,
                    Credits = credits,
                    DepartmentId = dept.DepartmentId,
                    ProfessorId = prof.ProfessorId,
                    Semester = vitiShkollor,
                    MaxEnrollment = 28
                };
                context.Courses.Add(course);
                await context.SaveChangesAsync(cancellationToken);
                context.Schedules.Add(new Schedule { CourseId = course.CourseId, Day = day, Time = time, Room = room });

                var enrollmentAdds = new Dictionary<string, string[]>
                {
                    ["NAT1"] = ["nx.ana.gashi@shkollademo.edu", "nx.besnik.krasniqi@shkollademo.edu", "nx.arben.morina@shkollademo.edu", "nx.genta.hoxha@shkollademo.edu"],
                    ["EDU2"] = ["nx.elira.berisha@shkollademo.edu", "nx.florian.meta@shkollademo.edu", "nx.petra.krasniqi@shkollademo.edu", "nx.blerta.shala@shkollademo.edu"],
                    ["INF4"] = ["nx.klea.meta@shkollademo.edu", "nx.luan.berisha@shkollademo.edu", "nx.erza.krasniqi@shkollademo.edu"],
                    ["HIS5"] = ["nx.mira.gashi@shkollademo.edu", "nx.nora.rama@shkollademo.edu", "nx.fisnik.berisha@shkollademo.edu"],
                };

                if (!enrollmentAdds.TryGetValue(code, out var studentEmails))
                    continue;

                foreach (var email in studentEmails)
                {
                    var student = await context.Students.FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
                    if (student == null)
                        continue;
                    if (await context.Enrollments.AnyAsync(
                            e => e.StudentId == student.StudentId && e.CourseId == course.CourseId, cancellationToken))
                        continue;

                    context.Enrollments.Add(new Enrollment
                    {
                        StudentId = student.StudentId,
                        CourseId = course.CourseId,
                        EnrollmentDate = DateTime.UtcNow.AddDays(-25)
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SyncParentDemoPasswordsAsync(UserManager<ApplicationUser> userManager)
        {
            foreach (var (email, _, _, _, _) in GetParentSpecs())
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                    continue;
                await SyncDemoPasswordAsync(userManager, user, DefaultPassword);
                await userManager.ResetAccessFailedCountAsync(user);
                await userManager.SetLockoutEndDateAsync(user, null);
            }
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
            "pr.artan.gashi@shkollademo.edu", "pr.luljeta.hoxha@shkollademo.edu",
            "pr.naim.krasniqi@shkollademo.edu", "pr.valbona.berisha@shkollademo.edu",
            "pr.agim.meta@shkollademo.edu", "pr.fatmir.rama@shkollademo.edu",
            "pr.klea.meta@shkollademo.edu", "pr.luan.berisha@shkollademo.edu",
            "pr.mira.gashi@shkollademo.edu", "pr.nora.rama@shkollademo.edu",
            "pr.olen.hoxha@shkollademo.edu", "pr.petra.krasniqi@shkollademo.edu",
            "pr.arben.morina@shkollademo.edu", "pr.blerta.shala@shkollademo.edu",
            "pr.dren.gashi@shkollademo.edu", "pr.erza.krasniqi@shkollademo.edu",
            "pr.fisnik.berisha@shkollademo.edu", "pr.gentiana.hoxha@shkollademo.edu",
            "nx.arben.morina@shkollademo.edu", "nx.blerta.shala@shkollademo.edu",
            "nx.dren.gashi@shkollademo.edu", "nx.erza.krasniqi@shkollademo.edu",
            "nx.fisnik.berisha@shkollademo.edu", "nx.genta.hoxha@shkollademo.edu"
        ];

        private static async Task SeedDemoAdminActivityAsync(
            ApplicationDbContext context,
            ApplicationUser adminUser,
            List<Student> students,
            Professor[] professors,
            CancellationToken cancellationToken)
        {
            if (await context.AdminActivityLogs.AnyAsync(cancellationToken))
                return;

            var rnd = new Random(77);
            var actions = new[]
            {
                AdminActions.Create, AdminActions.Update, AdminActions.ReportExport,
                AdminActions.GradeOverride, AdminActions.AtRiskNote, AdminActions.CalendarChange
            };

            for (var i = 0; i < 48; i++)
            {
                var student = students[rnd.Next(students.Count)];
                var professor = professors[rnd.Next(professors.Length)];
                var useStudent = rnd.Next(2) == 0;
                context.AdminActivityLogs.Add(new AdminActivityLog
                {
                    UserId = adminUser.Id,
                    Email = adminUser.Email,
                    Action = actions[rnd.Next(actions.Length)],
                    EntityType = rnd.Next(3) switch
                    {
                        0 => AuditEntityTypes.Grade,
                        1 => AuditEntityTypes.Report,
                        _ => AuditEntityTypes.InterventionNote
                    },
                    EntityId = useStudent ? student.StudentId.ToString() : professor.ProfessorId.ToString(),
                    Details = useStudent
                        ? $"Nxënës: {student.FullName} ({student.StudentNumber})"
                        : $"Mësues: {professor.FullName}",
                    IpAddress = "127.0.0.1",
                    Country = "Kosovo",
                    City = "Prishtinë",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-rnd.Next(2, 240))
                });
            }
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
                .Append(SystemAccountEmails.Admin)
                .Append(SystemAccountEmails.SuperAdmin)
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
            await Upsert(SystemSettingKeys.LessonsPerDay, "8", "Lesson slots per school day");
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

        private static async Task MigrateLegacyAdminEmailsAsync(UserManager<ApplicationUser> userManager)
        {
            foreach (var (legacy, current) in SystemAccountEmails.GetLegacyMigrations())
            {
                var legacyUser = await userManager.FindByEmailAsync(legacy);
                if (legacyUser == null)
                    continue;

                var currentUser = await userManager.FindByEmailAsync(current);
                if (currentUser != null)
                {
                    var delete = await userManager.DeleteAsync(legacyUser);
                    if (delete.Succeeded)
                        Console.WriteLine($"  Removed legacy account {legacy} ({current} already exists).");
                    continue;
                }

                legacyUser.Email = current;
                legacyUser.UserName = current;
                legacyUser.NormalizedEmail = current.ToUpperInvariant();
                legacyUser.NormalizedUserName = current.ToUpperInvariant();
                var update = await userManager.UpdateAsync(legacyUser);
                if (!update.Succeeded)
                {
                    Console.WriteLine($"  Warning: could not migrate {legacy} to {current}: {string.Join("; ", update.Errors.Select(e => e.Description))}");
                    continue;
                }

                await SyncDemoPasswordAsync(userManager, legacyUser, DefaultPassword);
                Console.WriteLine($"  Migrated {legacy} -> {current}");
            }
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

using Microsoft.EntityFrameworkCore;

namespace UM_Project.Data
{
    /// <summary>Applies additive schema changes when SQL scripts were not run manually.</summary>
    public static class DatabaseSchemaBootstrap
    {
        public static async Task ApplyAsync(ApplicationDbContext context)
        {
            await EnsureAspNetUserColumnsAsync(context);
            await EnsureCourseColumnsAsync(context);
            await EnsureCourseDocumentsTableAsync(context);
            await EnsureAttendanceAndSettingsAsync(context);
            await EnsureAuthAndAuditTablesAsync(context);
            await EnsureParentGuardiansTableAsync(context);
            await EnsureExtendedFeaturesAsync(context);
            await SeedDefaultsAsync(context);
        }

        private static async Task EnsureAspNetUserColumnsAsync(ApplicationDbContext context)
        {
            if (!await ColumnExistsAsync(context, "AspNetUsers", "MustChangePassword"))
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE `AspNetUsers` ADD COLUMN `MustChangePassword` tinyint(1) NOT NULL DEFAULT 0");
        }

        private static async Task EnsureAuthAndAuditTablesAsync(ApplicationDbContext context)
        {
            if (!await TableExistsAsync(context, "AuthAuditLogs"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `AuthAuditLogs` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `UserId` longtext NULL,
                        `Email` longtext NULL,
                        `EventType` longtext NOT NULL,
                        `Success` tinyint(1) NOT NULL,
                        `FailureReason` longtext NULL,
                        `IpAddress` longtext NULL,
                        `Country` longtext NULL,
                        `Region` longtext NULL,
                        `City` longtext NULL,
                        `UserAgent` longtext NULL,
                        `CreatedAtUtc` datetime(6) NOT NULL,
                        PRIMARY KEY (`Id`),
                        KEY `IX_AuthAuditLogs_CreatedAtUtc` (`CreatedAtUtc`)
                    )
                    """);
            }
            else
            {
                if (!await ColumnExistsAsync(context, "AuthAuditLogs", "Country"))
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE `AuthAuditLogs` ADD COLUMN `Country` longtext NULL");
                if (!await ColumnExistsAsync(context, "AuthAuditLogs", "Region"))
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE `AuthAuditLogs` ADD COLUMN `Region` longtext NULL");
                if (!await ColumnExistsAsync(context, "AuthAuditLogs", "City"))
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE `AuthAuditLogs` ADD COLUMN `City` longtext NULL");
            }

            if (!await TableExistsAsync(context, "RefreshTokens"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `RefreshTokens` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `UserId` longtext NOT NULL,
                        `TokenHash` varchar(64) NOT NULL,
                        `JwtId` longtext NOT NULL,
                        `CreatedAtUtc` datetime(6) NOT NULL,
                        `ExpiresAtUtc` datetime(6) NOT NULL,
                        `RevokedAtUtc` datetime(6) NULL,
                        `ReplacedByTokenHash` longtext NULL,
                        `RevokedReason` longtext NULL,
                        `CreatedByIp` longtext NULL,
                        PRIMARY KEY (`Id`),
                        UNIQUE KEY `IX_RefreshTokens_TokenHash` (`TokenHash`)
                    )
                    """);
            }

            if (!await TableExistsAsync(context, "AdminActivityLogs"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `AdminActivityLogs` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `UserId` longtext NULL,
                        `Email` longtext NULL,
                        `Action` longtext NOT NULL,
                        `EntityType` longtext NOT NULL,
                        `EntityId` longtext NULL,
                        `Details` longtext NULL,
                        `IpAddress` longtext NULL,
                        `Country` longtext NULL,
                        `Region` longtext NULL,
                        `City` longtext NULL,
                        `UserAgent` longtext NULL,
                        `CreatedAtUtc` datetime(6) NOT NULL,
                        PRIMARY KEY (`Id`),
                        KEY `IX_AdminActivityLogs_CreatedAtUtc` (`CreatedAtUtc`)
                    )
                    """);
            }
        }

        private static async Task EnsureParentGuardiansTableAsync(ApplicationDbContext context)
        {
            if (await TableExistsAsync(context, "ParentGuardians"))
                return;

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE `ParentGuardians` (
                    `ParentId` int NOT NULL AUTO_INCREMENT,
                    `FullName` longtext NOT NULL,
                    `Email` longtext NOT NULL,
                    `UserId` longtext NOT NULL,
                    `StudentId` int NOT NULL,
                    PRIMARY KEY (`ParentId`),
                    KEY `IX_ParentGuardians_StudentId` (`StudentId`),
                    CONSTRAINT `FK_ParentGuardians_Students_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Students` (`StudentId`) ON DELETE CASCADE
                )
                """);
        }

        private static async Task EnsureCourseColumnsAsync(ApplicationDbContext context)
        {
            var columns = new (string Name, string Sql)[]
            {
                ("Description", "ALTER TABLE `Courses` ADD COLUMN `Description` longtext NULL"),
                ("LearningOutcomes", "ALTER TABLE `Courses` ADD COLUMN `LearningOutcomes` longtext NULL"),
                ("Prerequisites", "ALTER TABLE `Courses` ADD COLUMN `Prerequisites` longtext NULL"),
                ("Semester", "ALTER TABLE `Courses` ADD COLUMN `Semester` longtext NULL"),
                ("MaxEnrollment", "ALTER TABLE `Courses` ADD COLUMN `MaxEnrollment` int NULL"),
                ("AdditionalNotes", "ALTER TABLE `Courses` ADD COLUMN `AdditionalNotes` longtext NULL"),
            };

            foreach (var (name, sql) in columns)
            {
                if (await ColumnExistsAsync(context, "Courses", name))
                    continue;
                await context.Database.ExecuteSqlRawAsync(sql);
            }
        }

        private static async Task EnsureCourseDocumentsTableAsync(ApplicationDbContext context)
        {
            if (await TableExistsAsync(context, "CourseDocuments"))
                return;

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE `CourseDocuments` (
                    `CourseDocumentId` int NOT NULL AUTO_INCREMENT,
                    `CourseId` int NOT NULL,
                    `Title` longtext NOT NULL,
                    `FileName` longtext NOT NULL,
                    `StoredPath` longtext NOT NULL,
                    `Description` longtext NULL,
                    `UploadedAtUtc` datetime(6) NOT NULL,
                    PRIMARY KEY (`CourseDocumentId`),
                    KEY `IX_CourseDocuments_CourseId` (`CourseId`)
                )
                """);
        }

        private static async Task<bool> ColumnExistsAsync(ApplicationDbContext context, string table, string column)
        {
            var result = await context.Database
                .SqlQueryRaw<int>(
                    """
                    SELECT COUNT(*) AS Value FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {0} AND COLUMN_NAME = {1}
                    """,
                    table, column)
                .FirstOrDefaultAsync();
            return result > 0;
        }

        private static async Task EnsureAttendanceAndSettingsAsync(ApplicationDbContext context)
        {
            if (!await TableExistsAsync(context, "LessonSlots"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `LessonSlots` (
                        `LessonSlotId` int NOT NULL AUTO_INCREMENT,
                        `SlotNumber` int NOT NULL,
                        `Title` longtext NOT NULL,
                        `StartTime` longtext NOT NULL,
                        `EndTime` longtext NOT NULL,
                        `IsActive` tinyint(1) NOT NULL DEFAULT 1,
                        PRIMARY KEY (`LessonSlotId`),
                        UNIQUE KEY `IX_LessonSlots_SlotNumber` (`SlotNumber`)
                    )
                    """);
            }

            if (!await TableExistsAsync(context, "SystemSettings"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `SystemSettings` (
                        `SystemSettingId` int NOT NULL AUTO_INCREMENT,
                        `SettingKey` longtext NOT NULL,
                        `SettingValue` longtext NOT NULL,
                        `Description` longtext NULL,
                        PRIMARY KEY (`SystemSettingId`)
                    )
                    """);
            }

            if (!await TableExistsAsync(context, "AttendanceRecords"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `AttendanceRecords` (
                        `AttendanceId` int NOT NULL AUTO_INCREMENT,
                        `StudentId` int NOT NULL,
                        `CourseId` int NOT NULL,
                        `LessonSlotId` int NOT NULL,
                        `AttendanceDate` date NOT NULL,
                        `IsPresent` tinyint(1) NOT NULL,
                        `ProfessorId` int NOT NULL,
                        `MarkedAtUtc` datetime(6) NOT NULL,
                        `Notes` longtext NULL,
                        PRIMARY KEY (`AttendanceId`),
                        UNIQUE KEY `IX_Attendance_Student_Course_Slot_Date` (`StudentId`,`CourseId`,`LessonSlotId`,`AttendanceDate`)
                    )
                    """);
            }
        }

        private static async Task EnsureExtendedFeaturesAsync(ApplicationDbContext context)
        {
            if (!await TableExistsAsync(context, "AcademicTerms"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `AcademicTerms` (
                        `AcademicTermId` int NOT NULL AUTO_INCREMENT,
                        `Name` longtext NOT NULL,
                        `StartDate` date NOT NULL,
                        `EndDate` date NOT NULL,
                        `IsActive` tinyint(1) NOT NULL DEFAULT 1,
                        PRIMARY KEY (`AcademicTermId`)
                    )
                    """);
            }
            if (!await TableExistsAsync(context, "SchoolCalendarEvents"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `SchoolCalendarEvents` (
                        `SchoolCalendarEventId` int NOT NULL AUTO_INCREMENT,
                        `EventDate` date NOT NULL,
                        `Title` longtext NOT NULL,
                        `EventType` longtext NOT NULL,
                        `BlocksAttendance` tinyint(1) NOT NULL DEFAULT 1,
                        `AcademicTermId` int NULL,
                        `Notes` longtext NULL,
                        PRIMARY KEY (`SchoolCalendarEventId`),
                        KEY `IX_SchoolCalendarEvents_EventDate` (`EventDate`)
                    )
                    """);
            }
            if (!await TableExistsAsync(context, "CourseAssignments"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `CourseAssignments` (
                        `CourseAssignmentId` int NOT NULL AUTO_INCREMENT,
                        `CourseId` int NOT NULL,
                        `Title` longtext NOT NULL,
                        `Description` longtext NULL,
                        `DueDate` datetime(6) NOT NULL,
                        `WeightPercent` decimal(5,2) NOT NULL,
                        `MaxPoints` int NULL,
                        `CreatedAtUtc` datetime(6) NOT NULL,
                        PRIMARY KEY (`CourseAssignmentId`),
                        KEY `IX_CourseAssignments_CourseId` (`CourseId`)
                    )
                    """);
            }
            if (!await TableExistsAsync(context, "DocumentRequests"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `DocumentRequests` (
                        `DocumentRequestId` int NOT NULL AUTO_INCREMENT,
                        `RequestedByUserId` longtext NOT NULL,
                        `StudentId` int NOT NULL,
                        `RequestType` longtext NOT NULL,
                        `Status` longtext NOT NULL,
                        `ParentNotes` longtext NULL,
                        `AdminNotes` longtext NULL,
                        `RequestedAtUtc` datetime(6) NOT NULL,
                        `ProcessedAtUtc` datetime(6) NULL,
                        `ProcessedByUserId` longtext NULL,
                        PRIMARY KEY (`DocumentRequestId`)
                    )
                    """);
            }
            if (!await TableExistsAsync(context, "InterventionNotes"))
            {
                await context.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE `InterventionNotes` (
                        `InterventionNoteId` int NOT NULL AUTO_INCREMENT,
                        `StudentId` int NOT NULL,
                        `Note` longtext NOT NULL,
                        `CreatedByUserId` longtext NOT NULL,
                        `CreatedAtUtc` datetime(6) NOT NULL,
                        PRIMARY KEY (`InterventionNoteId`)
                    )
                    """);
            }
        }

        private static async Task SeedDefaultsAsync(ApplicationDbContext context)
        {
            async Task EnsureSetting(string key, string value, string desc)
            {
                if (!await context.SystemSettings.AnyAsync(s => s.SettingKey == key))
                    context.SystemSettings.Add(new Models.SystemSetting { SettingKey = key, SettingValue = value, Description = desc });
            }

            await EnsureSetting(Models.SystemSettingKeys.GradeMinimum, "1", "Lowest grade");
            await EnsureSetting(Models.SystemSettingKeys.GradeMaximum, "5", "Highest grade");
            await EnsureSetting(Models.SystemSettingKeys.GradePassingMinimum, "3", "Passing grade");
            await EnsureSetting(Models.SystemSettingKeys.LessonsPerDay, "6", "Lessons per day");
            await EnsureSetting(Models.SystemSettingKeys.AtRiskAbsenceDays, "30", "At-risk absence window days");
            await EnsureSetting(Models.SystemSettingKeys.AtRiskAbsenceCount, "5", "At-risk absence threshold");
            await context.SaveChangesAsync();

            if (!await context.LessonSlots.AnyAsync())
            {
                var defaults = new (int num, string title, string start, string end)[]
                {
                    (1, "Lesson 1", "08:00", "09:30"),
                    (2, "Lesson 2", "09:45", "11:15"),
                    (3, "Lesson 3", "11:30", "13:00"),
                    (4, "Lesson 4", "13:30", "15:00"),
                    (5, "Lesson 5", "15:15", "16:45"),
                    (6, "Lesson 6", "17:00", "18:30")
                };
                foreach (var d in defaults)
                {
                    context.LessonSlots.Add(new Models.LessonSlot
                    {
                        SlotNumber = d.num,
                        Title = d.title,
                        StartTime = d.start,
                        EndTime = d.end,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }

            if (!await context.AcademicTerms.AnyAsync())
            {
                var year = DateTime.UtcNow.Year;
                context.AcademicTerms.Add(new Models.AcademicTerm
                {
                    Name = $"Fall {year}",
                    StartDate = new DateTime(year, 9, 1),
                    EndDate = new DateTime(year, 12, 20),
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task<bool> TableExistsAsync(ApplicationDbContext context, string table)
        {
            var result = await context.Database
                .SqlQueryRaw<int>(
                    """
                    SELECT COUNT(*) AS Value FROM information_schema.TABLES
                    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {0}
                    """,
                    table)
                .FirstOrDefaultAsync();
            return result > 0;
        }
    }
}

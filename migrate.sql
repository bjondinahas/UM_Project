CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `AspNetRoles` (
    `Id` varchar(255) NOT NULL,
    `Name` varchar(256) NULL,
    `NormalizedName` varchar(256) NULL,
    `ConcurrencyStamp` longtext NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetUsers` (
    `Id` varchar(255) NOT NULL,
    `FullName` longtext NOT NULL,
    `CustomId` longtext NOT NULL,
    `Address` longtext NOT NULL,
    `UserName` varchar(256) NULL,
    `NormalizedUserName` varchar(256) NULL,
    `Email` varchar(256) NULL,
    `NormalizedEmail` varchar(256) NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext NULL,
    `SecurityStamp` longtext NULL,
    `ConcurrencyStamp` longtext NULL,
    `PhoneNumber` longtext NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Departments` (
    `DepartmentId` int NOT NULL AUTO_INCREMENT,
    `DepartmentName` longtext NOT NULL,
    PRIMARY KEY (`DepartmentId`)
);

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(255) NOT NULL,
    `ProviderKey` varchar(255) NOT NULL,
    `ProviderDisplayName` longtext NULL,
    `UserId` varchar(255) NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(255) NOT NULL,
    `RoleId` varchar(255) NOT NULL,
    PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(255) NOT NULL,
    `LoginProvider` varchar(255) NOT NULL,
    `Name` varchar(255) NOT NULL,
    `Value` longtext NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Professors` (
    `ProfessorId` int NOT NULL AUTO_INCREMENT,
    `FullName` longtext NOT NULL,
    `Email` longtext NOT NULL,
    `Title` longtext NOT NULL,
    `DepartmentId` int NOT NULL,
    `UserId` longtext NOT NULL,
    PRIMARY KEY (`ProfessorId`),
    CONSTRAINT `FK_Professors_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`DepartmentId`) ON DELETE CASCADE
);

CREATE TABLE `Students` (
    `StudentId` int NOT NULL AUTO_INCREMENT,
    `FullName` longtext NOT NULL,
    `Email` longtext NOT NULL,
    `StudentNumber` longtext NOT NULL,
    `DepartmentId` int NOT NULL,
    `UserId` longtext NOT NULL,
    PRIMARY KEY (`StudentId`),
    CONSTRAINT `FK_Students_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`DepartmentId`) ON DELETE CASCADE
);

CREATE TABLE `Courses` (
    `CourseId` int NOT NULL AUTO_INCREMENT,
    `CourseCode` longtext NOT NULL,
    `CourseName` longtext NOT NULL,
    `Credits` int NOT NULL,
    `DepartmentId` int NOT NULL,
    `ProfessorId` int NOT NULL,
    PRIMARY KEY (`CourseId`),
    CONSTRAINT `FK_Courses_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`DepartmentId`) ON DELETE CASCADE,
    CONSTRAINT `FK_Courses_Professors_ProfessorId` FOREIGN KEY (`ProfessorId`) REFERENCES `Professors` (`ProfessorId`) ON DELETE CASCADE
);

CREATE TABLE `Enrollments` (
    `EnrollmentId` int NOT NULL AUTO_INCREMENT,
    `StudentId` int NOT NULL,
    `CourseId` int NOT NULL,
    `EnrollmentDate` datetime(6) NOT NULL,
    PRIMARY KEY (`EnrollmentId`),
    CONSTRAINT `FK_Enrollments_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`) ON DELETE CASCADE,
    CONSTRAINT `FK_Enrollments_Students_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Students` (`StudentId`) ON DELETE CASCADE
);

CREATE TABLE `Grades` (
    `GradeId` int NOT NULL AUTO_INCREMENT,
    `StudentId` int NOT NULL,
    `CourseId` int NOT NULL,
    `Value` int NOT NULL,
    `DateRecorded` datetime(6) NOT NULL,
    PRIMARY KEY (`GradeId`),
    CONSTRAINT `FK_Grades_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`) ON DELETE CASCADE,
    CONSTRAINT `FK_Grades_Students_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Students` (`StudentId`) ON DELETE CASCADE
);

CREATE TABLE `Schedules` (
    `ScheduleId` int NOT NULL AUTO_INCREMENT,
    `CourseId` int NOT NULL,
    `Day` varchar(255) NOT NULL,
    `Time` varchar(255) NOT NULL,
    `Room` longtext NOT NULL,
    PRIMARY KEY (`ScheduleId`),
    CONSTRAINT `FK_Schedules_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`) ON DELETE CASCADE
);

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_Courses_DepartmentId` ON `Courses` (`DepartmentId`);

CREATE INDEX `IX_Courses_ProfessorId` ON `Courses` (`ProfessorId`);

CREATE INDEX `IX_Enrollments_CourseId` ON `Enrollments` (`CourseId`);

CREATE INDEX `IX_Enrollments_StudentId` ON `Enrollments` (`StudentId`);

CREATE INDEX `IX_Grades_CourseId` ON `Grades` (`CourseId`);

CREATE INDEX `IX_Grades_StudentId` ON `Grades` (`StudentId`);

CREATE INDEX `IX_Professors_DepartmentId` ON `Professors` (`DepartmentId`);

CREATE UNIQUE INDEX `IX_Schedules_CourseId_Day_Time` ON `Schedules` (`CourseId`, `Day`, `Time`);

CREATE INDEX `IX_Students_DepartmentId` ON `Students` (`DepartmentId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260423132815_InitialCreate', '10.0.1');

COMMIT;


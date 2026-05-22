-- Optional manual script (app also auto-applies on startup)

CREATE TABLE IF NOT EXISTS `LessonSlots` (
    `LessonSlotId` int NOT NULL AUTO_INCREMENT,
    `SlotNumber` int NOT NULL,
    `Title` longtext NOT NULL,
    `StartTime` longtext NOT NULL,
    `EndTime` longtext NOT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`LessonSlotId`),
    UNIQUE KEY `IX_LessonSlots_SlotNumber` (`SlotNumber`)
);

CREATE TABLE IF NOT EXISTS `SystemSettings` (
    `SystemSettingId` int NOT NULL AUTO_INCREMENT,
    `SettingKey` longtext NOT NULL,
    `SettingValue` longtext NOT NULL,
    `Description` longtext NULL,
    PRIMARY KEY (`SystemSettingId`)
);

CREATE TABLE IF NOT EXISTS `AttendanceRecords` (
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
);

INSERT IGNORE INTO `SystemSettings` (`SettingKey`, `SettingValue`, `Description`) VALUES
('GradeMinimum', '5', 'Lowest grade'),
('GradeMaximum', '10', 'Highest grade'),
('GradePassingMinimum', '6', 'Passing grade'),
('LessonsPerDay', '6', 'Lessons per day');

INSERT IGNORE INTO `LessonSlots` (`SlotNumber`, `Title`, `StartTime`, `EndTime`, `IsActive`) VALUES
(1, 'Lesson 1', '08:00', '09:30', 1),
(2, 'Lesson 2', '09:45', '11:15', 1),
(3, 'Lesson 3', '11:30', '13:00', 1),
(4, 'Lesson 4', '13:30', '15:00', 1),
(5, 'Lesson 5', '15:15', '16:45', 1),
(6, 'Lesson 6', '17:00', '18:30', 1);

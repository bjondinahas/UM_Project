-- Run once in phpMyAdmin on UM_ProjectDB

ALTER TABLE `Courses`
    ADD COLUMN `Description` longtext NULL,
    ADD COLUMN `LearningOutcomes` longtext NULL,
    ADD COLUMN `Prerequisites` longtext NULL,
    ADD COLUMN `Semester` longtext NULL,
    ADD COLUMN `MaxEnrollment` int NULL,
    ADD COLUMN `AdditionalNotes` longtext NULL;

CREATE TABLE IF NOT EXISTS `CourseDocuments` (
    `CourseDocumentId` int NOT NULL AUTO_INCREMENT,
    `CourseId` int NOT NULL,
    `Title` longtext NOT NULL,
    `FileName` longtext NOT NULL,
    `StoredPath` longtext NOT NULL,
    `Description` longtext NULL,
    `UploadedAtUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`CourseDocumentId`),
    KEY `IX_CourseDocuments_CourseId` (`CourseId`)
);

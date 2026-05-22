CREATE TABLE IF NOT EXISTS `ParentGuardians` (
    `ParentId` int NOT NULL AUTO_INCREMENT,
    `FullName` longtext NOT NULL,
    `Email` longtext NOT NULL,
    `UserId` longtext NOT NULL,
    `StudentId` int NOT NULL,
    PRIMARY KEY (`ParentId`),
    KEY `IX_ParentGuardians_StudentId` (`StudentId`),
    CONSTRAINT `FK_ParentGuardians_Students_StudentId` FOREIGN KEY (`StudentId`) REFERENCES `Students` (`StudentId`) ON DELETE CASCADE
);

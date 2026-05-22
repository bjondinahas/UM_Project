-- Run once in phpMyAdmin on UM_ProjectDB (ignore errors if columns already exist)

ALTER TABLE `AuthAuditLogs` ADD COLUMN `Country` longtext NULL;
ALTER TABLE `AuthAuditLogs` ADD COLUMN `Region` longtext NULL;
ALTER TABLE `AuthAuditLogs` ADD COLUMN `City` longtext NULL;

CREATE TABLE IF NOT EXISTS `AdminActivityLogs` (
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
    KEY `IX_AdminActivityLogs_CreatedAtUtc` (`CreatedAtUtc`),
    KEY `IX_AdminActivityLogs_EntityType` (`EntityType`(255)),
    KEY `IX_AdminActivityLogs_Email` (`Email`(255))
);

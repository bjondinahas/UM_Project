-- Run in phpMyAdmin or: mysql -u root UM_ProjectDB < auth_security.sql

CREATE TABLE IF NOT EXISTS `AuthAuditLogs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` longtext NULL,
    `Email` longtext NULL,
    `EventType` longtext NOT NULL,
    `Success` tinyint(1) NOT NULL,
    `FailureReason` longtext NULL,
    `IpAddress` longtext NULL,
    `UserAgent` longtext NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_AuthAuditLogs_CreatedAtUtc` (`CreatedAtUtc`),
    KEY `IX_AuthAuditLogs_EventType` (`EventType`(255)),
    KEY `IX_AuthAuditLogs_Email` (`Email`(255))
);

CREATE TABLE IF NOT EXISTS `RefreshTokens` (
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
    UNIQUE KEY `IX_RefreshTokens_TokenHash` (`TokenHash`),
    KEY `IX_RefreshTokens_UserId` (`UserId`(255)),
    KEY `IX_RefreshTokens_UserId_ExpiresAtUtc` (`UserId`(255), `ExpiresAtUtc`)
);

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260521120000_AddAuthSecurity', '10.0.1');

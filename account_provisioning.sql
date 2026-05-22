-- Run once in phpMyAdmin on UM_ProjectDB

ALTER TABLE `AspNetUsers` ADD COLUMN `MustChangePassword` tinyint(1) NOT NULL DEFAULT 0;

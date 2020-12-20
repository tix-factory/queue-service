USE `queue`;

CREATE TABLE IF NOT EXISTS `queue-items` (
	`ID` BIGINT NOT NULL AUTO_INCREMENT,
	`QueueID` BIGINT NOT NULL,
	`Data` TEXT NOT NULL,
	`HolderID` VARCHAR(36) NULL,
	`LockExpiration` DATETIME NOT NULL,
	`Updated` DATETIME NOT NULL,
	`Created` DATETIME NOT NULL,

	PRIMARY KEY (`ID`),
	CONSTRAINT `UC_HolderID` UNIQUE(`HolderID`),
	INDEX `IX_QueueIDLockExpiration` (`QueueID`, `LockExpiration`)
	-- With this constraint the database runs into extreme locking issues under heavy load.
	-- FOREIGN KEY `FK_QueueItems_Queues_QueueID` (`QueueID`) REFERENCES `queues`(`ID`) ON DELETE CASCADE
);

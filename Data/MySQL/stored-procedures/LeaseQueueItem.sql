DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `LeaseQueueItem`(
	IN _QueueID BIGINT,
	IN _LockExpiration DATETIME
)
BEGIN
	SET @HolderID = UUID();

	UPDATE `queue`.`queue-items`
	SET
		`HolderID` = @HolderID,
		`LockExpiration` = _LockExpiration,
		`Updated` = UTC_Timestamp()
	WHERE (`QueueID` = _QueueID)
		AND (`LockExpiration` < UTC_Timestamp())
	ORDER BY `ID` ASC
	LIMIT 1;

	SELECT *
		FROM `queue`.`queue-items`
		WHERE `HolderID` = @HolderID
		LIMIT 1;
END$$

DELIMITER ;
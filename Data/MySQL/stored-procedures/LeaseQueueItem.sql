DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `LeaseQueueItem`(
	IN _QueueID BIGINT,
	IN _LockExpiration DATETIME
)
BEGIN
	START TRANSACTION;

	SET @HolderID = UUID();
	SET @ID = (SELECT `ID`
		FROM `queue`.`queue-items`
		WHERE (`QueueID` = _QueueID)
		AND (`LockExpiration` < UTC_Timestamp())
		ORDER BY `ID` ASC
		LIMIT 1
		FOR UPDATE);

	UPDATE `queue`.`queue-items`
	SET
		`HolderID` = @HolderID,
		`LockExpiration` = _LockExpiration,
		`Updated` = UTC_Timestamp()
	WHERE (`ID` = @ID)
	LIMIT 1;

	SELECT *
		FROM `queue`.`queue-items`
		WHERE `ID` = @ID
		LIMIT 1;

	COMMIT;
END$$

DELIMITER ;
DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `DeleteQueueItem`(
	IN _ID BIGINT,
	IN _HolderID VARCHAR(36)
)
BEGIN
	DELETE
		FROM `queue`.`queue-items`
		WHERE (`ID` = _ID)
		AND (`HolderID` = _HolderID)
		AND (`LockExpiration` >= UTC_Timestamp())
		LIMIT 1;
END$$

DELIMITER ;
DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `ReleaseQueueItem`(
	IN _ID BIGINT,
	IN _HolderID VARCHAR(36)
)
BEGIN
	UPDATE `queue`.`queue-items`
	SET
		`HolderID` = NULL,
		`LockExpiration` = UTC_Timestamp(),
		`Updated` = UTC_Timestamp()
	WHERE (`ID` = _ID)
	AND (`HolderID` = _HolderID)
	AND (`LockExpiration` >= UTC_Timestamp())
	LIMIT 1;
END$$

DELIMITER ;
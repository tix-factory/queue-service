DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `GetHeldQueueSize`(
	IN _QueueID BIGINT
)
BEGIN
	SELECT COUNT(*) as `Count`
		FROM `queue`.`queue-items`
		WHERE (`QueueID` = _QueueID)
		AND (`HolderID` IS NOT NULL)
		AND (`LockExpiration` > UTC_Timestamp());
END$$

DELIMITER ;
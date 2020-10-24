DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `GetQueueSize`(
	IN _QueueID BIGINT
)
BEGIN
	SELECT COUNT(*) as `Count`
		FROM `queue`.`queue-items`
		WHERE `QueueID` = _QueueID;
END$$

DELIMITER ;
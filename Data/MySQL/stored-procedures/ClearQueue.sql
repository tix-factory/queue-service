DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `ClearQueue`(
	IN _QueueID BIGINT
)
BEGIN
	DELETE
		FROM `queue`.`queue-items`
		WHERE (`QueueID` = _QueueID);
END$$

DELIMITER ;
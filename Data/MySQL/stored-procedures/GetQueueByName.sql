DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `GetQueueByName`(
	IN _Name VARCHAR(128)
)
BEGIN
	SELECT *
		FROM `queue`.`queues`
		WHERE `Name` = _Name
		LIMIT 1;
END$$

DELIMITER ;
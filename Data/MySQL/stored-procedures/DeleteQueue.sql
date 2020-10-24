DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `DeleteQueue`(
	IN _ID BIGINT
)
BEGIN
	DELETE
		FROM `queue`.`queues`
		WHERE (`ID` = _ID)
		LIMIT 1;
END$$

DELIMITER ;
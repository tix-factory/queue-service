DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `InsertQueue`(
	IN _Name VARCHAR(128)
)
BEGIN
	INSERT INTO `queue`.`queues`
	(
		`Name`,
		`Created`,
		`Updated`
	)
	VALUES
	(
		_Name,
		UTC_Timestamp(),
		UTC_Timestamp()
	);
	
	SELECT LAST_INSERT_ID() as `ID`;
END$$

DELIMITER ;
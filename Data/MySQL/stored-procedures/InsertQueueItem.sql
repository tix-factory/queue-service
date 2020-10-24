DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `InsertQueueItem`(
	IN _QueueID BIGINT,
	IN _Data TEXT
)
BEGIN
	INSERT INTO `queue`.`queue-items`
	(
		`QueueID`,
		`Data`,
		`HolderID`,
		`LockExpiration`,
		`Created`,
		`Updated`
	)
	VALUES
	(
		_QueueID,
		_Data,
		NULL,
		UTC_Timestamp(),
		UTC_Timestamp(),
		UTC_Timestamp()
	);
	
	SELECT LAST_INSERT_ID() as `ID`;
END$$

DELIMITER ;
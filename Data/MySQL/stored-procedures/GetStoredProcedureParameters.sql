DELIMITER $$
USE `queue`$$
CREATE PROCEDURE `GetStoredProcedureParameters`(
	IN _StoredProcedureName VARCHAR(128)
)
BEGIN
	SELECT *
		FROM `information_schema`.`parameters`
		WHERE `SPECIFIC_NAME` = _StoredProcedureName
        ORDER BY `ORDINAL_POSITION` ASC;
END$$

DELIMITER ;
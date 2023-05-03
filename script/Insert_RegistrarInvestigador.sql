DELIMITER $$

USE `bd`$$

DROP PROCEDURE IF EXISTS `Insert_RegistrarInvestigador`$$

CREATE DEFINER=`hgarcia`@`%` PROCEDURE `Insert_RegistrarInvestigador`(parempresa VARCHAR(4),
parNoDocumento VARCHAR(15),
parTipoDocumento VARCHAR(30),
parCiudadExp VARCHAR(60),
parNombres VARCHAR(60),
parApellidos VARCHAR(60),
parCorreoInvest VARCHAR(300),
parAgenciaInvest VARCHAR(30),
parAseguradora VARCHAR(30),
parFirmaDigital MEDIUMBLOB)
BEGIN
    INSERT INTO investigadores(empresa,NoDocumento,TipoDocumento,CiudadExp,Nombres,Apellidos,CorreoInvest,AgenciaInvest,Aseguradora,FirmaDigital,Estado)
    VALUES(parempresa,parNoDocumento,parTipoDocumento,parCiudadExp,parNombres,parApellidos,parCorreoInvest,parAgenciaInvest,parAseguradora,parFirmaDigital,'A');
	
    SELECT 'OK' AS Msg;
    END$$

DELIMITER ;
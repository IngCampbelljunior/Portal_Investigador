DELIMITER $$

USE `bd`$$

DROP PROCEDURE IF EXISTS `Get_DatosFormatoInvestigacion`$$

CREATE DEFINER=`hgarcia`@`%` PROCEDURE `Get_DatosFormatoInvestigacion`(
  parEmpresa VARCHAR(6),
  parNoCaso  NUMERIC(10),
  parFecha DATE  
  )
BEGIN

 SELECT a.Empresa,a.Caso,
   a.Fecha_elaboracion,
   a.NoDocumento,
   a.TipoDocumento,
   a.CiudadExp,
   a.Nombres,
   a.Apellidos,
   a.Fecha_acc,
   a.Hora_acc,
   a.Diagnostico,
   a.Reporte,
   a.Doc_Investigador,
   a.Aseguradora,
   CONCAT(b.Nombres,' ',b.Apellidos) Investigador,
   c.Nombre_Entidad
  FROM investigador_reporte a
    INNER JOIN investigadores b ON a.Doc_investigador=b.NoDocumento
     INNER JOIN entidades c ON a.empresa=c.empresa AND a.Aseguradora=c.nitentidad
   WHERE a.empresa=parEmpresa AND a.Caso=parNoCaso AND a.Fecha_elaboracion=parFecha;
   
  
END$$

DELIMITER ;
using LiloSoft.DataBase.ConectaDB;
using LiloSoft.Types.Data;
using LiloSoft.Web.ProveedorWeb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LiloSoft.Utils;
using System.Reflection;
using System.Xml.Linq;

namespace LiloSoft.Siesa.Interfaz
{
    public class ConsultasEsculapioDB : BaseDB
    {

        #region Contructores
        /// <summary>
        /// Constructor por Defecto
        /// </summary>
        public ConsultasEsculapioDB()
        {
            ComportamientoIndividualComandos = true;
            TipoComandoIndividual = TipoComando.InstruccionSQL;
        }

        /// <summary>
        /// Constructo con Objeto de Acceso a Datos
        /// </summary>
        /// <param name="pSqlDb"></param>
        public ConsultasEsculapioDB(DB pSqlDb) : this()
        {
            SqlDb = pSqlDb;
        }
        #endregion

        #region Metodos GET Consultas
       
        public List<ConsultaIPEmpresas> GetIpEmpresas(string Empresa)
        {
            var sql = @"SELECT Empresa, IpConexion, Descripcion, NombreComercial as Ubicacion
 FROM Ipconexion_Empresas WHERE Empresa = :Empresa Order by IpConexion";
            var lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);
            return ExecuteGetLista<ConsultaIPEmpresas>(sql, false, lstPars.ToArray());
        }

        public List<ConsultaIPEmpresas> GetCodigoEmpresa(string IpConexion) //
        {
            var sql = @"SELECT Empresa FROM Ipconexion_Empresas
WHERE IpConexion Like CONCAT('%',:IpConexion,'%') Order by IpConexion";
            var lstPars = new List<Parametro>();
            lstPars.AddParametro("IpConexion", IpConexion);
            return ExecuteGetLista<ConsultaIPEmpresas>(sql, false, lstPars.ToArray());
        }

        public List<ConsultaUsuariosAdv> GetUsuarioReporInvestigacion(string empresa, string usuario)
        {
            var sql = @"SELECT a.Empresa,a.usuario FROM usuarios_repor_invest a Inner Join Usuarios b On a.usuario=b.usuario WHERE a.usuario=:usuario And a.empresa=:empresa AND a.estado='A'";

            var lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", empresa);
            lstPars.AddParametro("usuario", usuario);

            //sql = sqlBld.ToString();
            return ExecuteGetLista<ConsultaUsuariosAdv>(sql, false, lstPars.ToArray());
        }
               
        ////---Mi metodos
        public List<DatosPacientes> GetDatosPacientes(string Empresa, decimal criterio)
        {
            string sql = "SELECT p.nombre1, p.nombre2, p.Apellido1, p.Apellido2, p.TipoDocumento, p.NoDocumento, c.FechaIngreso, c.HoraIngreso,";
            sql += "s.Fecha_Accidente, s.Hora_Accidente, c.Desc_Diag_Ing, s.Informe_Accid, c.Tipo_Diag,";
            sql += "IFNULL(p.documento_de, 'Sin Municipio') CodCiudad, IFNULL(mun.nombre, 'Sin Ciudad') Espedicion ";
            sql += "FROM cuenta c ";
            sql += "INNER JOIN pacientes p ON c.Nohistoria = p.NoHistoria ";
            sql += "INNER JOIN admisiones a ON c.noadm_inicial=a.noadmision AND c.empresa=a.empresa ";
            sql += "INNER JOIN datos_soat s ON c.empresa = s.empresa AND c.nocuenta = s.nocuenta ";
            sql += "LEFT OUTER JOIN municipios mun ON mun.empresa = 'CM' AND p.Documento_De = mun.CodCiudad ";
            sql += "WHERE c.empresa =:Empresa AND c.Nocuenta =:criterio;";

            
            List<Parametro> lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);
            lstPars.AddParametro("criterio", criterio);
            return base.ExecuteGetLista<DatosPacientes>(sql, false, lstPars.ToArray());
        }

        public List<Entidades> GetAseguradoras(string Empresa)
        {
            string sql = "SELECT DISTINCT a.nitentidad, a.nombre_entidad " +
                "FROM entidades a " +
                "INNER JOIN convenios b ON a.empresa=b.empresa AND a.nitentidad=b.nitentidad " +
                "AND b.fec_fin_vigencia>=CURRENT_DATE() AND b.estado='A' AND b.Soat='S' " +
                "WHERE a.empresa=:Empresa AND a.estado='A' " +
                "ORDER BY a.nombre_entidad";
            List<Parametro> lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);

            return base.ExecuteGetLista<Entidades>(sql, false, lstPars.ToArray());
        }

        public List<investigador_reporte> GetDatoFormatoInvestigador(string Empresa, decimal NoCaso, Fecha fechaElab, string Usuario)
        {
            string sql = @"SELECT i.empresa, a.nombre_entidad, i.Fecha_elaboracion, CONCAT_WS(' ',i.Nombres,i.Apellidos) AS paciente, i.TipoDocumento, 
                            i.NoDocumento, i.CiudadExp, i.Caso, i.FechaIngreso, i.HoraIngreso, i.Fecha_acc, i.Hora_acc, i.Diagnostico, i.Relato, CONCAT_WS(' ', v.Nombres, v.Apellidos) AS investigadores,
                            v.NoDocumento Doc_Investigador, v.AgenciaInvest, u.FirmaDigital FirmaUsuario
                            FROM investigador_reporte i
                            INNER JOIN investigadores v ON i.empresa = v.empresa AND i.Doc_Investigador = v.NoDocumento
                            INNER JOIN entidades a ON i.empresa=a.empresa and i.Aseguradora = a.nitentidad
                            INNER JOIN usuarios_acreditador u ON i.empresa=u.empresa AND u.usuario=:Usuario
                            WHERE i.empresa =:Empresa AND i.caso =:NoCaso";

            List<Parametro> lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);
            lstPars.AddParametro("NoCaso", NoCaso);
            lstPars.AddParametro("fechaElab", fechaElab);
            lstPars.AddParametro("Usuario", Usuario);

            return base.ExecuteGetLista<investigador_reporte>(sql, false, lstPars.ToArray());
        }

        public List<DatosInvestigadores> GetInvestigadores(string Empresa)
        {
            string sql = "SELECT NoDocumento, CONCAT(i.Nombres, ' ', i.Apellidos) as investigador " +
                "FROM investigadores i WHERE i.empresa=:Empresa";
            List<Parametro> lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);

            return base.ExecuteGetLista<DatosInvestigadores>(sql, false, lstPars.ToArray());
        }
       
        public List<ConsultarReportes> GetConsultarReportes(string Empresa, Fecha fechaInicial, Fecha fechaFinal)
        {
            string sql = @"SELECT Caso, Fecha_elaboracion, Nombres, Apellidos, TipoDocumento, NoDocumento
                            FROM investigador_reporte WHERE Empresa =:Empresa AND Fecha_elaboracion between :fechaInicial AND :fechaFinal";

            List<Parametro> lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);
            lstPars.AddParametro("fechaInicial", fechaInicial);
            lstPars.AddParametro("fechaFinal", fechaFinal);

            return base.ExecuteGetLista<ConsultarReportes>(sql, false, lstPars.ToArray());
        }

        public Entero GetConnectionId()
        {
            var sql = "SELECT CONNECTION_ID() Id";
            var id = SqlDb.EjecutarEscalar<Entero>(sql, false, null);
            return id;
        }

        public List<entrega_mipres> GetReportePrescripciones(string Empresa, Fecha FechaIni, Fecha FechaFin)
        {
            var sql = @"Select a.Empresa,a.NoPrescripcion,a.Responsable,a.NoCuenta,a.NoIdentificacion, CONCAT_WS(' ',b.nombre1,b.nombre2,b.apellido1,b.apellido2) As Paciente,
                      a.CodigoCum,a.Id,a.IdEntrega,a.IdReporte,CONCAT('$', FORMAT(a.VlrEntrega, 2)) as Valor ,a.IdAnulacion,a.FechaProceso,a.Estado 
                       From entrega_mipres a Inner Join Pacientes b On a.NoIdentificacion = b.NoHistoria 
                 Where a.empresa=:Empresa and a.FechaProceso between :FechaIni and :FechaFin
            Order by a.FechaProceso, a.NoPrescripcion";
            
            var lstPars = new List<Parametro>();
            lstPars.AddParametro("Empresa", Empresa);
            lstPars.AddParametro("FechaIni", FechaIni);
            lstPars.AddParametro("FechaFin", FechaFin);
            //sql = sqlBld.ToString();
            return ExecuteGetLista<entrega_mipres>(sql, false, lstPars.ToArray());
        }


         #endregion

        #region Metodos CRUD


        #endregion

        #region Tablero de Cirugia
        /// <summary>
        /// Lista de 
        /// </summary>
        /// <param name="Empresa"></param>
        /// <returns></returns>
        //public List<DataTableroCirugia> GetTableroCirugia(string Empresa)
        //{
        //    var sql = "GetTableroCirugiaUrgencia";
        //    var lstPars = new List<Parametro>();
        //    lstPars.AddParametro("Empresa", Empresa);
        //    return ExecuteGetLista<DataTableroCirugia>(sql, true, lstPars.ToArray());
        //}

        //public List<DataTableroCasosPendientes> GetTableroCasosPendiente(string Empresa, Fecha FechaInicial, string HoraInicial, Fecha FechaFinal, string HoraFinal, string Servicio)
        //{
        //    var sql = "GetCasosPendienteAtencion";
        //    var lstPars = new List<Parametro>();
        //    lstPars.AddParametro("Empresa", Empresa);
        //    lstPars.AddParametro("FechaInicial", FechaInicial);
        //    lstPars.AddParametro("HoraInicial", HoraInicial);
        //    lstPars.AddParametro("FechaFinal", FechaFinal);
        //    lstPars.AddParametro("HoraFinal", HoraFinal);
        //    lstPars.AddParametro("Servicio", Servicio);
        //    return ExecuteGetLista<DataTableroCasosPendientes>(sql, true, lstPars.ToArray());
        //}

        #endregion

    }//fin clase

    //***********************************
    //-- Modelos
    //***********************************

    #region Clases Modelos BD

    public class DatosCasoBasico
    {
        [DisplayName("No.Caso")]
        public Entero NoCuenta { get; set; }
        [DisplayName("No.Historia")]
        public string NoHistoria { get; set; }
        [DisplayName("Nombre del Paciente")]
        public string NombrePaciente { get; set; }
        [DisplayName("Fec.Ingreso")]
        public Fecha FechaIngreso { get; set; }
        [DisplayName("Hor.Ingreso")]
        public string HoraIngreso { get; set; }
        [DisplayName("Estado Cuenta ")]
        public string Estado { get; set; }

        public Entero ConsInternoFact { get; set; }
        public string sexo { get; set; }
        [NoDataBase]
        [DisplayName("Sexo")]
        public string NombreSexo
        {
            get
            {
                var nom = "";
                switch (sexo)
                {
                    case "M":
                        nom = "Masculino";
                        break;
                    case "F":
                        nom = "Femenino";
                        break;
                }
                return nom;
            }
        }
        public Entero Edad { get; set; }
        [NoDataBase]
        [DisplayName("Edad ")]
        public string NombreEdad
        {
            get
            {
                var data = "";
                if (Edad != null)
                {
                    var medida =
                      "";
                    var edad = Edad.ValorInterno;
                    var plural = true;
                    if (edad == 1)
                        plural = false;
                    switch (Medida_Edad)
                    {
                        case "A":
                            medida = "Año" + (plural ? "s" : "");
                            break;
                        case "M":
                            medida = "Mes" + (plural ? "es" : "");
                            break;
                        case "D":
                            medida = "Día" + (plural ? "s" : "");
                            break;
                    }
                    data = $"{edad} {medida}";
                }
                return data;
            }
        }
        public string Medida_Edad { get; set; }
    }

    public class ResponsablesCaso
    {
        public Entero ConsInterno { get; set; }
        public string NitEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string CodConvenio { get; set; }
        public string Nombre_Convenio { get; set; }

    }

    public interface IEstadosTrazabilidad
    {
        string CodEstadoTrazabilidad { get; set; }
        string NombreEstadoTrazabilidad { get; set; }
    }

    public class ResponsablesCasoTrazabilidad : ResponsablesCaso, IEstadosTrazabilidad
    {
        public string CodEstadoTrazabilidad { get; set; }
        [NoDataBase]
        public string NombreEstadoTrazabilidad { get; set; }

        public EnteroLargo IdTrazabilidadActual { get; set; }

    }

    public class TrazabilidadCuentaResponsable : ResponsablesCasoTrazabilidad
    {
        public Entero NoCuenta { get; set; }
        public Hora FechaEstado { get; set; }
        public string UsuarioEstado { get; set; }
        public string Estado { get; set; }
        public Moneda ValorFactura { get; set; }
    }
    /// <summary>
    /// Historico de Trazabilidad
    /// </summary>
    /// 
    public class TrazabilidadHistorico : IEstadosTrazabilidad
    {
        [DataObjectField(true)]
        public EnteroLargo IdTrazabilidad { get; set; }
        public Hora FechaTrazabilidad { get; set; }
        public string UsuarioHistTrazabilidad { get; set; }
        public string CodEstadoTrazabilidad { get; set; }
        [NoDataBase]
        public string NombreEstadoTrazabilidad { get; set; }
        public Hora FechaEstado { get; set; }
        public string UsuarioEstado { get; set; }
        public string Estado { get; set; }
        public Moneda ValorFactura { get; set; }
    }

    public class DetalleCuenta
    {
        public Fecha fecha_cargo { get; set; }
        public string horacargo { get; set; }
        public string cod_servicio_origen { get; set; }
        public string NombreServicioOrigen { get; set; }
        public string CodDependencia { get; set; }
        public string NoOrdenServicio { get; set; }
        public string CodServicio { get; set; }
        public string NombreServicio { get; set; }
        public Moneda Cantidad { get; set; }
        public Moneda ValorServicio { get; set; }
        [NoDataBase]
        public Moneda TotalServicio => Cantidad * ValorServicio;
        public Moneda Porcentaje { get; set; }
        public string CodClase { get; set; }
        public string Usuario_Grabacion { get; set; }
        public Fecha fecha_real_cargo { get; set; }
        public string Hora_real_cargo { get; set; }
        public Moneda ValorIva { get; set; }
        public Moneda CostoServicio { get; set; }
        public string CedulaMedico { get; set; }
        public string NombreMedico { get; set; }
        public string CodEspecialidad { get; set; }
        public string NombreEspecialidad { get; set; }


    }

    public class HistoriasClinicasEmpresas
    {
        public string CiudadServer { get; set; }
        public string Empresa { get; set; }
        public string NombreEmpresa { get; set; }
        public string Acompanante { get; set; }
        public string NoHistoria { get; set; }
        public string NoFolio { get; set; }
        public string NoCuenta { get; set; }
        public string NoAdmision { get; set; }
        public string Cod_Servicio { get; set; }
        public string NombreServicio { get; set; }
        public string CodDependencia { get; set; }
        public string CedulaMedico { get; set; }
        public Fecha FechaIngreso { get; set; }
        public string HoraIngreso { get; set; }
        public string usuario { get; set; }
        public string estado { get; set; }
        public string Urgencias { get; set; }
        public string consultaExterna { get; set; }
        public string RequiereMotivoIngreso { get; set; }
        public string cod_parent { get; set; }
        public Fecha FechaEgreso { get; set; }
        public string HoraEgreso { get; set; }

    }

    public class DatosAdmision
    {
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admisión")]
        public string NoAdmision { get; set; }
        [DisplayName("Fec.Ingreso")]
        public Fecha FechaIngreso { get; set; }
        [DisplayName("Hor.Ingreso")]
        public string HoraIngreso { get; set; }
        [DisplayName("Estado")]
        public string Estado { get; set; }
        [NoDataBase]
        [DisplayName("Estado Ingreso")]
        public string NombreEstado
        {
            get
            {
                var nom = "";
                switch (Estado)
                {
                    case "I":
                        nom = "Ingreso";
                        break;
                    case "T":
                        nom = "Traslado";
                        break;
                    case "E":
                        nom = "Egreso";
                        break;
                    case "X":
                        nom = "Anulado";
                        break;
                }
                return nom;
            }
        }

        [DisplayName("Fec.Egreso")]
        public Fecha FechaEgreso { get; set; }
        [DisplayName("Hor.Egreso")]
        public string HoraEgreso { get; set; }
        [DisplayName("Servicio")]
        public string CodServicio { get; set; }
        [DisplayName("Nombre del Servicio")]
        public string NombreServicio { get; set; }
        [NoDataBase]
        [DisplayName("Servicio de Ingreso")]
        public string ServicioAdmision => $"{CodServicio}-{NombreServicio}";
        [DisplayName("Habitacion")]
        public string Habitacion { get; set; }
        [DisplayName("Pendiente Habitación")]
        public string Pendiente_Hab { get; set; }
        [DisplayName("Habitación")]
        public string NombreHabitacion => Pendiente_Hab == "S" ? "Pendiente" : Habitacion;
        [DisplayName("Cédula Médico")]
        public string CodMedico { get; set; }
        [DisplayName("Nombre del Médico")]
        public string NombreMedico { get; set; }
        [NoDataBase]
        [DisplayName("Medico del Servicio")]
        public string MedicoServicio => $"{CodMedico}-{NombreMedico}";
        [DisplayName("Reingreso")]
        [BooleanEquivalent("S", "N")]
        public bool Reingreso { get; set; }
        [DisplayName("Autorización Salida")]
        [BooleanEquivalent("S", "N")]
        public bool Salida { get; set; }

    }//fin clase

    public class DatosBasicosHistoria
    {
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admision")]
        public string NoAdmision { get; set; }
        [DisplayName("Fecha Atención")]
        public Fecha FechaAtencion { get; set; }
        [DisplayName("Hora Atención")]
        public string HoraAtencion { get; set; }
        [DisplayName("Motivo Consulta")]
        public string MotivoConsulta { get; set; }
        [DisplayName("Enfermedad Actual")]
        public string EnfermedadActual { get; set; }
        [DisplayName("Frec.Cardiaca")]
        public Moneda FrecCardiaca { get; set; }
        [DisplayName("Frec.Respiratoria")]
        public Moneda FrecRespitatoria { get; set; }
        [DisplayName("Presión")]
        public string PresionArterial { get; set; }
        [DisplayName("Temperatura")]
        public Moneda TEmperatura { get; set; }
        [DisplayName("Peso")]
        public Moneda Peso { get; set; }
        [DisplayName("Cédula del Médico")]
        public string CedulaMedico { get; set; }
        [DisplayName("Nombre del Médico")]
        public string NombreMedico { get; set; }
        [NoDataBase]
        public string MedicoHistoria => $"{CedulaMedico}-{NombreMedico}";
        [DisplayName("Diagnostico Presuntivo")]
        public string Diagnostico { get; set; }
    }//fin clase

    public class OrdenesMedicamentos
    {
        public string CiudadServer { get; set; }
        public string Empresa { get; set; }
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admision")]
        public string NoAdmision { get; set; }

        [DisplayName("No.Orden Medica")]
        public string NoOrdenMedica { get; set; }
        [DisplayName("Fecha Orden")]
        public Fecha FechaOrden { get; set; }
        [DisplayName("Hora de la Orden")]
        public string HoraOrden { get; set; }
        [DisplayName("Cédula Médico Orden")]
        public string CedulaMedico { get; set; }
        [DisplayName("Orden Salida")]
        public string ordensalida { get; set; }
        [DisplayName("Nombre del Medico")]
        public string NombreMedico { get; set; }
        [DisplayName("Medico Orden")]
        [NoDataBase]
        public string MedicoOrden => $"{CedulaMedico}-{NombreMedico}";
        [DisplayName("Usuario Grabación")]
        public string UsuarioGrabacion { get; set; }
        [DisplayName("EstadoM")]
        public string estadoM { get; set; }
        [DisplayName("Estado Orden")]
        public string estado { get; set; }
        [DisplayName("Estado Aplicación")]
        public string EstadoAplicacion { get; set; }
        [DisplayName("No. Radicado")]
        public string noRadicado { get; set; }
        [DisplayName("Tipo Orden")]
        public string TipoOrden { get; set; }
        [DisplayName("Fecha Real Cargo")]
        public Fecha FechaReal { get; set; }
    }//fin clase

    public class OrdenMedicamentoDetalle
    {
        public string NoOrdenMedica { get; set; }
        public string ConsecutivoDetalle { get; set; }
        public string CodMedicamento { get; set; }
        public string NombreMedicamento { get; set; }
        public string Frecuencia { get; set; }
        public string EstadoAplicacion { get; set; }
        public string NomEstadoAplicacion { get; set; }
    }//fin clase

    public class OrdenesParaclinicos
    {
        public string CiudadServer { get; set; }
        public string Empresa { get; set; }
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admision")]
        public string NoAdmision { get; set; }

        [DisplayName("No.Folio")]
        public string NoFolio { get; set; }
        [DisplayName("No.Orden Médica")]
        public string NoOrdenMedica { get; set; }
        [DisplayName("Fec.Orden")]
        public Fecha FechaOrden { get; set; }
        [DisplayName("Hor.Orden")]
        public string HoraOrden { get; set; }
        [DisplayName("Cédula del Médico")]
        public string CedulaMedico { get; set; }
        [DisplayName("Nombre del Médico")]
        public string NombreMedico { get; set; }
        [NoDataBase]
        [DisplayName("Médico de la Orden")]
        public string MedicoOrden => $"{CedulaMedico}-{NombreMedico}";

        [DisplayName("Usuario Grabación")]
        public string UsuarioGrabacion { get; set; }
        [DisplayName("Dependencia Orden")]
        public string CodDependencia { get; set; }
        [DisplayName("Dependencia Encabezado")]
        public string DptoEnc { get; set; }
        [DisplayName("Nombre de la Dependencia")]
        public string NomDependecia { get; set; }
        [NoDataBase]
        [DisplayName("Dependencia de la Orden")]
        public string DependenciaOrden => $"{CodDependencia} {NomDependecia}";

        [DisplayName("No. Orden Servicio")]
        public string noordenservicio { get; set; }
        [DisplayName("Esquema Tarifario")]
        public string CodEsquema { get; set; }
        [DisplayName("Usuario Grabación")]
        public string usuariograbacion { get; set; }
        [DisplayName("Valor Estado Orden")]
        public string estadoM { get; set; }
        [DisplayName("Estado Orden")]
        public string estado { get; set; }
        [DisplayName("No.Radicado")]
        public string noradicado { get; set; }
        [DisplayName("Fecha Real Orden")]
        public Fecha FechaReal { get; set; }
        [DisplayName("Justificación Orden")]
        public string justificacion { get; set; }
    }//fin clase

    public class OrdenParaClinicoDetalle
    {
        [DisplayName("No.Orden Médica")]
        public string NoOrdenMedica { get; set; }
        [DisplayName("Consecutivo Detalle")]
        public string ConsecutivoDetalle { get; set; }
        [DisplayName("Código del Servicio")]
        public string CodServicio { get; set; }
        [DisplayName("Nombre del Servicio Solicitado")]
        public string NombreServicio { get; set; }
        [DisplayName("Observaciones Generales")]
        public string Observaciones { get; set; }
    }//fin clase

    public class OrdenCuidadoPaciente
    {
        public string CiudadServer { get; set; }
        public string Empresa { get; set; }
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admision")]
        public string NoAdmision { get; set; }

        [DisplayName("No. Orden Médica")]
        public string NoOrdenMedica { get; set; }

        [DisplayName("Fecha de la Orden")]
        public Fecha FechaOrden { get; set; }

        [DisplayName("Hora de la Orden")]
        public string HoraOrden { get; set; }

        [DisplayName("Cédula del Médico")]
        public string CedulaMedico { get; set; }

        [DisplayName("Nombre del Médico")]
        public string NombreMedico { get; set; }
        [NoDataBase]
        [DisplayName("Medico de la Orden")]
        public string MedicoOrden => $"{CedulaMedico}-{NombreMedico}";

        [DisplayName("Usuario Grabación")]
        public string UsuarioGrabacion { get; set; }

        [DisplayName("Estado Orden")]
        public string Estado { get; set; }

        [DisplayName("No.Radicado")]
        public string NoRadicado { get; set; }
    }//fin clase

    public class OrdenCiuidadPacienteDetalle
    {
        public string CiudadServer { get; set; }
        public string Empresa { get; set; }
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admision")]
        public string NoAdmision { get; set; }

        [DisplayName("No. Orden Médica")]
        public string NoOrdenMedica { get; set; }

        [DisplayName("Fecha de la Orden")]
        public Fecha FechaOrden { get; set; }

        [DisplayName("Hora de la Orden")]
        public string HoraOrden { get; set; }

        [DisplayName("Cédula del Médico")]
        public string CedulaMedico { get; set; }

        [DisplayName("Nombre del Médico")]
        public string NombreMedico { get; set; }
        [NoDataBase]
        [DisplayName("Medico de la Orden")]
        public string MedicoOrden => $"{CedulaMedico}-{NombreMedico}";

        [DisplayName("Usuario Grabación")]
        public string UsuarioGrabacion { get; set; }

        [DisplayName("Estado Orden")]
        public string Estado { get; set; }

        [DisplayName("No.Radicado")]
        public string NoRadicado { get; set; }

        [DisplayName("Descripcion Cuidado")]
        public string DescripcionCuidado { get; set; }

        [DisplayName("Observacion Cuidado")]
        public string ObservacionCuidado { get; set; }
    }//fin clase

    public class EvolucionMedica
    {
        public string CiudadServer { get; set; }
        public string Empresa { get; set; }
        [DisplayName("No.Caso")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admision")]
        public string NoAdmision { get; set; }
        [DisplayName("No. de Historia")]
        public string NoHistoria { get; set; }
        [DisplayName("No. de Folio")]
        public string NoFolio { get; set; }
        [DisplayName("No. Nota Clínica")]
        public string NoNotaClinica { get; set; }
        [DisplayName("Fecha Nota")]
        public Fecha FechaNotaClinica { get; set; }
        [DisplayName("Hora Nota ")]
        public string HoraNotaClinica { get; set; }
        [DisplayName("Cédula del Médico")]
        public string CedulaMedicoNota { get; set; }
        [DisplayName("Nombre del Médico")]
        public string NombreMedico { get; set; }
        [NoDataBase]
        [DisplayName("Medico de la Orden")]
        public string MedicoNota => $"{CedulaMedicoNota}-{NombreMedico}";
        [DisplayName("Usuario Grabación")]
        public string UsuarioGrabacion { get; set; }
        [DisplayName("Usuario")]
        public string usuario { get; set; }
        [DisplayName("Resumen de la Nota (Para incluir en Epicrisis")]
        public string TituloResumenNota { get; set; }
        [DisplayName("Texto de la Nota de Evolución del Paciente")]
        public string TextoNotaClinica { get; set; }
        [DisplayName("Diagnostico Actualizado")]
        public string diagnostico { get; set; }
        [DisplayName("Estado")]
        public string estado { get; set; }
        [DisplayName("Especialidad")]
        public string Especialidad { get; set; }
        [DisplayName("Servicio")]
        public string Servicio { get; set; }
        [DisplayName("Justificación de indicaciones terapeuticas")]
        public string justificacion { get; set; }
        [DisplayName("Adicionar Epicrisis")]
        public string AdicionarEpicrisis { get; set; }
        [DisplayName("Resultados de Todos los Procedimientos Diagnósticos")]
        public string ResultadosExamenes { get; set; }
        [DisplayName("Fecha de Grabación")]
        public Fecha fechagraba { get; set; }
        [DisplayName("Hora de Grabacion")]
        public string horagraba { get; set; }
        [DisplayName("Nombre del Servicio")]
        public string nombreServicio { get; set; }
        [NoDataBase]
        [DisplayName("Servicio Nota")]
        public string ServicioNota => $"{Servicio} {nombreServicio}";
    }//fin clase

    public class InformeQuirurgico
    {
        [DisplayName("No.Historia")]
        public string NoHistoria { get; set; }
        [DisplayName("No.Folio")]
        public string NoFolio { get; set; }
        [DisplayName("No.Epicrisis")]
        public string NoEpicrisis { get; set; }
        [DisplayName("Servicio")]
        public string CodServicio { get; set; }
        [DisplayName("No.Cuenta")]
        public string NoCuenta { get; set; }
        [DisplayName("No.Admisión")]
        public string NoAdmision { get; set; }
        [DisplayName("Fecha Inicio")]
        public Fecha FechaInicio { get; set; }
        [DisplayName("Hora Inicio")]
        public string HoraInicio { get; set; }
        [DisplayName("Fecha Final")]
        public Fecha FechaFinal { get; set; }
        [DisplayName("Hora Final")]
        public string HoraFinal { get; set; }
        [DisplayName("Cédula Medico Cirujano")]
        public string CedulaMedico { get; set; }
        [DisplayName("Nombre Médico Cirujano")]
        public string NombreCedulaMedico { get; set; }
        [NoDataBase]
        [DisplayName("Cirujano")]
        public string MedicoCirujano => $"{CedulaMedico}-{NombreCedulaMedico}";

        [DisplayName("Cédula Medico Ayudante")]
        public string CedulaAyudante { get; set; }
        [DisplayName("Nombre Médico Ayudante")]
        public string NombreCedulaAyudante { get; set; }
        [NoDataBase]
        [DisplayName("Ayudante")]
        public string MedicoAyudante => $"{CedulaAyudante}-{NombreCedulaAyudante}";

        [DisplayName("Cedula del Anestesiologo")]
        public string CedulaAnestesiologo { get; set; }
        [DisplayName("Nombre Anestesiologo")]
        public string NombreCedulaAnestesiologo { get; set; }
        [NoDataBase]
        [DisplayName("Anestesiologo")]
        public string MedicoAnestesiologo => $"{CedulaAnestesiologo}-{NombreCedulaAnestesiologo}";
        [DisplayName("Diagnosticos PreQuirurgico")]
        public string DiagnosticoPrequirurgico { get; set; }
        [DisplayName("Diagnostico Pre 1")]
        public string DiagnosticoPre1 { get; set; }
        [DisplayName("Diagnostico Pre 2")]
        public string DiagnosticoPre2 { get; set; }
        [DisplayName("Diagnostico Pre 3")]
        public string DiagnosticoPre3 { get; set; }
        [DisplayName("Diagnostico Pre 4")]
        public string DiagnosticoPre4 { get; set; }
        [DisplayName("Diagnostico Pre 5")]
        public string DiagnosticoPre5 { get; set; }
        [DisplayName("Hallazgos")]
        public string Hallazgos { get; set; }
        [DisplayName("Procedimientos Realizados")]
        public string ProcedimientosRealizados { get; set; }
        [DisplayName("Diagnostico Pos 1")]
        public string DiagnosticoPos1 { get; set; }
        [DisplayName("Diagnostico Pos 2")]
        public string DiagnosticoPos2 { get; set; }
        [DisplayName("Diagnostico Pos 3")]
        public string DiagnosticoPos3 { get; set; }
        [DisplayName("Diagnostico Pos 4")]
        public string DiagnosticoPos4 { get; set; }
        [DisplayName("Diagnostico Pos 5")]
        public string DiagnosticoPos5 { get; set; }
        [DisplayName("Justificación del Procedimiento")]
        public string JustificacionProc { get; set; }
        [DisplayName("Diagnosticos PostQuirurgico:")]
        public string DiagnosticoPostQuirurgico { get; set; }
        [DisplayName("Descripción del Procedimiento")]
        public string DescripcionProc { get; set; }
        [DisplayName("Usuario que Graba")]
        public string UsuarioGraba { get; set; }
        [DisplayName("Fecha de Grabación")]
        public Fecha FechaGraba { get; set; }
        [DisplayName("Hora de Grabación")]
        public string HoraGraba { get; set; }
        [DisplayName("Usuario Actualización")]
        public string UsuarioAct { get; set; }
        [DisplayName("Fecha Actualización")]
        public Fecha FechaAct { get; set; }
        [DisplayName("Hora Actualizacón")]
        public string HoraAct { get; set; }
        [DisplayName("Consecutivo Cirugía")]
        public string ConsCir { get; set; }
        [DisplayName("Código del Convenio")]
        public string CodConvenio { get; set; }
        [DisplayName("Consecutivo Interno Factura")]
        public string ConsInternoFactura { get; set; }
        [DisplayName("Consecutivo")]
        public string Consecutivo { get; set; }
        [DisplayName("Conducta a Seguir")]
        public string Conducta { get; set; }
        [DisplayName("Ordenes Médicas")]
        public string OrdenesMedicas { get; set; }
        [DisplayName("Tiene Foto")]
        public string TieneFoto { get; set; }
        [DisplayName("Cédula Médico que Opera")]
        public string CedulaMedicoOpera { get; set; }
        [DisplayName("Nombre Medico que Opera")]
        public string NombreCedulaMedicoOpera { get; set; }
        [NoDataBase]
        [DisplayName("Medico que Opera")]
        public string MedicoOpera => $"{CedulaMedicoOpera}-{NombreCedulaMedicoOpera}";

        [DisplayName("Deja Mechas, Drenes, Comprensas, Gasas, Cuantas y Explique:")]
        public string DrenesMechas { get; set; }
        [DisplayName("Material Osteosintesis")]
        public string MaterialOsteosintesis { get; set; }
        [DisplayName("Tipo de Anestesia")]
        public string tipoanestesia { get; set; }
        [DisplayName("Osteosintesis")]
        public string Osteosintesis { get; set; }
    }//fin clase

    public class ConsultaCasos
    {
        [DisplayName("No. de Cuenta")]
        public string NoCuenta { get; set; }
        [DisplayName("No. de Historia")]
        public string NoHistoria { get; set; }
        [DisplayName("Fecha de Ingreso")]
        public Fecha FechaIngreso { get; set; }
        [DisplayName("Nombre del Paciente")]
        public string NombrePaciente { get; set; }
    }//fin clase

    public class ConsultaPacienteEventoAdverso
    {
        [DisplayName("Nombres")]
        public string NombrePaciente { get; set; }
        [DisplayName("Apellidos")]
        public string ApellidoPaciente { get; set; }
        [DisplayName("No de Identificacion")]
        public string NoIdentificacion { get; set; }
        [DisplayName("No Caso")]
        public string NoCaso { get; set; }
    }//fin clase

    public class ConsultaEventosAdversos
    {

        [DisplayName("Fecha Evento")]
        public string Fecha { get; set; }
        [DisplayName("Hora Evento")]
        public string Hora { get; set; }
        [DisplayName("Servicio")]
        public string Servicio { get; set; }
        [DisplayName("No Caso")]
        public string NoCaso { get; set; }
        [DisplayName("Nombre del Paciente")]
        public string NombrePaciente { get; set; }
        [DisplayName("Apellido del Paciente")]
        public string ApellidoPaciente { get; set; }
        [DisplayName("Sucesos del Evento")]
        public string Sucesos { get; set; }
        [DisplayName("Causas del Evento")]
        public string Causas { get; set; }

    }//fin clase

    public class ConsultaUsuariosAdv
    {

        [DisplayName("Usuario")]
        public string Usuario { get; set; }
        [DisplayName("PassWord")]
        public string PassWordUsu { get; set; }
        [DisplayName("Nombre")]
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public string Role {get;set;}
        public string Empresa { get; set; }
    }//fin clase

    public class ConsultaIPEmpresas
    {
        [DisplayName("Empresa")]
        public string Empresa { get; set; }
        [DisplayName("IpConexion")]
        public string IpConexion { get; set; }
        [DisplayName("Descripcion")]
        public string Descripcion { get; set; }
        [DisplayName("Ubicacion")]
        public string Ubicacion { get; set; }
    }//fin clase

    public class ConsultaPacientesRonda
    {
        [DisplayName("No. de Cuenta")]
        public string NoCaso { get; set; }
        [DisplayName("No. de Historia")]
        public string NoHistoria { get; set; }
        [DisplayName("No. de Admision")]
        public string NoAdmision { get; set; }
        [DisplayName("No. de Folio")]
        public string NoFolio { get; set; }
        [DisplayName("Codigo Servicio")]
        public string Cod_Servicio { get; set; }
        [DisplayName("Nombre Servicio")]
        public string NombreServicio { get; set; }
        [DisplayName("Nombres")]
        public string NombrePaciente { get; set; }
        [DisplayName("Documento")]
        public string NoDocumento { get; set; }


    }//fin clase

    public class ConsultaEspecialidades
    {
        [DisplayName("Codigo Especialidad")]
        public string Cod_Especialidad { get; set; }
        [DisplayName("Descripcion")]
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string Especialidad { get; set; }
    }

    public class ConsultaTiemposRM
    {
        [DisplayName("Id Tiempo")]
        public Int32 Id_Tiempo { get; set; }
        [DisplayName("Codigo Tiempo")]
        public string Cod_Tiempo { get; set; }
        public Int32 Duracion { get; set; }

    }

    public class ConsultaReconmendaciones
    {
        [DisplayName("Id Ronda")]
        public Int32 Id_RondaMed { get; set; }
        [DisplayName("Id Reconmendacion")]
        public string Cod_Reconmendacion { get; set; }
        [DisplayName("Descripcion")]
        public string Reconmendacion { get; set; }
        public string Tipo { get; set; }
        public Fecha Fecha_Creacion { get; set; }
        public string Estado { get; set; }
        public string Cod_Dieta { get; set; }
        public string Descripcion { get; set; }
        public string Observacion { get; set; }


    }

    public class ConsultaProcedimientoQx
    {
        [DisplayName("Id Ronda")]
        public Int32 Id_RondaMed { get; set; }
        [DisplayName("Codigo ProcedQx")]
        public string Cod_ProcedQx { get; set; }
        [DisplayName("Descripcion")]
        public string Descripcion { get; set; }
        public string Estado { get; set; }
    }

    public class ConsultaViasAplicaMed
    {
        [DisplayName("Codigo Via Aplicacion")]
        public string CodViaAplicacion { get; set; }
        [DisplayName("Nombre Via")]
        public string NombreViaAplicacion { get; set; }
        public string Estado { get; set; }
    }

    public class ConsultaUnidadMedicamentos
    {
        [DisplayName("Codigo Unidad")]
        public string Unidad { get; set; }
        [DisplayName("Nombre Unidad")]
        public string NombreUnidad { get; set; }
        public decimal EquivalenciaConsumo { get; set; }
    }

    public class MaestroArticulos
    {
        [DataObjectField(true)]
        public string CodArticulo { get; set; }
        public string NombreArticulo { get; set; }
        public string Unidad_Consumo { get; set; }
        public string Pos { get; set; }
        public string Empresa { get; set; }
        public string PrincipioActivo { get; set; }
    }

    public class Ronda_Med_ProcedimientoQx
    {
        public Int32 Id_RondaMed { get; set; }
        public string Cod_ProcedQx { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
    }

    public class historia_cuidadosenf_enc
    {
        [DisplayName("Codigo Cuidados")]
        public string CodigoCuidadoEnc { get; set; }
        public string DescripcionCuidadoEnc { get; set; }
        public string Alias { get; set; }
        public string Estado { get; set; }
    }

    public class ConsultaCuidados
    {
        [DisplayName("Codigo Cuidados")]
        public string CodigoCuidadoEnc { get; set; }
        public string DescripcionCuidadoEnc { get; set; }
        public string CodigoCuidado { get; set; }
        public string DescripcionCuidado { get; set; }
    }

    public class ConsultaDietas
    {
        [DisplayName("Codigo Cuidados")]
        public string CodigoCuidadoEnc { get; set; }
        public string DescripcionCuidadoEnc { get; set; }
        public string CodigoCuidado { get; set; }
        public string DescripcionCuidado { get; set; }
    }

    public class ConsultaMedicos
    {
        [DisplayName("Cedula Medicos")]
        public string Cedula { get; set; }
        public string Medico { get; set; }
        public string Cod_Especialidad { get; set; }
    }

    public class ConsultaRondaMed
    {
        [DisplayName("Id Ronda")]
        public Int32 Id_RondaMed { get; set; }
        public Fecha Fecha { get; set; }
        public string Hora { get; set; }
    }

    public class ConsultaKardex
    {
        public string CodPab { get; set; }
        public string Nombre { get; set; }
        public string CodHab { get; set; }
        public string Habitacion { get; set; }
        public string NoCuenta { get; set; }
        public string NombrePaciente { get; set; }
        public Fecha FechaIngreso { get; set; }
        public string NoHistoria { get; set; }
        public string Nombre_Convenio { get; set; }
        public string NoCuentaMaestra { get; set; }
        public string Edad { get; set; }
        public string CodMedico { get; set; }
        public string NombreMedico { get; set; }
        public string Cod_Especialidad { get; set; }
        public string NombreEspecialidad { get; set; }
        public string Diagnostico { get; set; }
        public string NoAdmision { get; set; }
        public string Ruta { get; set; }

    }

    public class ConsultaServicios
    {
        [DisplayName("Id Servicio")]
        public string CodServicio { get; set; }
        public string NomServicio { get; set; }
        public string CodDependencia { get; set; }
        public string NomDependecia { get; set; }
    }

    public class ConsultaDependencias
    {
        public string CodDependencia { get; set; }

        public string NomDependecia { get; set; }

    }

    public class ConsultaResponsables
    {
        public string NitEntidad { get; set; }
        public string Nombre_Entidad { get; set; }
        public decimal ConsInterno { get; set; }
        public string CodConvenio { get; set; }
        public string CodEsquemaTar { get; set; }
        public string TipoTopeSoat { get; set; }
        public string soat { get; set; }
        public string CopagoPorNivel { get; set; }
        public decimal ValorFactura { get; set; }
        public decimal Valor_Descuento { get; set; }
        public string Cod_Ent_Admin { get; set; }
        public string Nofactura { get; set; }
        public string NomEPS { get; set; }
        public decimal Valor_coopago { get; set; }
        public decimal valor_moderadora { get; set; }
    }

    public class EsquemasTarifarios
    {
        [DisplayName("Id Esquema")]
        public string CodEsquema { get; set; }

    }

    public class HistoriaClinica
    {
        public decimal ConseOrdenesMedicas { get; set; }
        public string DiagnosticoIngreso { get; set; }
        //        Empresa varchar(4)
        //NoHistoria varchar(15)
        //NoFolio decimal (10,0)
        //NoCuenta decimal (10,0)
        //NoAdmision decimal (10,0)
        //Cod_Servicio char (2)

    }

    public class conse_hist_sol_cirugia
    {
        public decimal NoBoleta { get; set; }

    }

    public class ConsultaHabitacion
    {
        public string CodPab { get; set; }
        public string Nombre { get; set; }
        public string CodHab { get; set; }
        public string NomHab { get; set; }

    }

    public class ConsultaEmpresa
    {
        public string Empresa { get; set; }
        public string Nombre_Empresa { get; set; }
        public string EmpresaInv { get; set; }


    }

    public class CasoSIRAS
    {
        public string NoCaso { get; set; }
        public string NoHistoria { get; set; }
        public string Paciente { get; set; }
        public string RadicadoEnvio { get; set; }
        public string RadicadoAnulado { get; set; }
        public string Reporte { get; set; }
        public string Observacion { get; set; }
        public string DatosEnviados { get; set; }
        public Fecha Fecha { get; set; }
        public string NoCuenta { get; set; }
        public string Estado { get; set; }
        public Fecha FechaIngreso { get; set; }
        public string HoraIngreso { get; set; }

    }

    public class PacientesMIPRES
    {
        public string Empresa { get; set; }
        public string NoCuenta { get; set; }
        public string Paciente { get; set; }
        public string Identificacion { get; set; }
        public Fecha FechaIngreso { get; set; }
        public string HoraIngreso { get; set; }
        public string FechaEgreso { get; set; }
        public string HoraEgreso { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
    }

    public class tipos_gestion_siras
    {
        public string CodigoGestion { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
    }

    public class Causas_Siras
    {
        public string CodCausa { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
    }

    public class parametros_siras
    {
        public string key { get; set; }
        public string token { get; set; }
        public string Url { get; set; }
        public string Maneja_Ws { get; set; }
        public string UrlAnulacion { get; set; }
        public string HoraAutomatico { get; set; }

    }

    public class parametros_mipres
    {
        public string Nit { get; set; }
        public string token { get; set; }
        public string Maneja_Ws { get; set; }
        public string Empresa { get; set; }
        public string UrlPrescripciones { get; set; }
        public string UrlUnaPrescripcion { get; set; }
        public string UrlValidaToken { get; set; }
        public string UrlEntregaAmbito { get; set; }
        public string UrlReporteEntrega { get; set; }
        public string TokenGenerado { get; set; }
        public string tokenproveedor { get; set; }
        public string UrlEntregasFechas { get; set; }
        public string UrlAnularNoEntrega { get; set; }
        public string UrlAnularReporteEntrega { get; set; }
        public string UrlEntregaPrescripcion { get; set; }
        public string UrlReportePrescripcion { get; set; }
        public string UrlReportesFechas { get; set; }
        public string UrlFacturacion { get; set; }


    }

    public class planilla_det_siras
    {
        public string Empresa { get; set; }
        public decimal NoCuenta { get; set; }
        public Fecha FechaGestion { get; set; }
        public string HoraGestion { get; set; }
        public string TipoGestion { get; set; }
        public string CodCausa { get; set; }
        public string Comentario { get; set; }
        public string RutaArchivo { get; set; }
        public string Estado { get; set; }
        public string UsuarioGraba { get; set; }
    }

    public class datosaccidente
    {
        //public string empresa { get; set; }
        public string NoCuenta { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string FechaNacimiento { get; set; }
        public string TipoIngreso { get; set; }
        public string Fecha_Ingreso { get; set; }
        public string Hora_Ingreso { get; set; }
        public string EsVictimaRemitida { get; set; }
        public string RazonSocialRemitente { get; set; }
        public string CodigoHabilitacionRemitente { get; set; }
        public string CodigoDepartamentoRemitente { get; set; }
        public string CodigoMunicipioRemitente { get; set; }
        public string TrasladoTransporteEspecial { get; set; }
        public string CodigoPrestador { get; set; }
        public string PlacaTransporte { get; set; }
        public string Fecha_Accidente { get; set; }
        public string Hora_Accidente { get; set; }
        public string Direccion { get; set; }
        public string CodigoDepartamento { get; set; }
        public string CodigoMunicipio { get; set; }
        public string EsVehiculoIdentificado { get; set; }
        public string Placa { get; set; }
        public string PrimerApellido_Conductor { get; set; }
        public string SegundoApellido_Conductor { get; set; }
        public string PrimerNombre_Conductor { get; set; }
        public string SegundoNombre_Conductor { get; set; }
        public string TipoIdentificacion_Conductor { get; set; }
        public string Identificacion_Conductor { get; set; }
        public string NoDocumento_Cond { get; set; }
        public string Direccion_Conductor { get; set; }
        public string Telefono_Conductor { get; set; }
        public string CodigoDepartamento_Conductor { get; set; }
        public string CodigoMunicipio_Conductor { get; set; }
        public string TipoIdentificacion_Reporta { get; set; }
        public string Identificacion_Reporta { get; set; }
        public string CodigoPrestador_Reporta { get; set; }

    }

    public class requeridos_modulos
    {
        public string CampoReq { get; set; }
        public string Alfanumerico { get; set; }
        public string Panel { get; set; }

    }

    public class DataTableroCasosPendientes
    {
        public string FechaHoraIng { get; set; }
        public string NoCaso { get; set; }
        public string codServicio { get; set; }
        public string paciente { get; set; }
        public string Plan { get; set; }
        public string Responsable { get; set; }
        public string Diagnostico { get; set; }
        public string fechaingreso { get; set; }
        public string horaingreso { get; set; }

    }

    public class EgresosCasos
    {

        public string empresa { get; set; }
        public string NoCuenta { get; set; }
        public string NoAdmision { get; set; }
        public Fecha fechaIngreso { get; set; }
        public string horaingreso { get; set; }
        public string Servicio { get; set; }
        public string estado { get; set; }
        public Fecha fechaegreso { get; set; }
        public string horaEgreso { get; set; }
        public string estadoIng { get; set; }
        public string EstAdm { get; set; }
        public string servTras { get; set; }
        public string no_Autorizacion { get; set; }
        public string autorizado_Por { get; set; }
    }

    public class ExtractoCasos
    {
        public string empresa { get; set; }
        public string NoCuenta { get; set; }
        public string NoAdmision { get; set; }
        public Fecha fechaIngreso { get; set; }
        public string horaingreso { get; set; }
        public string Servicio { get; set; }
        public string estado { get; set; }
        public Fecha fechaegreso { get; set; }
        public string horaEgreso { get; set; }
        public string estadoIng { get; set; }
        public string EstAdm { get; set; }
        public string servTras { get; set; }
        public string no_Autorizacion { get; set; }
        public string autorizado_Por { get; set; }
    }

    public class DetalleExtractoMIPRES
    {
        public string NoCuenta { get; set; }
        public string fecha_cargo { get; set; }
        public decimal ConsInternofactura { get; set; }
        public string CodigoArt { get; set; }
        public string NombreArticulo { get; set; }
        public decimal cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal total { get; set; }
        public string CumArt { get; set; }
        public string CodigoCUM { get; set; }
        public string CodigoCUM_Alterno { get; set; }
    }

    public class entrega_mipres
    {
        public string Empresa { get; set; }
        public string NoPrescripcion { get; set; }
        public string NoCuenta { get; set; }
        public string NoIdentificacion { get; set; }
        public string Responsable { get; set; }
        public string CodigoCum { get; set; }
        public decimal Id { get; set; }
        public decimal IdEntrega { get; set; }
        public decimal IdReporte { get; set; }
        public decimal VlrEntrega { get; set; }
        public decimal IdAnulacion { get; set; }
        public Fecha FechaProceso { get; set; }
        public string Estado { get; set; }
        public string Paciente { get; set; }
        public string Valor { get; set; }
        public string NitEntidad { get; set; }
        public string NoHistoria { get; set; }
        public string CodEPS { get; set; }
        public decimal CantEnt { get; set; }
        public decimal PrecioUni { get; set; }
    }

    //---------- Mis modelos

    public class DatosPacientes {
        
        public string nombre1 { get; set; }
        public string nombre2 { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string TipoDocumento { get; set; }
        public string NoDocumento { get; set; }
        public string Espedicion { get; set; }
        public string FechaIngreso { get; set; }
        public string HoraIngreso { get; set; }
        public string Fecha_Accidente { get; set; }
        public string Hora_Accidente { get; set; }
        public string Desc_Diag_Ing { get; set; }
        public string Informe_Accid { get; set; }
        public string Tipo_Diag { get; set; }

    }

    public class investigador_reporte
    {
        public string Usuario { get; set; }
        public Fecha Fecha_elaboracion { get; set; }
        public string Aseguradora { get; set; }
        public string Empresa { get; set; }
        public string Caso { get; set; }
        public string NoDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public string CiudadExp { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public Fecha FechaIngreso { get; set; }
        public string HoraIngreso { get; set; }
        public Fecha Fecha_acc { get; set; }
        public string Hora_acc { get; set; }
        public string Diagnostico { get; set; }
        public string Relato { get; set; }
        public string Doc_Investigador { get; set; }
        public byte[] FirmaUsuario { get; set; }
        public byte[] FirmaFun { get; set; }
        public string AgenciaInvest { get; set; }
        public string nombre_entidad { get; set; }
        public string paciente { get; set; }
        public string investigadores { get; set; }

    }
    public class DatosInvestigadores
    {

        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string TipoDocumento { get; set; }
        public string NoDocumento { get; set; }
        public string CiudadExp { get; set; }
        public string CorreoInvest { get; set; }
        public string AgenciaInvest { get; set; }
        public string Aseguradora { get; set; }
        public string empresa { get; set; }
        public string investigador { get; set; }

    }
    
    public class ConsultarReportes
    {
        public string Caso { get; set; }
        public string Fecha_elaboracion { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string TipoDocumento { get; set; }
        public string NoDocumento { get; set; }
    }
    public class DatosAseguradoras
    {

        public string nitentidad { get; set; }
        public string nombre_entidad { get; set; }

    }
    public class SampleViewModel
    {
                
        public string Question { get; set; }

        //See here for list of answers
        public string Answer { get; set; }
    }
    #endregion

}



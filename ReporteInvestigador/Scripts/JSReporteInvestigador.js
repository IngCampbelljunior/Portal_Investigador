$(document).ready(function () {

    LovInvestigador();
    //evento del botón consultar del formulario ConsultarPaciente
    $('#btnBuscar').click(function (e) {

        var criterio = $("#caso").val();

        if (criterio.length == 0) {
            alert("Digitar criterio de búsqueda.");
            return;
        }

        GetDatosPacientes(criterio);

    });

    //eventos de la ventana modal para captura de firma
    
    $('#btnFirma').click(function (e) {
        event.preventDefault(e);

        $("#modalSign").modal("show");

    });

    //Limpia el canvas
    $('#btnClear').click(function (e) {

        signaturePad.clear();

    });

    //Elimina el contenido del canvas al cerrar el canvas
    $('#btnClose').click(function (e) {

        signaturePad.clear();

    });

    //Confirma que el input "aseguradora" tenga un valor para abrir el modal de la firma
    
    $("#aseguradora").change(function () {

        var selectedTxt = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        $("#hfaseguradora").val(selectedValue);
        $("#btnFirma").attr("disabled", false);
    });

    $("#agencia").change(function () {

        var selectedTxt = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        $("#hfagencia").val(selectedValue);

    });

    $("#btnBuscarInf").click(function () {

        var parFechaI = $("#FechaIni").val();
        var parFechaF = $("#FechaFin").val();

        GetConsultarReportes(parFechaI, parFechaF);

    });

});

function LovInvestigador() {
    $.post('LovInvestigador', function (data) {

        var msg = data.d;
        console.log(data);

        var msg = data.d;
        console.log(data);

        if (msg !== "") {
            var res = JSON.parse(data);
            $.each(res, function (i, medical) {
                $("#investigador").append('<option value="' + medical["NoDocumento"] + '">' + medical["investigador"] + '</option>');
            });
        }
        else {
            alert("Datos en Blanco");
        }
    });

}

function SeleccionInvestigador() {

    var x = document.getElementById("investigador").value;
    var z = $("#investigador option:selected").text();
    var selectedTxt = z;
    var selectedValue = x;
    //alert("Seleccionado: " + selectedValue);
    $("#hfinvestigador").val(selectedValue);
}

function SeleccionAseguradora() {

    var x = document.getElementById("aseguradora").value;
    var z = $("#aseguradora option:selected").text();
    var selectedTxt = z;
    var selectedValue = x;
    //alert("Seleccionado: " + selectedValue);
    $("#hfaseguradora").val(selectedValue);
}

function SaveImagenTemp(raw_image_data, Tipo) {
    var Uridata = "SaveImagenTemp";
    var data = { datauri: raw_image_data }

    $.ajax({
        url: Uridata,
        type: 'POST',
        dataType: 'json',
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        error: function (xhr, status, error) {
            alert("ERROR: " + status + " " + error + " " + xhr.status + " " + xhr.statusText)
        },
        success: function (result) {
            imgBase64 = result;
            swfImg = true;

            $("#modalSign").modal("hide");

            //alert('Image Saved');
        }
    });
}

var canvas = document.querySelector("canvas");

var signaturePad = new SignaturePad(canvas);

function GetDatosPacientes(criterio) {

    var uriruta = "GetDatosPaciente";
    
    $.ajax({
        type: "POST", //GET
        url: uriruta,
        content: "application/json; charset=utf-8",
        dataType: "json",
        // De la siguiente manera indicamos que del div tome los input.
        data: { "criterio": criterio},
        
        success: function (data) {
            //alert(data);
            
            var msg = data.d;
            console.log(data);
            var res = JSON.parse(data);

            $(res).each(function () { 
                var nombrePaciente = this.nombre1.toString() + " " + this.nombre2.toString();
                var apellidoPaciente = this.Apellido1 + " " + this.Apellido2;
                $("#nombre").val(nombrePaciente);  //llena los campos en el form
                $("#apellido").val(apellidoPaciente);
                $("#tipodoc").val(this.TipoDocumento);
                $("#numdoc").val(this.NoDocumento);
                $("#expedido").val(this.Espedicion);
                $("#fechIngreso").val(this.FechaIngreso);
                $("#horaIngreso").val(this.HoraIngreso);
                $("#fechAcci").val(this.Fecha_Accidente);
                $("#horaAcci").val(this.Hora_Accidente);
                $("#dX").val(this.Desc_Diag_Ing);
                $("#relato").val(this.Informe_Accid);
            });

        },
        error: function (xhr, status, error) {
            alert("ERROR: " + status + " " + error + " " + xhr.status + " " + xhr.statusText)
        }
    });

}

//envia a la base de datos investigador_reporte la infomacion de los input de la vista Consultapaciente
function GuardarReporte() {

    var aseguradora = $("#hfaseguradora").val();
    var investigador = $("#hfinvestigador").val();
    var caso = $("#caso").val();
    var nombre = $("#nombre").val();
    var apellido = $("#apellido").val();
    var tipodoc = $("#tipodoc").val();
    var numdoc = $("#numdoc").val();
    var expedido = $("#expedido").val();
    var fechIng = $("#fechIngreso").val();
    var horaIng = $("#horaIngreso").val();
    var fechAcci = $("#fechAcci").val();
    var horaAcci = $("#horaAcci").val();
    var dx = $("#dX").val();
    var relato = $("#relato").val();

    if (aseguradora == null || aseguradora == "undefined") {
        alert("Debe seleccionar la Aseguradora");
        return;
    }

    if (investigador == "-- Seleccione --" || investigador == "undefined") {
        alert("Debe seleccionar un Investigador");
        return;
    }

    if (dx.length < 7 || dx == null) {
        alert("Digite un diagnostico del paciente");
        return;
    }
    //Empresa,Caso,Fecha_elaboracion,NoDocumento,TipoDocumento,CiudadExp,Nombres,Apellidos,FechaIngreso,HoraIngreso,Fecha_acc,Hora_acc,Diagnostico,Relato,Doc_Investigador,Aseguradora
    var parametros = { nocaso: caso, nodocumento: numdoc, tipodocumento: tipodoc, ciudadexp: expedido, nombres: nombre, apellidos: apellido, fechaing: fechIng, horaing: horaIng, fechaacc: fechAcci, horaacci: horaAcci, diagnos: dx, relato: relato, investigador: investigador, aseguradora:aseguradora}

    $.post("RegistrarReporte", { nocaso: caso, nodocumento: numdoc, tipodocumento: tipodoc, ciudadexp: expedido, nombres: nombre, apellidos: apellido, fechaing: fechIng, horaing: horaIng, fechaacc: fechAcci, horaacci: horaAcci, diagnos: dx, relato: relato, investigador: investigador, aseguradora: aseguradora }, function (result) {
        
        var msg = result;
        console.log(result);
        
    });
    ViewPDF();
}

function ViewPDF() {
    var Uridata = "ViewFormato";
    var NoCaso = $("#caso").val();
    $.post("ViewFormato", { NoCuenta: NoCaso }, function (result) {
        
        imgBase64 = result;
        
        let pdfWindow = window.open("")
        pdfWindow.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(imgBase64) + "'></iframe>")

    });

}

function GetConsultarReportes(pFechaI, pFechaF) {

    var UriRuta = "GetConsultarReportes";

    $.ajax({
        type: "POST", //GET
        url: UriRuta,
        content: "application/json; charset=utf-8",
        dataType: "json",
        // De la siguiente manera indicamos que del div tome los input.
        data: { FechaIni: pFechaI, FechaFin: pFechaF }, //JSON.stringify(parametros),
        success: function (data) {

            var msg = data.d;
            console.log(data);

            if (msg !== "") {


                var LstCampos = "Fecha_elaboracion(T)(L)(N)(G)(N)[5][15],";
                LstCampos += "Caso(T)(L)(N)(G)(N)[5][15],";
                LstCampos += "TipoDocumento(T)(L)(N)(G)(N)[10][25],";
                LstCampos += "NoDocumento(T)(L)(N)(G)(N)[5][15],";
                LstCampos += "Nombres(T)(L)(N)(G)(N)[10][25],";
                LstCampos += "Apellidos(T)(L)(N)(G)(N)[10][25],";
                DesingTabla('gvPrescripcion', LstCampos, data, true, false, false, false);


            }
            else {
                alert("Datos en Blanco");
            }
        },
        error: function (xhr, status, error) {
            alert("ERROR: " + status + " " + error + " " + xhr.status + " " + xhr.statusText)
        }
    });

}

//Función para el botón Guardar de la vista RegistrarInvestigador (line 1195)
function GuardarInvestigador() {

    var agencia = $("#hfagencia").val();
    var aseguradora = $("#hfaseguradora").val();
    var nombre = $("#nombre").val();
    var apellido = $("#apellido").val();
    var tipodoc = $("#tipodoc").val();
    var numdoc = $("#numdoc").val();
    var city = $("#city").val();
    var email = $("#email").val();
   
    if (aseguradora == "-- Seleccione --" || aseguradora == null || aseguradora == "") {
        alert("Debe seleccionar la Aseguradora");
        return;
    }

    if (agencia == "-- Seleccione --" || agencia == null || agencia == "" ) {
        alert("Debe seleccionar una Agencia Investigadora");
        return;
    }

    //empresa,NoDocumento,TipoDocumento,CiudadExp,Nombres,Apellidos,CorreoInvest,AgenciaInvest,Aseguradora,FirmaDigital,Estado
    var parametros = { nodocumento: numdoc, tipodocumento: tipodoc, ciudadexp: city, nombres: nombre, apellidos: apellido, correoinvest: email, agenciainvest: agencia, aseguradora: aseguradora}

    $.post("RegistrarInvestigador", { nodocumento: numdoc, tipodocumento: tipodoc, ciudadexp: city, nombres: nombre, apellidos: apellido, correoinvest: email, agenciainvest: agencia, aseguradora: aseguradora}, function (result) {

        var msg = result;
        console.log(result);


        if (msg == "OK") {
            alert("Datos en Blanco");

        }
        else {
            alert("Investigador Registrado");
        }

    });
}

function dt_gvPrescripcion_RowClick(ObjetoTR, tit) {

    var NoCaso = ObjetoTR.cells[1].childNodes[0].nodeValue;

    var urlFile = "https://" + window.location.host + "/Documentos/";
    
    var FilePDF = urlFile + NoCaso + '.pdf';
    //let pdfWindow = window.open("")
    hidden = open(FilePDF, 'NewWindow', 'top=0,left=0,width=800,height=600,status=no,resizable=no,scrollbars=yes');

}
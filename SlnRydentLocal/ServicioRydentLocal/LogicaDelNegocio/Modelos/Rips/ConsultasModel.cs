using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class ConsultasModel
    {
        public string? codPrestador { get; set; }
        public string? fechaInicioAtencion { get; set; }
        public string? codConsulta { get; set; }
        public string? modalidadGrupoServicioTecSal { get; set; }
        public string? grupoServicios { get; set; }
        public int? codServicio { get; set; }
        public string? finalidadTecnologiaSalud { get; set; }
        public string? causaMotivoAtencion { get; set; }
        public string? codDiagnosticoPrincipal { get; set; }
        public string? codDiagnosticoRelacionado1 { get; set; }
        public string? codDiagnosticoRelacionado2 { get; set; }
        public string? codDiagnosticoRelacionado3 { get; set; }
        public string? tipoDiagnosticoPrincipal { get; set; }
        public string? tipoDocumentoIdentificacion { get; set; }
        public string? numDocumentoIdentificacion { get; set; }
        public long? vrServicio { get; set; }
        public string? conceptoRecaudo { get; set; }
        public long? valorPagoModerador { get; set; }
        public string? numFEVPagoModerador { get; set; }
        public int consecutivo { get; set; }
    }
}

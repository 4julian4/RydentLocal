using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
    public class ProcedimientosModel
    {
        public string codPrestador { get; set; }
        public string fechaInicioAtencion { get; set; }
        public string? idMIPRES { get; set; }
        public string? numAutorizacion { get; set; }
        public string? codProcedimiento { get; set; }
        public string? viaIngresoServicioSalud { get; set; }
        public string? modalidadGrupoServicioTecSal { get; set; }
        public string? grupoServicios { get; set; }
        public int? codServicio { get; set; }
        public string? finalidadTecnologiaSalud { get; set; }
        public string? tipoDocumentoIdentificacion { get; set; }
        public string? numDocumentoIdentificacion { get; set; }
        public string? codDiagnosticoPrincipal { get; set; }
        public string? codDiagnosticoRelacionado { get; set; }
        public string? codComplicacion { get; set; }
        public long? vrServicio { get; set; }
        public string? conceptoRecaudo { get; set; }
        public long? valorPagoModerador { get; set; }
        public string? numFEVPagoModerador { get; set; }
        public int consecutivo { get; set; }
    }
}

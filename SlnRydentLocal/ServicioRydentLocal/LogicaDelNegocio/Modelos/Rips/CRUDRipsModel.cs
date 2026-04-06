using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
	
    public class ConsultarRipsRequest
    {
        public int? IDANAMNESIS { get; set; }
        public DateTime? FECHAINI { get; set; }
        public DateTime? FECHAFIN { get; set; }
    }

    public class EliminarRipsRequest
    {
        public int? IDANAMNESIS { get; set; }
        public string? FACTURA { get; set; }
        public DateTime? FECHA { get; set; }
        public TimeSpan? HORA { get; set; }
    }

    public class ConsultarRipsDetalleRequest
    {
        public int? IDANAMNESIS { get; set; }
        public string? FACTURA { get; set; }
        public DateTime? FECHA { get; set; }
        public TimeSpan? HORA { get; set; }
    }

    public class RipsListadoItem
    {
        public int? IDANAMNESIS { get; set; }
        public string? FACTURA { get; set; }
        public DateTime? FECHA { get; set; }
        public TimeSpan? HORA { get; set; }
        public bool TIENECONSULTA { get; set; }
        public int CANTIDADPROCEDIMIENTOS { get; set; }
        public string? ENTIDAD { get; set; }
        public string? DESCRIPCION { get; set; }
    }

    public class RipsDetalleResponse
    {
        public int? IDANAMNESIS { get; set; }
        public int? IDDOCTOR { get; set; }

        public string? FACTURA { get; set; }
        public DateTime? FECHACONSULTA { get; set; }
        public TimeSpan? HORA { get; set; }

        public string? CODIGOENTIDAD { get; set; }
        public string? NOMBREENTIDAD { get; set; }
        public string? NUMEROAUTORIZACION { get; set; }

        public string? EXTRANJERO { get; set; }
        public string? PAIS { get; set; }

        public string? CODIGOCONSULTA { get; set; }
        public string? FINALIDADCONSULTA { get; set; }
        public string? CAUSAEXTERNA { get; set; }

        public string? CODIGODIAGNOSTICOPRINCIPAL { get; set; }
        public string? CODIGODIAGNOSTICO2 { get; set; }
        public string? CODIGODIAGNOSTICO3 { get; set; }
        public string? CODIGODIAGNOSTICO4 { get; set; }

        public string? TIPODIAGNOSTICO { get; set; }

        public double? VALORCONSULTA { get; set; }
        public double? VALORCUOTAMODERADORA { get; set; }
        public double? VALORNETO { get; set; }

        public List<RipsProcedimientoItem>? PROCEDIMIENTOS { get; set; }
    }
}


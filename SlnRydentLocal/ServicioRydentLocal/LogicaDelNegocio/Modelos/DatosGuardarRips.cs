using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class RipsProcedimientoItem
	{
		public string? CODIGOPROCEDIMIENTO { get; set; }
		public string? NOMBREPROCEDIMIENTO { get; set; }

		public string? DXPRINCIPAL { get; set; }
		public string? DXRELACIONADO { get; set; }

		public string? AMBITOREALIZACION { get; set; }
		public string? FINALIDADPROCEDIMIENTI { get; set; }
		public string? PERSONALQUEATIENDE { get; set; }

		public double? VALORPROCEDIMIENTO { get; set; }

		public string? COMPLICACION { get; set; }
		public string? FORMAREALIZACIONACTOQUIR { get; set; }
	}

	public class DatosGuardarRips
	{
		public int? IDANAMNESIS { get; set; }
		public int? IDDOCTOR { get; set; }

		public string? FACTURA { get; set; }
		public DateTime? FECHACONSULTA { get; set; }

		public string? CODIGOENTIDAD { get; set; }
		public string? NOMBREENTIDAD { get; set; }

		public string? NUMEROAUTORIZACION { get; set; }

		public string? EXTRANJERO { get; set; }
		public string? PAIS { get; set; }

		public string? CODIGOCONSULTA { get; set; }
		public string? NOMBRECONSULTA { get; set; }

		public string? FINALIDADCONSULTA { get; set; }
		public string? CAUSAEXTERNA { get; set; }

		public string? CODIGODIAGNOSTICOPRINCIPAL { get; set; }
		public string? CODIGODIAGNOSTICO2 { get; set; }
		public string? CODIGODIAGNOSTICO3 { get; set; }
		public string? CODIGODIAGNOSTICO4 { get; set; }

		public string? NOMBREDIAGNOSTICOPRINCIPAL { get; set; }
		public string? NOMBREDIAGNOSTICO2 { get; set; }
		public string? NOMBREDIAGNOSTICO3 { get; set; }
		public string? NOMBREDIAGNOSTICO4 { get; set; }

		public string? TIPODIAGNOSTICO { get; set; }

		public double? VALORCONSULTA { get; set; }
		public double? VALORCUOTAMODERADORA { get; set; }
		public double? VALORNETO { get; set; }

		public string? CODIGOPROCEDIMIENTO { get; set; }
		public string? FINALIDADPROCEDIMIENTI { get; set; }
		public string? AMBITOREALIZACION { get; set; }
		public string? PERSONALQUEATIENDE { get; set; }
		public string? DXPRINCIPAL { get; set; }
		public string? DXRELACIONADO { get; set; }
		public string? COMPLICACION { get; set; }
		public string? FORMAREALIZACIONACTOQUIR { get; set; }
		public double? VALORPROCEDIMIENTO { get; set; }

		public List<RipsProcedimientoItem>? PROCEDIMIENTOS { get; set; }

		public string? MODO { get; set; } // CREAR | EDITAR
		public string? FACTURAORIGINAL { get; set; }
		public bool? REEMPLAZAR_EXISTENTE { get; set; }

		// NUEVO: para identificar lote cuando factura sea PENDIENTE
		public TimeSpan? HORALOTE { get; set; }
		public DateTime? FECHAORIGINAL { get; set; }
		public TimeSpan? HORAORIGINAL { get; set; }
	}
}
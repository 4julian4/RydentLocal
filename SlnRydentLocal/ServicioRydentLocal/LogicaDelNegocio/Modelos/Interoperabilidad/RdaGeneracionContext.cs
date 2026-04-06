using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaGeneracionContext
	{
		public int IdAnamnesis { get; set; }
		public int? IdDoctor { get; set; }
		public DateTime? FechaConsulta { get; set; }
		public TimeSpan? HoraConsulta { get; set; }
		public string? Factura { get; set; }
		public string? CodigoConsulta { get; set; }
		public string? CodigoDiagnosticoPrincipal { get; set; }
		public string? CodigoProcedimiento { get; set; }
		public string? NumeroAutorizacion { get; set; }
	}
}

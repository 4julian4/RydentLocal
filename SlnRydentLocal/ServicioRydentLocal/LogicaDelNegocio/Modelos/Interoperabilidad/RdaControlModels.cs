using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaControlFiltro
	{
		public DateTime? FechaDesde { get; set; }
		public DateTime? FechaHasta { get; set; }
		public string? Estado { get; set; }
		public string? Texto { get; set; }
		public int MaxRegistros { get; set; } = 100;
	}

	public sealed class RdaControlItem
	{
		public int ID { get; set; }
		public int IDANAMNESIS { get; set; }
		public int? IDEVOLUCION { get; set; }
		public DateTime? FECHA_ATENCION { get; set; }
		public string? TIPO_DOCUMENTO { get; set; }
		public string? ESTADO { get; set; }
		public DateTime? FECHA_GENERACION { get; set; }
		public DateTime? FECHA_ENVIO { get; set; }
		public int? INTENTOS { get; set; }
		public int? CODIGO_HTTP { get; set; }
		public string? MENSAJE_ERROR { get; set; }

		public string? NOMBRE_PACIENTE { get; set; }
		public string? DOCUMENTO_PACIENTE { get; set; }
		public string? NUMERO_HISTORIA { get; set; }
		public string? DOCTOR { get; set; }
		public string? FACTURA { get; set; }
	}

	public sealed class RdaControlRespuesta
	{
		public List<RdaControlItem> Items { get; set; } = new List<RdaControlItem>();
	}
}
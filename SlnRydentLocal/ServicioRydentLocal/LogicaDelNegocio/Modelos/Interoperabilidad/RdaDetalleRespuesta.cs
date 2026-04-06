using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaDetalleRespuesta
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

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

		public string? JSON_RDA { get; set; }
		public string? JSON_SNAPSHOT { get; set; }
		public string? REQUEST_API { get; set; }
		public string? RESPUESTA_API { get; set; }
	}
}
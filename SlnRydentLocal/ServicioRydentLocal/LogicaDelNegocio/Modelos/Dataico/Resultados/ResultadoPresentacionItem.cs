using Newtonsoft.Json;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Resultados
{
	public class ResultadoPresentacionItem
	{
		// Campos “legacy” que ya estabas usando internamente:
		[JsonProperty("idRelacion")]
		public int idRelacion { get; set; }

		[JsonProperty("factura")]
		public string? factura { get; set; }

		[JsonProperty("codigoPrestador")]
		public string codigoPrestador { get; set; } = "";

		[JsonProperty("ok")]
		public bool ok { get; set; }

		[JsonProperty("mensaje")]
		public string? mensaje { get; set; }

		[JsonProperty("externalId")]
		public string? externalId { get; set; } // id/uuid devuelto por la API

		/// <summary>
		/// Tipo de factura (mismo sentido que en el payload):
		/// 1 = normal; !=1 = CxC.
		/// </summary>
		[JsonProperty("tipoFactura")]
		public int? tipoFactura { get; set; }

		/// <summary>
		/// Operación de salud aplicada a esta factura: SS_SIN_APORTE, SS_RECAUDO, etc.
		/// </summary>
		[JsonProperty("operation")]
		public string? operation { get; set; }

		// ===== Aliases para que el front no tenga que adivinar =====
		// (Se serializan además de los campos legacy)
		[JsonProperty("tenantCode")]
		public string tenantCode => codigoPrestador;

		[JsonProperty("documentRef")]
		public int documentRef => idRelacion;

		[JsonProperty("numeroFactura")]
		public string? numeroFactura => factura;
	}
}

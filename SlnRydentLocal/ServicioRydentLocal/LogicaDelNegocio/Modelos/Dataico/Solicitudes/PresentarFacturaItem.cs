using Newtonsoft.Json;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Solicitudes
{
	/// <summary>
	/// Ítem individual que el FRONT envía al worker para presentar UNA factura.
	/// </summary>
	public class PresentarFacturaItem
	{
		[JsonProperty("idRelacion")]
		public int idRelacion { get; set; }               // clave para armar el body (SP)

		[JsonProperty("codigoPrestador")]
		public string codigoPrestador { get; set; } = ""; 
		
		[JsonProperty("codigoPrestadorPPAL")]
		public string codigoPrestadorPPAL { get; set; } = ""; // header X-Tenant-Code

		// Acepta "factura" y también "numeroFactura" desde el front:
		[JsonProperty("factura")]
		public string? factura { get; set; }              // opcional (traza)

		// Alias de entrada: si llega "numeroFactura", lo copia a "factura"
		[JsonProperty("numeroFactura")]
		public string? numeroFactura { set { factura = value; } }

		/// <summary>
		/// Tipo de factura:
		///   1  = factura "normal" (T_ADICIONALES_ABONOS)
		///   !=1 = cuenta por cobrar (T_CUENTASXCOBRAR)
		/// 
		/// En el worker, si viene null, asumiremos 1 por defecto.
		/// </summary>
		[JsonProperty("tipoFactura")]
		public int? tipoFactura { get; set; }

		/// <summary>
		/// Campo genérico "operation".
		/// Antes lo usabas para cosas como "FES_REGISTRAR_EN_DIAN".
		/// 
		/// Para el flujo de salud podemos usarlo también si queremos:
		/// ej. "SS_SIN_APORTE", "SS_RECAUDO", etc.
		/// 
		/// El worker:
		/// - Primero mira payload.Operation
		/// - Si es nulo/vacío, puede usar este campo del primer item.
		/// </summary>
		[JsonProperty("operation")]
		public string? operation { get; set; }
	}
}

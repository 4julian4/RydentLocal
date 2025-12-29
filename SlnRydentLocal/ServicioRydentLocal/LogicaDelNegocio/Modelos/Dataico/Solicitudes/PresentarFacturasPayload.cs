using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Solicitudes
{
	/// <summary>
	/// Payload que envía el FRONT al worker para presentar facturas en DIAN.
	/// 
	/// AHORA:
	/// - Puede venir "operation" a nivel de lote (ej: "SS_SIN_APORTE", "SS_RECAUDO", etc.).
	/// - Sigue trayendo la lista de items como antes.
	/// 
	/// NOTA: Para no romper nada:
	/// - Si Operation viene nulo/vacío, el worker puede mirar item.operation del primer item.
	/// </summary>
	public class PresentarFacturasPayload
	{
		/// <summary>
		/// Operación de salud a nivel de lote:
		/// "SS_SIN_APORTE", "SS_RECAUDO", "SS_CUFE", "SS_REPORTE".
		/// Opcional: si no viene, el worker usará el operation del primer item.
		/// </summary>
		[JsonProperty("operation")]
		public string? Operation { get; set; }

		/// <summary>
		/// Lista de facturas a presentar.
		/// </summary>
		[JsonProperty("items")]
		public List<PresentarFacturaItem> items { get; set; } = new();
	}
}

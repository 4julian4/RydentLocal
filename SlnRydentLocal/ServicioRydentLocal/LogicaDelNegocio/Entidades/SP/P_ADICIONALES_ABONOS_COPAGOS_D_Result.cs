using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
	/// <summary>
	/// Representa una fila devuelta por el SP P_ADICIONALES_ABONOS_COPAGOS_D.
	/// Este SP devuelve los copagos asociados a una factura:
	///  - VALOR          -> Monto del copago
	///  - FECHA          -> Fecha en que se hizo el copago
	///  - CONCEPTOCOPAGO -> Texto que indica el concepto del copago
	/// 
	/// IMPORTANTE:
	/// - Esta clase se usa SOLO para leer (no se mapea a una tabla física).
	/// - Por eso se marca como [Keyless].
	/// </summary>
	[Keyless] // Indicamos a EF Core que NO tiene clave primaria
	public class P_ADICIONALES_ABONOS_COPAGOS_D_Result
	{
		/// <summary>
		/// Valor del copago (monto en dinero).
		/// </summary>
		public decimal VALOR { get; set; }

		/// <summary>
		/// Fecha en la que se registró ese copago.
		/// </summary>
		public DateTime FECHA { get; set; }

		/// <summary>
		/// Concepto o tipo de copago (ej: COPAGO, CUOTA_MODERADORA).
		/// </summary>
		public string CONCEPTOCOPAGO { get; set; } = string.Empty;
	}
}

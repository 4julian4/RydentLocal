using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaHistorialItem
	{
		public DateTime? Fecha { get; set; }
		public string? Hora { get; set; }
		public string? Usuario { get; set; }
		public string? Descripcion { get; set; }
		public string? Tipo { get; set; }
	}

	public sealed class RdaHistorialRespuesta
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }
		public List<RdaHistorialItem> Items { get; set; } = new List<RdaHistorialItem>();
	}
}
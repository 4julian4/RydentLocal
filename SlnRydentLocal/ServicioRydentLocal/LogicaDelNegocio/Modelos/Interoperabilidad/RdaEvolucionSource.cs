using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaEvolucionSource
	{
		public int IdEvolucion { get; set; }
		public int IdAnamnesis { get; set; }
		public DateTime? Fecha { get; set; }
		public TimeSpan? Hora { get; set; }
		public string? Doctor { get; set; }
		public string? Nota { get; set; }
		public string? Evolucion { get; set; }
		public string? Complicacion { get; set; }
		public DateTime? FechaProximaCita { get; set; }
		public string? ProximaCitaTexto { get; set; }
		public string? Urgencias { get; set; }
	}
}
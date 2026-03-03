using System.Collections.Concurrent;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Whatsap;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public static class WhatsAppQueue
	{
		public static readonly ConcurrentQueue<WaJob> High = new();   // Agendar / Reagendar
		public static readonly ConcurrentQueue<WaJob> Normal = new(); // Recordar masivo
	}
}

using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Solicitudes
{
	public class PresentarFacturasPayload
	{
		public List<PresentarFacturaItem> items { get; set; } = new();
	}
}

using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Resultados
{
	public class ResumenPresentacionLote
	{
		public int total { get; set; }
		public int ok { get; set; }
		public int fail { get; set; }
		public List<ResultadoPresentacionItem> resultados { get; set; } = new();
	}
}

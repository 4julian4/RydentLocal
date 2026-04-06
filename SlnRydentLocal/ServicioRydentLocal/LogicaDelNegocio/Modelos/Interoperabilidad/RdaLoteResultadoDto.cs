using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public class RdaLoteResultadoDto
	{
		public bool Ok { get; set; }
		public string Accion { get; set; } = "";
		public int Total { get; set; }
		public int Procesadas { get; set; }
		public int Exitosas { get; set; }
		public int Fallidas { get; set; }
		public string Mensaje { get; set; } = "";
		public List<RdaProcesoResultado> Resultados { get; set; } = new List<RdaProcesoResultado>();
	}
}
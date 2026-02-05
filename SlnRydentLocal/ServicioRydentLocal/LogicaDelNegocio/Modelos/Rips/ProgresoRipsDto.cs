using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rips
{
	public class ProgresoRipsDto
	{
		public string Accion { get; set; } = "PRESENTAR"; // o "GENERAR"
		public int Total { get; set; }
		public int Procesadas { get; set; }
		public int Exitosas { get; set; }
		public int Fallidas { get; set; }
		public string? UltimoDocumento { get; set; } // factura o nota
		public string? Mensaje { get; set; }
	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	internal class GenerarRdaDesdeRipsRequest
	{
		public int IDANAMNESIS { get; set; }
		public string FACTURA { get; set; } = string.Empty;
		public DateTime? FECHA { get; set; }
		public string HORA { get; set; } = string.Empty;
	}
}





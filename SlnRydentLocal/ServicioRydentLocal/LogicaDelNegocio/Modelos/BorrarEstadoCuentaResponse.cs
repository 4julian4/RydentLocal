using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public class BorrarEstadoCuentaResponse
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }

		// Para replicar el “debe borrarlos primero”
		public bool TieneAbonosOAdicionales { get; set; }
	}
}

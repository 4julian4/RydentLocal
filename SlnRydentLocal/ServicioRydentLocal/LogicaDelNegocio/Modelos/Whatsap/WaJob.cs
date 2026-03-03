using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Whatsap
{
	public class WaJob
	{
		public string Numero { get; set; } = "";
		public string Mensaje { get; set; } = "";
		public DateTime? Fecha { get; set; } // opcional para revalidar
		public int? Silla { get; set; }
		public TimeSpan? Hora { get; set; }
		public string Tipo { get; set; } = ""; // "AGENDA" o "RECORDAR"
	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
	public class EntResolucion_Dian
	{
		public int id { set; get; }
		public string? descripcion { set; get; }
		public DateTime fecha { set; get; }
		public string resolucion { set; get; }
		public string numeracion { set; get; }
		public int rangoIni { set; get; }
		public int rangoFin { set; get; }
		public string prefijo { set; get; }
		public int tamanno { set; get; }
		public int tipo { set; get; }
		public int id_Reemplaza { set; get; }
		public string resolucionExt { get { return resolucion + " (" + id.ToString() + ")"; } }
	}
}

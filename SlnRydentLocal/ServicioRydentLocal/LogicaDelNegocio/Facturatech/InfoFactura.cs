using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Facturatech
{
	public class InfoFactura
	{
		public string Numero { set; get; } = "";
		public string Prefijo { set; get; } = "";
		public MemoryStream PDFStream { set; get; }
		public NegMensaje Mensaje { set; get; } = new NegMensaje();
	}
}

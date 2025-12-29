using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Entidades.SP
{
	using Microsoft.EntityFrameworkCore;

	[Keyless]
	public class P_MAX_CON_LETRAS_Result
	{
		public string? FACTURA { get; set; }
		public int NUMERO { get; set; }
		public int IDRESOLUCION { get; set; }
	}
}

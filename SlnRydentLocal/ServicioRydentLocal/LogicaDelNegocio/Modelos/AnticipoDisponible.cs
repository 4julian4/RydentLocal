using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos
{
	public sealed class AnticipoDisponible
	{
		public int IdRelacionAbono { get; set; }     // aa.IDRELACION (abono / anticipo origen)
		public double Acumulado { get; set; }        // saldo disponible (aa.VALOR - sum(ap.VALOR))
	}

	public sealed class AnticipoDisponibleDto
	{
		public int IdRelacionAbono { get; set; }   // aa.IDRELACION
		public double Acumulado { get; set; }     // aa.VALOR - sum(ap.VALOR)
		public int Identificador { get; set; }    // aa.IDENTIFICADOR (para ordenar)
	}

	public sealed class RelacionAnticiposResult
	{
		public bool Relaciono { get; set; }
		public double Restante { get; set; }
	}
}

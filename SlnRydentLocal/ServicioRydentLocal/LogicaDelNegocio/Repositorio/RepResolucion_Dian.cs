using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Facturatech;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Repositorio
{
	public class RepResolucion_Dian
	{

		public List<EntResolucion_Dian> ListarTodas()
		{
			using (var db = new AppDbContext())
			{
				List<EntResolucion_Dian> _LstResolucion_Dian = new List<EntResolucion_Dian>();
				var LSTTRESOLUCION_DIAN = db.TRESOLUCION_DIAN.Where(x => !string.IsNullOrEmpty(x.RESOLUCION) && !string.IsNullOrEmpty(x.PREFIJO) && (x.TAMANNO != null)).ToList();
				if (LSTTRESOLUCION_DIAN.Count() > 0)
				{
					foreach (var resolucion in LSTTRESOLUCION_DIAN)
					{
						_LstResolucion_Dian.Add(Automap.AutoMapearDesdeObjeto<EntResolucion_Dian>(resolucion));
					}
				}
				return _LstResolucion_Dian.OrderBy(x => x.id).ToList();
			}
		}
	}
}

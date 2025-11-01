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
	public class RepAdicionales_Abonos
	{
		public List<EntAdicionales_Abonos> listar(int idAnamnesis)
		{
			List<EntAdicionales_Abonos> resultado = new List<EntAdicionales_Abonos>();
			using (var db = new AppDbContext())
			{
				var _T_ADICIONALES_ABONOS = db.T_ADICIONALES_ABONOS.Where(x => x.ID == idAnamnesis).ToList();
				foreach (var ADICIONALES_ABONOS in _T_ADICIONALES_ABONOS)
				{
					var s = Automap.AutoMapearDesdeObjeto<EntAdicionales_Abonos>(ADICIONALES_ABONOS);
					resultado.Add(s);
				}
			}
			return resultado;
		}


		public EntAdicionales_Abonos Consultar(int idRelacion)
		{
			List<EntAdicionales_Abonos> resultado = new List<EntAdicionales_Abonos>();
			using (var db = new AppDbContext())
			{
				var ADICIONALES_ABONOS = db.T_ADICIONALES_ABONOS.Where(x => x.IDRELACION == idRelacion).FirstOrDefault();
				if (ADICIONALES_ABONOS != null && ADICIONALES_ABONOS.ID > 0)
				{
					var retornar = Automap.AutoMapearDesdeObjeto<EntAdicionales_Abonos>(ADICIONALES_ABONOS);
					return retornar;
				}
				else
				{
					return new EntAdicionales_Abonos();
				}
			}
		}


		public bool Editar(EntAdicionales_Abonos Adicionales_Abonos)
		{
			List<EntAdicionales_Abonos> resultado = new List<EntAdicionales_Abonos>();
			using (var db = new AppDbContext())
			{
				var ADICIONALES_ABONOS = db.T_ADICIONALES_ABONOS.Where(x => x.IDRELACION == Adicionales_Abonos.idRelacion).FirstOrDefault();
				if (ADICIONALES_ABONOS != null && ADICIONALES_ABONOS.ID > 0)
				{
					ADICIONALES_ABONOS = Automap.AutoMapearDesdeObjeto<T_ADICIONALES_ABONOS>(Adicionales_Abonos);
					db.SaveChanges();
					return true;
				}
				else
				{
					return false;
				}
			}
		}


		public bool EditarTransaccionId(int idRelacion, string transaccionId)
		{
			List<EntAdicionales_Abonos> resultado = new List<EntAdicionales_Abonos>();
			using (var db = new AppDbContext())
			{
				var ADICIONALES_ABONOS = db.T_ADICIONALES_ABONOS.Where(x => x.IDRELACION == idRelacion).FirstOrDefault();
				if (ADICIONALES_ABONOS != null && ADICIONALES_ABONOS.ID > 0)
				{
					ADICIONALES_ABONOS.TRANSACCIONID = transaccionId;
					db.SaveChanges();
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		public bool EditarCXCTransaccionId(int id, string transaccionId)
		{
			List<EntAdicionales_Abonos> resultado = new List<EntAdicionales_Abonos>();
			using (var db = new AppDbContext())
			{
				var T_CUENTASXCOBRAR = db.T_CUENTASXCOBRAR.Where(x => x.ID == id).FirstOrDefault();
				if (T_CUENTASXCOBRAR != null && T_CUENTASXCOBRAR.ID > 0)
				{
					T_CUENTASXCOBRAR.TRANSACCIONID = transaccionId;
					db.SaveChanges();
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}

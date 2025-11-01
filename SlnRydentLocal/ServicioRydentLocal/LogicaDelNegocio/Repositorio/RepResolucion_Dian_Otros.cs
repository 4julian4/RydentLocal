using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Repositorio
{
	public class RepResolucion_Dian_Otros
	{
		public EntResolucion_Dian_Otros Obtener(int idResolucion_Dian)
		{
			using (var db = new AppDbContext())
			{
				var _Resolucion_Dian_Otros = db.TRESOLUCION_DIAN_OTROS.Where(x => x.IDRESOLUCION_DIAN == idResolucion_Dian).FirstOrDefault();
				if (_Resolucion_Dian_Otros != null && _Resolucion_Dian_Otros.ID > 0)
				{
					var retornar = Automap.AutoMapearDesdeObjeto<EntResolucion_Dian_Otros>(_Resolucion_Dian_Otros);
					return retornar;
				}
				else
				{
					return new EntResolucion_Dian_Otros();
				}
			}
		}


		public List<EntResolucion_Dian_Otros> lstResolucion_Dian_Otros()
		{
			using (var db = new AppDbContext())
			{
				List<EntResolucion_Dian_Otros> retornar = new List<EntResolucion_Dian_Otros>();
				var lstTRESOLUCION_DIAN_OTROS = db.TRESOLUCION_DIAN_OTROS.ToList();
				if (lstTRESOLUCION_DIAN_OTROS.Count() > 0)
				{
					foreach (var _TRESOLUCION_DIAN_OTROS in lstTRESOLUCION_DIAN_OTROS)
					{
						if (_TRESOLUCION_DIAN_OTROS != null && _TRESOLUCION_DIAN_OTROS.ID > 0)
						{
							retornar.Add(Automap.AutoMapearDesdeObjeto<EntResolucion_Dian_Otros>(_TRESOLUCION_DIAN_OTROS));

						}
					}
				}
				return retornar;


			}
		}



		public int Crear(EntResolucion_Dian_Otros _EntResolucion_Dian_Otros)
		{

			int id = 0;
			using (var db = new AppDbContext())
			{
				var _TRESOLUCION_DIAN_OTROS = new TRESOLUCION_DIAN_OTROS();
				_TRESOLUCION_DIAN_OTROS = Automap.AutoMapearDesdeObjeto<TRESOLUCION_DIAN_OTROS>(_EntResolucion_Dian_Otros);
				db.TRESOLUCION_DIAN_OTROS.Add(_TRESOLUCION_DIAN_OTROS);
				db.SaveChanges();
				id = _TRESOLUCION_DIAN_OTROS.ID;
				return id;
			}
		}

		public bool Editar(EntResolucion_Dian_Otros _EntResolucion_Dian_Otros)
		{
			using (var db = new AppDbContext())
			{
				var _TRESOLUCION_DIAN_OTROS = db.TRESOLUCION_DIAN_OTROS.Where(x => x.ID == _EntResolucion_Dian_Otros.id).FirstOrDefault();
				if (_TRESOLUCION_DIAN_OTROS != null && _TRESOLUCION_DIAN_OTROS.ID > 0)
				{
					Automap.AutoMapearObjeto<TRESOLUCION_DIAN_OTROS>(_EntResolucion_Dian_Otros, _TRESOLUCION_DIAN_OTROS, true, nameof(_EntResolucion_Dian_Otros.id));
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

using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	internal class T_CONVENIOSServicios: IT_CONVENIOSServicios
	{
		private readonly AppDbContext _dbcontext;
		public T_CONVENIOSServicios()
		{
		}
		public async Task<T_CONVENIOS> ConsultarPorId(int ID)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.T_CONVENIOS.FirstOrDefaultAsync(x => x.ID == ID);
				return obj == null ? new T_CONVENIOS() : obj;
			}
		}

		public async Task<List<T_CONVENIOS>> ConsultarTodos()
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.T_CONVENIOS.ToListAsync();
				return obj == null ? new List<T_CONVENIOS>() : obj;
			}
		}
	}
	public interface IT_CONVENIOSServicios
	{
		Task<T_CONVENIOS> ConsultarPorId(int ID);

		Task<List<T_CONVENIOS>> ConsultarTodos();
	}
}

using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TDIAGNOSTICOServicios : ITDIAGNOSTICOServicios
	{
		public TDIAGNOSTICOServicios()
		{
		}

		public async Task<int> Agregar(TDIAGNOSTICO tdiagnostico)
		{
			using (var _dbcontext = new AppDbContext())
			{
				try
				{
					_dbcontext.TDIAGNOSTICO.Add(tdiagnostico);
					await _dbcontext.SaveChangesAsync();
					return tdiagnostico.ID;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return 0;
				}
			}
		}

		public async Task<bool> Editar(int id, TDIAGNOSTICO tdiagnostico)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TDIAGNOSTICO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj == null)
					return false;

				_dbcontext.Entry(obj).CurrentValues.SetValues(tdiagnostico);
				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}

		public async Task<TDIAGNOSTICO> ConsultarPorId(int id)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TDIAGNOSTICO
					.AsNoTracking()
					.FirstOrDefaultAsync(x => x.ID == id);

				return obj ?? new TDIAGNOSTICO();
			}
		}

		public async Task<TDIAGNOSTICO> ConsultarUltimoPorAnamnesis(int idDiagnos)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TDIAGNOSTICO
					.AsNoTracking()
					.Where(x => x.IDDIAGNOS == idDiagnos)
					.OrderByDescending(x => x.FECHA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();

				return obj ?? new TDIAGNOSTICO();
			}
		}

		public async Task<List<TDIAGNOSTICO>> ConsultarPorAnamnesis(int idDiagnos)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var lista = await _dbcontext.TDIAGNOSTICO
					.AsNoTracking()
					.Where(x => x.IDDIAGNOS == idDiagnos)
					.OrderByDescending(x => x.FECHA)
					.ThenByDescending(x => x.ID)
					.ToListAsync();

				return lista ?? new List<TDIAGNOSTICO>();
			}
		}

		public async Task Borrar(int id)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TDIAGNOSTICO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj != null)
				{
					_dbcontext.TDIAGNOSTICO.Remove(obj);
					await _dbcontext.SaveChangesAsync();
				}
			}
		}
	}

	public interface ITDIAGNOSTICOServicios
	{
		Task<int> Agregar(TDIAGNOSTICO tdiagnostico);
		Task<bool> Editar(int id, TDIAGNOSTICO tdiagnostico);
		Task<TDIAGNOSTICO> ConsultarPorId(int id);
		Task<TDIAGNOSTICO> ConsultarUltimoPorAnamnesis(int idDiagnos);
		Task<List<TDIAGNOSTICO>> ConsultarPorAnamnesis(int idDiagnos);
		Task Borrar(int id);
	}
}
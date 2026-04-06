using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_RIPS_DXServicios : IT_RIPS_DXServicios
    {
        private readonly AppDbContext _dbcontext;
        public T_RIPS_DXServicios()
        {
            
        }

       

        public async Task<bool> Agregar(T_RIPS_DX t_rips_dx)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.T_RIPS_DX.Add(t_rips_dx);
                await _dbcontext.SaveChangesAsync();
                return true;
            }   
        }


        
        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_DX.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj != null)
                {
                    _dbcontext.T_RIPS_DX.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }  
        }

        
        public async Task<T_RIPS_DX> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_DX.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_RIPS_DX() : obj;
            }   
        }




        public async Task<bool> Editar(int ID, T_RIPS_DX t_rips_dx)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_DX.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_rips_dx);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }  
        }

		public async Task<T_RIPS_DX?> ConsultarUltimoPorAnamnesis(int idAnamnesis)
		{
			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_DX
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis)
					.OrderByDescending(x => x.FECHACONSULTA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<T_RIPS_DX?> ConsultarExactoAsync(int idAnamnesis, DateTime fecha, TimeSpan? hora, string? factura = null)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var query = _dbcontext.T_RIPS_DX
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis && x.FECHACONSULTA.HasValue && x.FECHACONSULTA.Value.Date == fecha.Date);

				if (hora.HasValue)
					query = query.Where(x => x.HORA.HasValue && x.HORA.Value == hora.Value);

				if (!string.IsNullOrWhiteSpace(factura))
					query = query.Where(x => x.FACTURA == factura);

				return await query
					.OrderByDescending(x => x.FECHACONSULTA)
					.ThenByDescending(x => x.HORA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<T_RIPS_DX?> ConsultarPorAnamnesisYFechaAsync(int idAnamnesis, DateTime fecha)
		{
			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_DX
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis && x.FECHACONSULTA.HasValue && x.FECHACONSULTA.Value.Date == fecha.Date)
					.OrderByDescending(x => x.HORA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<T_RIPS_DX?> ConsultarPorAnamnesisFechaYFacturaAsync(int idAnamnesis, DateTime fecha, string factura)
		{
			if (string.IsNullOrWhiteSpace(factura))
				return null;

			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_DX
					.AsNoTracking()
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FECHACONSULTA.HasValue &&
						x.FECHACONSULTA.Value.Date == fecha.Date &&
						x.FACTURA == factura)
					.OrderByDescending(x => x.HORA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}


	}

    public interface IT_RIPS_DXServicios
    {
        Task<bool> Agregar(T_RIPS_DX t_rips_dx);
        Task<bool> Editar(int ID, T_RIPS_DX t_rips_dx);
        Task<T_RIPS_DX> ConsultarPorId(int ID);
        Task<T_RIPS_DX> ConsultarUltimoPorAnamnesis(int idAnamnesis);
        Task Borrar(int ID);
        Task<T_RIPS_DX?> ConsultarExactoAsync(int idAnamnesis, DateTime fecha, TimeSpan? hora, string? factura = null);
        Task<T_RIPS_DX?> ConsultarPorAnamnesisYFechaAsync(int idAnamnesis, DateTime fecha);
        Task<T_RIPS_DX?> ConsultarPorAnamnesisFechaYFacturaAsync(int idAnamnesis, DateTime fecha, string factura);

	}
}
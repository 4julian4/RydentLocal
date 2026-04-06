using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_RIPS_PROCEDIMIENTOSServicios : IT_RIPS_PROCEDIMIENTOSServicios
    {
        private readonly AppDbContext _dbcontext;
        public T_RIPS_PROCEDIMIENTOSServicios()
        {
            
        }
        

        public async Task<bool> Agregar(T_RIPS_PROCEDIMIENTOS t_rips_procedimientos)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.T_RIPS_PROCEDIMIENTOS.Add(t_rips_procedimientos);
                await _dbcontext.SaveChangesAsync();
                return true;
            } 
        }



        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_PROCEDIMIENTOS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj != null)
                {
                    _dbcontext.T_RIPS_PROCEDIMIENTOS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }   
        }


        public async Task<T_RIPS_PROCEDIMIENTOS> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_PROCEDIMIENTOS.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_RIPS_PROCEDIMIENTOS() : obj;
            }   
        }




        public async Task<bool> Editar(int ID, T_RIPS_PROCEDIMIENTOS t_rips_procedimientos)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_PROCEDIMIENTOS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_rips_procedimientos);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }

		public async Task<T_RIPS_PROCEDIMIENTOS?> ConsultarUltimoPorAnamnesis(int idAnamnesis)
		{
			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis)
					.OrderByDescending(x => x.FECHAPROCEDIMIENTO)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<T_RIPS_PROCEDIMIENTOS?> ConsultarUltimoPorAnamnesisYFactura(int idAnamnesis, string factura)
		{
			if (string.IsNullOrWhiteSpace(factura))
				return null;

			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis && x.FACTURA == factura)
					.OrderByDescending(x => x.FECHAPROCEDIMIENTO)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}
			

		public async Task<T_RIPS_PROCEDIMIENTOS?> ConsultarExactoAsync(int idAnamnesis, DateTime fecha, TimeSpan? hora, string? factura = null)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var query = _dbcontext.T_RIPS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis && x.FECHAPROCEDIMIENTO.HasValue && x.FECHAPROCEDIMIENTO.Value.Date == fecha.Date);

				if (hora.HasValue)
					query = query.Where(x => x.HORA.HasValue && x.HORA.Value == hora.Value);

				if (!string.IsNullOrWhiteSpace(factura))
					query = query.Where(x => x.FACTURA == factura);

				return await query
					.OrderByDescending(x => x.FECHAPROCEDIMIENTO)
					.ThenByDescending(x => x.HORA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<T_RIPS_PROCEDIMIENTOS?> ConsultarPorAnamnesisYFechaAsync(int idAnamnesis, DateTime fecha)
		{
			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis && x.FECHAPROCEDIMIENTO.HasValue && x.FECHAPROCEDIMIENTO.Value.Date == fecha.Date)
					.OrderByDescending(x => x.HORA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}

		public async Task<T_RIPS_PROCEDIMIENTOS?> ConsultarPorAnamnesisFechaYFacturaAsync(int idAnamnesis, DateTime fecha, string factura)
		{
			if (string.IsNullOrWhiteSpace(factura))
				return null;

			using (var _dbcontext = new AppDbContext())
			{
				return await _dbcontext.T_RIPS_PROCEDIMIENTOS
					.AsNoTracking()
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.FECHAPROCEDIMIENTO.HasValue &&
						x.FECHAPROCEDIMIENTO.Value.Date == fecha.Date &&
						x.FACTURA == factura)
					.OrderByDescending(x => x.HORA)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();
			}
		}


	}

    public interface IT_RIPS_PROCEDIMIENTOSServicios
    {
        Task<bool> Agregar(T_RIPS_PROCEDIMIENTOS t_rips_procedimientos);
        Task<bool> Editar(int ID, T_RIPS_PROCEDIMIENTOS t_rips_procedimientos);
        Task<T_RIPS_PROCEDIMIENTOS> ConsultarPorId(int ID);
		Task<T_RIPS_PROCEDIMIENTOS?> ConsultarUltimoPorAnamnesis(int idAnamnesis);
		Task<T_RIPS_PROCEDIMIENTOS?> ConsultarUltimoPorAnamnesisYFactura(int idAnamnesis, string factura);
		Task<T_RIPS_PROCEDIMIENTOS?> ConsultarExactoAsync(int idAnamnesis, DateTime fecha, TimeSpan? hora, string? factura = null);
		Task<T_RIPS_PROCEDIMIENTOS?> ConsultarPorAnamnesisYFechaAsync(int idAnamnesis, DateTime fecha);
		Task<T_RIPS_PROCEDIMIENTOS?> ConsultarPorAnamnesisFechaYFacturaAsync(int idAnamnesis, DateTime fecha, string factura);
		Task Borrar(int ID);
    }
}
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    
    public class TTRATAMIENTOServicios : ITTRATAMIENTOServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TTRATAMIENTOServicios()
        {
        }

        public async Task<TTRATAMIENTO> Agregar(TTRATAMIENTO ttratamiento)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TTRATAMIENTO.Add(ttratamiento);
                await _dbcontext.SaveChangesAsync();
                return ttratamiento;
            }
        }

        public async Task Borrar(int IDTRATAMIENTO, DateTime FECHA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TTRATAMIENTO.FirstOrDefaultAsync(x => x.IDTRATAMIENTO == IDTRATAMIENTO && x.FECHA == FECHA);
                _dbcontext.TTRATAMIENTO.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TTRATAMIENTO> ConsultarPorId(int IDTRATAMIENTO, DateTime FECHA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TTRATAMIENTO.FirstOrDefaultAsync(x => x.IDTRATAMIENTO == IDTRATAMIENTO && x.FECHA == FECHA);
                return obj == null ? new TTRATAMIENTO() : obj;
            }
        }




        public async Task<bool> Editar(int IDTRATAMIENTO, DateTime FECHA, TTRATAMIENTO ttratamiento)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TTRATAMIENTO.FirstOrDefaultAsync(x => x.IDTRATAMIENTO == IDTRATAMIENTO && x.FECHA == FECHA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(ttratamiento);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITTRATAMIENTOServicios
    {
        Task<TTRATAMIENTO> Agregar(TTRATAMIENTO ttratamiento);
        Task<bool> Editar(int IDTRATAMIENTO, DateTime FECHA, TTRATAMIENTO ttratamiento);
        Task<TTRATAMIENTO> ConsultarPorId(int IDTRATAMIENTO, DateTime FECHA);
        Task Borrar(int IDTRATAMIENTO, DateTime FECHA);
    }
}

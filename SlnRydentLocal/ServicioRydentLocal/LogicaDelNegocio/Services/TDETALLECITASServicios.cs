using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TDETALLECITASServicios : ITDETALLECITASServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TDETALLECITASServicios()
        {
        }

        public async Task<TDETALLECITAS> Agregar(TDETALLECITAS tdetallecitas)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TDETALLECITAS.Add(tdetallecitas);
                await _dbcontext.SaveChangesAsync();
                return tdetallecitas;
            }
        }

        public async Task Borrar(DateTime FECHA, int SILLA)
        {
            if (FECHA != null && SILLA != null)
            {
                using (var _dbcontext = new AppDbContext())
                {
                    var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.FECHA == FECHA && x.SILLA == SILLA);
                    _dbcontext.TDETALLECITAS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        public async Task<TDETALLECITAS> ConsultarPorId(DateTime FECHA, int SILLA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.FECHA == FECHA && x.SILLA == SILLA);
                return obj == null ? new TDETALLECITAS() : obj;
            }
        }




        public async Task<bool> Editar(DateTime FECHA, int SILLA, TDETALLECITAS tdetallecitas)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.FECHA == FECHA && x.SILLA == SILLA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tdetallecitas);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITDETALLECITASServicios
    {
        Task<TDETALLECITAS> Agregar(TDETALLECITAS tdetallecitas);
        Task<bool> Editar(DateTime FECHA, int SILLA, TDETALLECITAS tdetallecitas);
        Task<TDETALLECITAS> ConsultarPorId(DateTime FECHA, int SILLA);
        Task Borrar(DateTime FECHA, int SILLA);
    }
}

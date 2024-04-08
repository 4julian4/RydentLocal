using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCITASServicios : ITCITASServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCITASServicios()
        {
        }

        public async Task<TCITAS> Agregar(TCITAS tcitas)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TCITAS.Add(tcitas);
                await _dbcontext.SaveChangesAsync();
                return tcitas;
            }
        }

        public async Task Borrar(int SILLA, DateTime FECHA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCITAS.FirstOrDefaultAsync(x => x.SILLA == SILLA && x.FECHA == FECHA);
                if (obj != null)
                {
                    _dbcontext.TCITAS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        public async Task<TCITAS> ConsultarPorId(int SILLA, DateTime FECHA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCITAS.FirstOrDefaultAsync(x => x.SILLA == SILLA && x.FECHA == FECHA.Date);
                return obj == null ? new TCITAS() : obj;
            }
        }




        public async Task<bool> Editar(int SILLA, DateTime FECHA, TCITAS tcitas)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCITAS.FirstOrDefaultAsync(x => x.SILLA == SILLA && x.FECHA == FECHA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tcitas);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITCITASServicios
    {
        Task<TCITAS> Agregar(TCITAS tcitas);
        Task<bool> Editar(int SILLA, DateTime FECHA, TCITAS tcitas);
        Task<TCITAS> ConsultarPorId(int SILLA, DateTime FECHA);
        Task Borrar(int SILLA, DateTime FECHA);
    }
}

using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TRESOLUCION_DIANServicios : ITRESOLUCION_DIANServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TRESOLUCION_DIANServicios()
        {
        }

        public async Task<int> Agregar(TRESOLUCION_DIAN tresolucion_dian)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TRESOLUCION_DIAN.Add(tresolucion_dian);
                await _dbcontext.SaveChangesAsync();
                return tresolucion_dian.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TRESOLUCION_DIAN.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TRESOLUCION_DIAN.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TRESOLUCION_DIAN> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TRESOLUCION_DIAN.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TRESOLUCION_DIAN() : obj;
            }
        }




        public async Task<bool> Editar(int ID, TRESOLUCION_DIAN tresolucion_dian)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TRESOLUCION_DIAN.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tresolucion_dian);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITRESOLUCION_DIANServicios
    {
        Task<int> Agregar(TRESOLUCION_DIAN tresolucion_dian);
        Task<bool> Editar(int ID, TRESOLUCION_DIAN tresolucion_dian);
        Task<TRESOLUCION_DIAN> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

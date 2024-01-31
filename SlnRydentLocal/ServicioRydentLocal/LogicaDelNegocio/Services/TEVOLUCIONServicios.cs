using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TEVOLUCIONServicios : ITEVOLUCIONServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TEVOLUCIONServicios()
        {
        }

        public async Task<int> Agregar(TEVOLUCION tevolucion)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TEVOLUCION.Add(tevolucion);
                await _dbcontext.SaveChangesAsync();
                return tevolucion.IDEVOLUCION ?? 0;
            }
        }

        public async Task Borrar(int IDEVOLUCION)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.FirstOrDefaultAsync(x => x.IDEVOLUCION == IDEVOLUCION);
                _dbcontext.TEVOLUCION.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TEVOLUCION> ConsultarPorIdEvolucion(int IDEVOLUCION)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.FirstOrDefaultAsync(x => x.IDEVOLUCION == IDEVOLUCION);
                return obj == null ? new TEVOLUCION() : obj;
            }
        }

        public async Task<List<TEVOLUCION>> ConsultarPorAnamnesis(int IDEVOLUSECUND)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION
                .Where(x => x.IDEVOLUSECUND == IDEVOLUSECUND)
                .OrderByDescending(x => x.FECHA)
                .ToListAsync();
                return obj == null ? new List<TEVOLUCION>() : obj;
            }
        }




        public async Task<bool> Editar(int IDEVOLUCION, TEVOLUCION tevolucion)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.FirstOrDefaultAsync(x => x.IDEVOLUCION == IDEVOLUCION);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tevolucion);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITEVOLUCIONServicios
    {
        Task<int> Agregar(TEVOLUCION tevolucion);
        Task<bool> Editar(int IDEVOLUCION, TEVOLUCION tevolucion);
        Task<TEVOLUCION> ConsultarPorIdEvolucion(int IDEVOLUCION);
        Task<List<TEVOLUCION>> ConsultarPorAnamnesis(int IDEVOLUSECUND);
        Task Borrar(int IDEVOLUCION);
    }
}

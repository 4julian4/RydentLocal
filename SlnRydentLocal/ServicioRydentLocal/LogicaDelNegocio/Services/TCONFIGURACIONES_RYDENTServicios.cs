using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCONFIGURACIONES_RYDENTServicios : ITCONFIGURACIONES_RYDENTServicios
    {
        private readonly AppDbContext _dbcontext;
        public TCONFIGURACIONES_RYDENTServicios()
        {
            
        }

        public async Task<int> Agregar(TCONFIGURACIONES_RYDENT tconfiguraciones_rydent)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.TCONFIGURACIONES_RYDENT.Add(tconfiguraciones_rydent);
                await _dbcontext.SaveChangesAsync();
                return tconfiguraciones_rydent.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCONFIGURACIONES_RYDENT.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TCONFIGURACIONES_RYDENT.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TCONFIGURACIONES_RYDENT> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCONFIGURACIONES_RYDENT.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TCONFIGURACIONES_RYDENT() : obj;
            }
        }

        public async Task<TCONFIGURACIONES_RYDENT> ConsultarPorNombre(string NOMBRE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCONFIGURACIONES_RYDENT.FirstOrDefaultAsync(x => x.NOMBRE == NOMBRE);
                return obj == null ? new TCONFIGURACIONES_RYDENT() : obj;
            }
        }

        public async Task<List<TCONFIGURACIONES_RYDENT>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCONFIGURACIONES_RYDENT.ToListAsync();
                return obj == null ? new List<TCONFIGURACIONES_RYDENT>() : obj;
            }
        }


        public async Task<bool> Editar(int ID, TCONFIGURACIONES_RYDENT tconfiguraciones_rydent)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCONFIGURACIONES_RYDENT.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tconfiguraciones_rydent);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITCONFIGURACIONES_RYDENTServicios
    {
        Task<int> Agregar(TCONFIGURACIONES_RYDENT tconfiguraciones_rydent);
        Task<bool> Editar(int ID, TCONFIGURACIONES_RYDENT tconfiguraciones_rydent);
        Task<TCONFIGURACIONES_RYDENT> ConsultarPorId(int ID);
        Task<TCONFIGURACIONES_RYDENT> ConsultarPorNombre(string NOMBRE);
        Task<List<TCONFIGURACIONES_RYDENT>> ConsultarTodos();
        Task Borrar(int ID);
    }
}

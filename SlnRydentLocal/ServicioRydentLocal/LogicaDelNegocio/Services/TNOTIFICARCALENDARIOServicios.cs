using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    
    public class TNOTIFICARCALENDARIOServicios : ITNOTIFICARCALENDARIOServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TNOTIFICARCALENDARIOServicios()
        {
        }

        public async Task<int> Agregar(TNOTIFICARCALENDARIO tnotificarcalendario)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TNOTIFICARCALENDARIO.Add(tnotificarcalendario);
                await _dbcontext.SaveChangesAsync();
                return tnotificarcalendario.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TNOTIFICARCALENDARIO.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TNOTIFICARCALENDARIO.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TNOTIFICARCALENDARIO> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TNOTIFICARCALENDARIO.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TNOTIFICARCALENDARIO() : obj;
            }
        }




        public async Task<bool> Editar(int ID, TNOTIFICARCALENDARIO tnotificarcalendario)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TNOTIFICARCALENDARIO.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tnotificarcalendario);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITNOTIFICARCALENDARIOServicios
    {
        Task<int> Agregar(TNOTIFICARCALENDARIO tnotificarcalendario);
        Task<bool> Editar(int ID, TNOTIFICARCALENDARIO tnotificarcalendario);
        Task<TNOTIFICARCALENDARIO> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

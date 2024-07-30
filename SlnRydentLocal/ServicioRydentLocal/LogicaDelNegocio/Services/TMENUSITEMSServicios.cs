using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TMENUSITEMSServicios : ITMENUSITEMSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TMENUSITEMSServicios()
        {
            
        }


        public async Task<int> Agregar(TMENUSITEMS tmenusitems)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.TMENUSITEMS.Add(tmenusitems);
                await _dbcontext.SaveChangesAsync();
                return tmenusitems.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TMENUSITEMS.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TMENUSITEMS.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TMENUSITEMS> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TMENUSITEMS.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TMENUSITEMS() : obj;
            }
        }




        public async Task<bool> Editar(int ID, TMENUSITEMS tmenusitems)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TMENUSITEMS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tmenusitems);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }

        }
    }

    public interface ITMENUSITEMSServicios
    {
        Task<int> Agregar(TMENUSITEMS tmenusitems);
        Task<bool> Editar(int ID, TMENUSITEMS tmenusitems);
        Task<TMENUSITEMS> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

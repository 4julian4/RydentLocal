using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TINFORMACIONREPORTESServicios : ITINFORMACIONREPORTESServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TINFORMACIONREPORTESServicios()
        {
        }

        public async Task<int> Agregar(TINFORMACIONREPORTES tinformacionreportes)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TINFORMACIONREPORTES.Add(tinformacionreportes);
                await _dbcontext.SaveChangesAsync();
                return tinformacionreportes.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TINFORMACIONREPORTES.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TINFORMACIONREPORTES> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TINFORMACIONREPORTES() : obj;
            }
        }




        public async Task<bool> Editar(int ID, TINFORMACIONREPORTES tinformacionreportes)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tinformacionreportes);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITINFORMACIONREPORTESServicios
    {
        Task<int> Agregar(TINFORMACIONREPORTES tinformacionreportes);
        Task<bool> Editar(int ID, TINFORMACIONREPORTES tinformacionreportes);
        Task<TINFORMACIONREPORTES> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

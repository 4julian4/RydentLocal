using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_RIPS_DXServicios : IT_RIPS_DXServicios
    {
        protected readonly AppDbContext _dbcontext;
        public T_RIPS_DXServicios()
        {
        }

        public async Task<bool> Agregar(T_RIPS_DX t_rips_dx)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.T_RIPS_DX.Add(t_rips_dx);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
        }


        
        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_DX.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj != null)
                {
                    _dbcontext.T_RIPS_DX.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        
        public async Task<T_RIPS_DX> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_DX.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_RIPS_DX() : obj;
            }
        }




        public async Task<bool> Editar(int ID, T_RIPS_DX t_rips_dx)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_DX.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_rips_dx);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }

        
    }

    public interface IT_RIPS_DXServicios
    {
        Task<bool> Agregar(T_RIPS_DX t_rips_dx);
        Task<bool> Editar(int ID, T_RIPS_DX t_rips_dx);
        Task<T_RIPS_DX> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}
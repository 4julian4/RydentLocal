using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_PRESUPUESTOServicios : IT_PRESUPUESTOServicios
    {
        private readonly AppDbContext _dbcontext;
        public T_PRESUPUESTOServicios()
        {
            
        }
        

        public async Task<int> Agregar(T_PRESUPUESTO t_presupuesto)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.T_PRESUPUESTO.Add(t_presupuesto);
                await _dbcontext.SaveChangesAsync();
                return t_presupuesto.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_PRESUPUESTO.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.T_PRESUPUESTO.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }       
        }

        public async Task<T_PRESUPUESTO> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_PRESUPUESTO.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_PRESUPUESTO() : obj;
            }    
        }




        public async Task<bool> Editar(int ID, T_PRESUPUESTO t_presupuesto)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_PRESUPUESTO.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_presupuesto);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface IT_PRESUPUESTOServicios
    {
        Task<int> Agregar(T_PRESUPUESTO t_presupuesto);
        Task<bool> Editar(int ID, T_PRESUPUESTO t_presupuesto);
        Task<T_PRESUPUESTO> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

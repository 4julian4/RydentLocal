using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_RIPS_PROCEDIMIENTOSServicios : IT_RIPS_PROCEDIMIENTOSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public T_RIPS_PROCEDIMIENTOSServicios()
        {
        }

        public async Task<bool> Agregar(T_RIPS_PROCEDIMIENTOS t_rips_procedimientos)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.T_RIPS_PROCEDIMIENTOS.Add(t_rips_procedimientos);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
        }



        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_PROCEDIMIENTOS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj != null)
                {
                    _dbcontext.T_RIPS_PROCEDIMIENTOS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }


        public async Task<T_RIPS_PROCEDIMIENTOS> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_PROCEDIMIENTOS.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_RIPS_PROCEDIMIENTOS() : obj;
            }
        }




        public async Task<bool> Editar(int ID, T_RIPS_PROCEDIMIENTOS t_rips_procedimientos)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_RIPS_PROCEDIMIENTOS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_rips_procedimientos);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }


    }

    public interface IT_RIPS_PROCEDIMIENTOSServicios
    {
        Task<bool> Agregar(T_RIPS_PROCEDIMIENTOS t_rips_procedimientos);
        Task<bool> Editar(int ID, T_RIPS_PROCEDIMIENTOS t_rips_procedimientos);
        Task<T_RIPS_PROCEDIMIENTOS> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}
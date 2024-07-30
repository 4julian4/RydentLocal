using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{

    public class T_CONFIGURACION_VALORServicios : IT_CONFIGURACION_VALORServicios
    {
        private readonly AppDbContext _dbcontext;
        public T_CONFIGURACION_VALORServicios()
        {
            
        }

        public async Task<int> Agregar(T_CONFIGURACION_VALOR t_configuracion_valor)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.T_CONFIGURACION_VALOR.Add(t_configuracion_valor);
                await _dbcontext.SaveChangesAsync();
                return t_configuracion_valor.ID;
            }

        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_CONFIGURACION_VALOR.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.T_CONFIGURACION_VALOR.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<T_CONFIGURACION_VALOR> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_CONFIGURACION_VALOR.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_CONFIGURACION_VALOR() : obj;
            }
        }




        public async Task<bool> Editar(int ID, T_CONFIGURACION_VALOR t_configuracion_valor)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_CONFIGURACION_VALOR.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_configuracion_valor);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface IT_CONFIGURACION_VALORServicios
    {
        Task<int> Agregar(T_CONFIGURACION_VALOR t_configuracion_valor);
        Task<bool> Editar(int ID, T_CONFIGURACION_VALOR t_configuracion_valor);
        Task<T_CONFIGURACION_VALOR> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

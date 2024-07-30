using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_FRASE_XEVOLUCIONServicios : IT_FRASE_XEVOLUCIONServicios
    {
        private readonly AppDbContext _dbcontext;
        public T_FRASE_XEVOLUCIONServicios()
        {
            
        }


        public async Task<List<T_FRASE_XEVOLUCION>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_FRASE_XEVOLUCION.ToListAsync();
                return obj == null ? new List<T_FRASE_XEVOLUCION>() : obj;
            }
        }


    }

    public interface IT_FRASE_XEVOLUCIONServicios
    {
        Task<List<T_FRASE_XEVOLUCION>> ConsultarTodos();
    }
}
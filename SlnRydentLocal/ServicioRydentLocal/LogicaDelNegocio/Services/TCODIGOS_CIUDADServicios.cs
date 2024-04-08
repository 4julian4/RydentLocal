using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCODIGOS_CIUDADServicios : ITCODIGOS_CIUDADServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCODIGOS_CIUDADServicios()
        {
        }


        public async Task<List<TCODIGOS_CIUDAD>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCODIGOS_CIUDAD.ToListAsync();
                return obj == null ? new List<TCODIGOS_CIUDAD>() : obj;
            }
        }


    }

    public interface ITCODIGOS_CIUDADServicios
    {

        Task<List<TCODIGOS_CIUDAD>> ConsultarTodos();
    }
}
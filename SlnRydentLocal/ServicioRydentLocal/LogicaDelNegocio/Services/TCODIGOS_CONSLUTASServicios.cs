using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    internal class TCODIGOS_CONSLUTASServicios: ITCODIGOS_CONSLUTASServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCODIGOS_CONSLUTASServicios()
        {
        }


        public async Task<List<TCODIGOS_CONSLUTAS>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCODIGOS_CONSLUTAS.ToListAsync();
                return obj == null ? new List<TCODIGOS_CONSLUTAS>() : obj;
            }
        }


    }

    public interface ITCODIGOS_CONSLUTASServicios
    {

        Task<List<TCODIGOS_CONSLUTAS>> ConsultarTodos();
    }
}

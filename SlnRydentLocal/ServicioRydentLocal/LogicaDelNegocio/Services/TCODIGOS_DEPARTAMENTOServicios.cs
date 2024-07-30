using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCODIGOS_DEPARTAMENTOServicios: ITCODIGOS_DEPARTAMENTOServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCODIGOS_DEPARTAMENTOServicios()
        {
            
        }
        


        public async Task<List<TCODIGOS_DEPARTAMENTO>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCODIGOS_DEPARTAMENTO.ToListAsync();
                return obj == null ? new List<TCODIGOS_DEPARTAMENTO>() : obj;
            }
        }


    }

    public interface ITCODIGOS_DEPARTAMENTOServicios
    {

        Task<List<TCODIGOS_DEPARTAMENTO>> ConsultarTodos();
    }
}
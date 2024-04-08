using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    
    public class  TCODIGOS_EPSServicios : ITCODIGOS_EPSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public  TCODIGOS_EPSServicios()
        {
        }

        
        public async Task<List<TCODIGOS_EPS>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCODIGOS_EPS.ToListAsync();
                return obj == null ? new List<TCODIGOS_EPS>() : obj;
            }
        }

               
    }

    public interface ITCODIGOS_EPSServicios
    {
        
        Task<List<TCODIGOS_EPS>> ConsultarTodos();
    }
}
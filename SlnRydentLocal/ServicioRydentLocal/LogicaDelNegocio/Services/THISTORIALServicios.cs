using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class THISTORIALServicios : ITHISTORIALServicios
    {
        protected readonly AppDbContext _dbcontext;
        public THISTORIALServicios()
        {
        }

        public async Task<THISTORIAL> Agregar(THISTORIAL thistorial)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.THISTORIAL.Add(thistorial);
                await _dbcontext.SaveChangesAsync();
                return thistorial;
            }
        }

               
    }

    public interface ITHISTORIALServicios
    {
        Task<THISTORIAL> Agregar(THISTORIAL thistorial);
    }
    
}

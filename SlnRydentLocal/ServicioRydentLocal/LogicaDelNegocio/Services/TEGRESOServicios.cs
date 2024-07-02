using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TEGRESOServicios: ITEGRESOServicios  
    {
        protected readonly AppDbContext _dbcontext;
        public TEGRESOServicios()
        {
        }

        public async Task<double> BuscarTotalEgresosPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEGRESO
                    .Where(x => x.FECHA >= fechaInicio && x.FECHA <= fechaFin)
                    .Select(x => x.VALOR_EGRESO)
                    .SumAsync();
                return obj ?? 0;
            }
        }
       
    }
    public interface ITEGRESOServicios
    {
        Task<double> BuscarTotalEgresosPorFecha(DateTime fechaInicio, DateTime fechaFin);
        
    }
}

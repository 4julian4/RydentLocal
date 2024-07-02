using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    internal class TINGRESOServicios: ITINGRESOServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TINGRESOServicios()
        {
        }

        public async Task<double> BuscarTotalIngresosPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINGRESO
                    .Where(x => x.FECHA >= fechaInicio && x.FECHA <= fechaFin)
                    .Select(x => x.VALOR_INGRESO)
                    .SumAsync();
                return obj ?? 0;
            }
        }

    }
    public interface ITINGRESOServicios
    {
        Task<double> BuscarTotalIngresosPorFecha(DateTime fechaInicio, DateTime fechaFin);

    }
}

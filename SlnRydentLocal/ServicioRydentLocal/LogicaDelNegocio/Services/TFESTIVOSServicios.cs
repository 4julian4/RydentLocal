using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TFESTIVOSServicios : ITFESTIVOSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TFESTIVOSServicios()
        {

        }


        public async Task<TFESTIVOS> ConsultarPorFecha(DateTime FECHA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFESTIVOS.Where(x => x.FECHA.Date == FECHA.Date).ToListAsync();
                return !obj.Any() ? new TFESTIVOS() : obj.FirstOrDefault();
            }
        }


        public async Task<List<TFESTIVOS>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFESTIVOS.ToListAsync();
                return obj == null ? new List<TFESTIVOS>() : obj;
            }
        }


    }

    public interface ITFESTIVOSServicios
    {

        Task<List<TFESTIVOS>> ConsultarTodos();
        Task<TFESTIVOS> ConsultarPorFecha(DateTime FECHA);
    }
}


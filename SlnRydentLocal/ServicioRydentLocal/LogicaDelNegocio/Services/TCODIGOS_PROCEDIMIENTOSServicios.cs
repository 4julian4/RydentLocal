using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCODIGOS_PROCEDIMIENTOSServicios : ITCODIGOS_PROCEDIMIENTOSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCODIGOS_PROCEDIMIENTOSServicios()
        {
        }


        public async Task<List<TCODIGOS_PROCEDIMIENTOS>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                try
                {
                    var obj = await _dbcontext.TCODIGOS_PROCEDIMIENTOS.ToListAsync();
                    return obj == null ? new List<TCODIGOS_PROCEDIMIENTOS>() : obj;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //var obj = await _dbcontext.TCODIGOS_PROCEDIMIENTOS.ToListAsync();
                //return obj == null ? new List<TCODIGOS_PROCEDIMIENTOS>() : obj;
            }
        }


    }

    public interface ITCODIGOS_PROCEDIMIENTOSServicios
    {
        Task<List<TCODIGOS_PROCEDIMIENTOS>> ConsultarTodos();
    }
}
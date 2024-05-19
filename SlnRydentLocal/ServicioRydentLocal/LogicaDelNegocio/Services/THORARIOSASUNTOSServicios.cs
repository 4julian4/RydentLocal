using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class THORARIOSASUNTOSServicios : ITHORARIOSASUNTOSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public THORARIOSASUNTOSServicios()
        {
        }

        public async Task<List<THORARIOSASUNTOS>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.THORARIOSASUNTOS.ToListAsync();
                return obj == null ? new List<THORARIOSASUNTOS>() : obj;
            }
        }
    }
    public interface ITHORARIOSASUNTOSServicios
    {
        Task<List<THORARIOSASUNTOS>> ConsultarTodos();
    }
}

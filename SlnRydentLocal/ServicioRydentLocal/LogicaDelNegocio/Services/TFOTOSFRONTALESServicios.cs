using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TFOTOSFRONTALESServicios : ITFOTOSFRONTALESServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TFOTOSFRONTALESServicios()
        {
            
        }


        public async Task<TFOTOSFRONTALES> ConsultarPorId(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFOTOSFRONTALES.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                return obj == null ? new TFOTOSFRONTALES() : obj;
            }
        }

        public async Task<string> ConsultarBase64PorId(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFOTOSFRONTALES.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                return (obj == null || obj.FOTOFRENTE.Length < 1) ? "" : Convert.ToBase64String(obj.FOTOFRENTE);
            }
        }
    }

    public interface ITFOTOSFRONTALESServicios
    {
        Task<TFOTOSFRONTALES> ConsultarPorId(int IDANAMNESIS);
        Task<string> ConsultarBase64PorId(int IDANAMNESIS);
    }
}

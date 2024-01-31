using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCLAVEServicios : ITCLAVEServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TCLAVEServicios()
        {
        }

        public async Task<TCLAVE> Agregar(TCLAVE tclave)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TCLAVE.Add(tclave);
                await _dbcontext.SaveChangesAsync();
                return tclave;
            }
        }

        public async Task Borrar(string CLAVE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCLAVE.FirstOrDefaultAsync(x => x.CLAVE == CLAVE);
                _dbcontext.TCLAVE.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TCLAVE> ConsultarPorId(string CLAVE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCLAVE.FirstOrDefaultAsync(x => x.CLAVE == CLAVE);
                return obj == null ? new TCLAVE() : obj;
            }
        }




        public async Task<bool> Editar(string CLAVE, TCLAVE tclave)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCLAVE.FirstOrDefaultAsync(x => x.CLAVE == CLAVE);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tclave);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITCLAVEServicios
    {
        Task<TCLAVE> Agregar(TCLAVE tclave);
        Task<bool> Editar(string CLAVE, TCLAVE tclave);
        Task<TCLAVE> ConsultarPorId(string CLAVE);
        Task Borrar(string CLAVE);
    }
}

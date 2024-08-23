using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{

    public class TCITASBORRADASServicios : ITCITASBORRADASServicios
    {
        private readonly AppDbContext _dbcontext;
        public TCITASBORRADASServicios()
        {
            
        }
            
        public async Task<TCITASBORRADAS> Agregar(TCITASBORRADAS  tcitasborradas)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.TCITASBORRADAS.Add(tcitasborradas);
                await _dbcontext.SaveChangesAsync();
                return (tcitasborradas);
            }
                       
        }

        public async Task Borrar(int SILLA, DateTime FECHA, TimeSpan HORA)
        {
            using (var _dbcontext = new AppDbContext())
            {

                var obj = await _dbcontext.TCITASBORRADAS.FirstOrDefaultAsync(x => x.SILLA == SILLA && x.FECHA == FECHA && x.HORA == HORA);
                if (obj != null)
                {
                    _dbcontext.TCITASBORRADAS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        public async Task<TCITASBORRADAS> ConsultarPorId(int SILLA, DateTime FECHA, TimeSpan HORA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCITASBORRADAS.FirstOrDefaultAsync(x => x.SILLA == SILLA && x.FECHA == FECHA && x.HORA == HORA);
                return obj == null ? new TCITASBORRADAS() : obj;
            }
        }




        public async Task<bool> Editar(int SILLA, DateTime FECHA, TimeSpan HORA, TCITASBORRADAS tcitasborradas)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TCITASBORRADAS.FirstOrDefaultAsync(x => x.SILLA == SILLA && x.FECHA == FECHA && x.HORA == HORA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tcitasborradas);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITCITASBORRADASServicios
    {
        Task<TCITASBORRADAS> Agregar(TCITASBORRADAS tcitasborradas);
        Task<bool> Editar(int SILLA, DateTime FECHA, TimeSpan HORA, TCITASBORRADAS tcitasborradas);
        Task<TCITASBORRADAS> ConsultarPorId(int SILLA, DateTime FECHA, TimeSpan HORA);
        Task Borrar(int SILLA, DateTime FECHA, TimeSpan HORA);
    }
}

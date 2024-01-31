using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TDATOSCLIENTESServicios : ITDATOSCLIENTESServicios
    {
        public async Task<TDATOSCLIENTES> Agregar(TDATOSCLIENTES tdatosclientes)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TDATOSCLIENTES.Add(tdatosclientes);
                await _dbcontext.SaveChangesAsync();
                return tdatosclientes;
            }
        }

        public async Task Borrar(string NOMBRE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSCLIENTES.FirstOrDefaultAsync(x => x.NOMBRE == NOMBRE);
                _dbcontext.TDATOSCLIENTES.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TDATOSCLIENTES> ConsultarPorId(string NOMBRE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSCLIENTES.FirstOrDefaultAsync(x => x.NOMBRE == NOMBRE);
                return obj == null ? new TDATOSCLIENTES() : obj;
            }
        }

        public async Task<bool> Editar(string NOMBRE, TDATOSCLIENTES tdatosclientes)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSCLIENTES.FirstOrDefaultAsync(x => x.NOMBRE == NOMBRE);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tdatosclientes);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }
    public interface ITDATOSCLIENTESServicios
    {
        Task<TDATOSCLIENTES> Agregar(TDATOSCLIENTES tdatosclientes);
        Task<bool> Editar(string NOMBRE, TDATOSCLIENTES tdatosclientes);
        Task<TDATOSCLIENTES> ConsultarPorId(string NOMBRE);
        Task Borrar(string NOMBRE);
    }
}



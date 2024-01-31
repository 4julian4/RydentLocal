using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_CUENTASXCOBRARServicios : IT_CUENTASXCOBRARServicios
    {
        protected readonly AppDbContext _dbcontext;
        public T_CUENTASXCOBRARServicios()
        {
        }

        public async Task<int> Agregar(T_CUENTASXCOBRAR t_cuentasxcobrar)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.T_CUENTASXCOBRAR.Add(t_cuentasxcobrar);
                await _dbcontext.SaveChangesAsync();
                return t_cuentasxcobrar.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_CUENTASXCOBRAR.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.T_CUENTASXCOBRAR.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<T_CUENTASXCOBRAR> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_CUENTASXCOBRAR.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new T_CUENTASXCOBRAR() : obj;
            }
        }




        public async Task<bool> Editar(int ID, T_CUENTASXCOBRAR t_cuentasxcobrar)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_CUENTASXCOBRAR.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_cuentasxcobrar);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface IT_CUENTASXCOBRARServicios
    {
        Task<int> Agregar(T_CUENTASXCOBRAR t_cuentasxcobrar);
        Task<bool> Editar(int ID, T_CUENTASXCOBRAR t_cuentasxcobrar);
        Task<T_CUENTASXCOBRAR> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

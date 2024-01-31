using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{

    public class TMENUSServicios : ITMENUSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TMENUSServicios()
        {
        }

        public async Task<int> Agregar(TMENUS tmenus)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TMENUS.Add(tmenus);
                await _dbcontext.SaveChangesAsync();
                return tmenus.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TMENUS.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TMENUS.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TMENUS> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TMENUS.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TMENUS() : obj;
            }
        }




        public async Task<bool> Editar(int ID, TMENUS tmenus)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TMENUS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tmenus);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITMENUSServicios
    {
        Task<int> Agregar(TMENUS tmenus);
        Task<bool> Editar(int ID, TMENUS tmenus);
        Task<TMENUS> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

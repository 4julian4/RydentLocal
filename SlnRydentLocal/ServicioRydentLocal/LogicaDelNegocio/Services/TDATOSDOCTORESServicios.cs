using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System.ComponentModel;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{

    public class TDATOSDOCTORESServicios : ITDATOSDOCTORESServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TDATOSDOCTORESServicios()
        {
        }

        public async Task<int> Agregar(TDATOSDOCTORES tdatosdoctores)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TDATOSDOCTORES.Add(tdatosdoctores);
                await _dbcontext.SaveChangesAsync();
                return tdatosdoctores.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSDOCTORES.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TDATOSDOCTORES.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TDATOSDOCTORES> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSDOCTORES.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TDATOSDOCTORES() : obj;
            }
        }
        
        public async Task<List<TDATOSDOCTORES>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSDOCTORES.ToListAsync();
                return obj == null ? new List<TDATOSDOCTORES>() : obj;
            }
        }



        public async Task<bool> Editar(int ID, TDATOSDOCTORES tdatosdoctores)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDATOSDOCTORES.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tdatosdoctores);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITDATOSDOCTORESServicios
    {
        Task<int> Agregar(TDATOSDOCTORES tdatosdoctores);
        Task<bool> Editar(int ID, TDATOSDOCTORES tdatosdoctores);
        Task<TDATOSDOCTORES> ConsultarPorId(int ID);
        Task<List<TDATOSDOCTORES>> ConsultarTodos();
        Task Borrar(int ID);
    }
}

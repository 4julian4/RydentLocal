using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCITASPENDIENTESServicios : ITCITASPENDIENTESServicios
    {
        private readonly AppDbContext _dbcontext;
        public TCITASPENDIENTESServicios(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }


        public async Task<int> Agregar(TCITASPENDIENTES tcitaspendientes)
        {


            _dbcontext.TCITASPENDIENTES.Add(tcitaspendientes);
            await _dbcontext.SaveChangesAsync();
            return tcitaspendientes.ID;

        }

        public async Task Borrar(int ID)
        {

            var obj = await _dbcontext.TCITASPENDIENTES.FirstOrDefaultAsync(x => x.ID == ID);
            _dbcontext.TCITASPENDIENTES.Remove(obj);
            await _dbcontext.SaveChangesAsync();

        }

        public async Task<TCITASPENDIENTES> ConsultarPorId(int ID)
        {

            var obj = await _dbcontext.TCITASPENDIENTES.FirstOrDefaultAsync(x => x.ID == ID);
            return obj == null ? new TCITASPENDIENTES() : obj;

        }




        public async Task<bool> Editar(int ID, TCITASPENDIENTES tcitaspendientes)
        {

            var obj = await _dbcontext.TCITASPENDIENTES.FirstOrDefaultAsync(x => x.ID == ID);
            if (obj == null)
            {
                return false;
            }
            else
            {
                _dbcontext.Entry(obj).CurrentValues.SetValues(tcitaspendientes);
                await _dbcontext.SaveChangesAsync();
                return true;
            }

        }
    }

    public interface ITCITASPENDIENTESServicios
    {
        Task<int> Agregar(TCITASPENDIENTES tcitaspendientes);
        Task<bool> Editar(int ID, TCITASPENDIENTES tcitaspendientes);
        Task<TCITASPENDIENTES> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

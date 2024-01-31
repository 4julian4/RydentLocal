using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_ADICIONALES_ABONOSServicios : IT_ADICIONALES_ABONOSServicios
    {
        protected readonly AppDbContext _dbcontext;
        public T_ADICIONALES_ABONOSServicios()
        {
        }

        public async Task<T_ADICIONALES_ABONOS> Agregar(T_ADICIONALES_ABONOS t_adicionales_abonos)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.T_ADICIONALES_ABONOS.Add(t_adicionales_abonos);
                await _dbcontext.SaveChangesAsync();
                return t_adicionales_abonos;
            }
        }

        public async Task Borrar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.FirstOrDefaultAsync(x => x.ID == ID && x.IDENTIFICADOR == IDENTIFICADOR && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                if (obj != null) 
                {
                    _dbcontext.T_ADICIONALES_ABONOS.Remove(obj);
                    await _dbcontext.SaveChangesAsync();
                }
            }
        }

        public async Task<T_ADICIONALES_ABONOS> ConsultarPorId(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.FirstOrDefaultAsync(x => x.ID == ID && x.IDENTIFICADOR == IDENTIFICADOR && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                return obj == null ? new T_ADICIONALES_ABONOS() : obj;
            }
        }




        public async Task<bool> Editar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE, T_ADICIONALES_ABONOS t_adicionales_abonos)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_ADICIONALES_ABONOS.FirstOrDefaultAsync(x => x.ID == ID && x.IDENTIFICADOR == IDENTIFICADOR && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(t_adicionales_abonos);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface IT_ADICIONALES_ABONOSServicios
    {
        Task<T_ADICIONALES_ABONOS> Agregar(T_ADICIONALES_ABONOS t_adicionales_abonos);
        Task<bool> Editar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE, T_ADICIONALES_ABONOS t_adicionales_abonos);
        Task<T_ADICIONALES_ABONOS> ConsultarPorId(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE);
        Task Borrar(int ID, int IDENTIFICADOR, int IDDOCTOR, int FASE);
    }
}

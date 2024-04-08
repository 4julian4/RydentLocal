using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TANAMNESISServicios : ITANAMNESISServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TANAMNESISServicios()
        {
        }

        public async Task<int> Agregar(TANAMNESIS tanamnesis)
        {
            using (var _dbcontext = new AppDbContext())
            {
                
                _dbcontext.TANAMNESIS.Add(tanamnesis);
                await _dbcontext.SaveChangesAsync();
                return tanamnesis.IDANAMNESIS;
            }
        }

        public async Task Borrar(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                _dbcontext.TANAMNESIS.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TANAMNESIS> ConsultarPorId(int IDANAMNESIS)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                return obj == null ? new TANAMNESIS() : obj;
            }
        }

        public async Task<TANAMNESIS> ConsultarPorIdTexto(string IDANAMNESIS_TEXTO)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS_TEXTO == IDANAMNESIS_TEXTO);
                return obj == null ? new TANAMNESIS() : obj;
            }
        }

        public async Task<int> ConsultarTotalPacientesPorDoctor(int IDDOCTOR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TTRATAMIENTO.Where(x => x.ID_DOCTOR == IDDOCTOR).Select(x => x.IDTRATAMIENTO).Distinct().ToListAsync();
                return obj == null ? 0 : obj.Count();
            }
        }

        public async Task<List<P_BUSCARPACIENTE>> BuscarPacientePorTipo(int TIPO, string BUSCAR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                BUSCAR = BUSCAR.Trim().Replace("  ", " ").Replace(" ", "%").ToUpper();
                var obj = await _dbcontext.P_BUSCARPACIENTE(TIPO, BUSCAR);
                return obj == null ? new List<P_BUSCARPACIENTE>() : obj;
            }
        }

        public async Task<bool> Editar(int IDANAMNESIS, TANAMNESIS tanamnesis)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TANAMNESIS.FirstOrDefaultAsync(x => x.IDANAMNESIS == IDANAMNESIS);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tanamnesis);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITANAMNESISServicios
    {
        Task<int> Agregar(TANAMNESIS tanamnesis);
        Task<bool> Editar(int IDANAMNESIS, TANAMNESIS tanamnesis);
        Task<TANAMNESIS> ConsultarPorId(int IDANAMNESIS);
        Task<List<P_BUSCARPACIENTE>> BuscarPacientePorTipo(int TIPO, string BUSCAR);
        Task Borrar(int IDANAMNESIS);
    }
}
        


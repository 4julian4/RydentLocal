using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TFIRMAServicios : ITFIRMAServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TFIRMAServicios()
        {
        }

        public async Task<int> Agregar(TFIRMA tfirma)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TFIRMA.Add(tfirma);
                await _dbcontext.SaveChangesAsync();
                return tfirma.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFIRMA.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TFIRMA.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TFIRMA> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFIRMA.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TFIRMA() : obj;
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

        

        public async Task<bool> Editar(int ID, TFIRMA tfirma)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TFIRMA.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tfirma);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITFIRMAServicios
    {
        Task<int> Agregar(TFIRMA tfirma);
        Task<bool> Editar(int ID, TFIRMA tfirma);
        Task<TFIRMA> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

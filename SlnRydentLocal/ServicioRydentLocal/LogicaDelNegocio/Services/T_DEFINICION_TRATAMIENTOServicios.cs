using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class T_DEFINICION_TRATAMIENTOServicios : IT_DEFINICION_TRATAMIENTOServicios
    {
        protected readonly AppDbContext _dbcontext;
        public T_DEFINICION_TRATAMIENTOServicios()
        {
            
        }
        
        public async Task<List<T_DEFINICION_TRATAMIENTO>> ConsultarPorIdAnamnesisIdDoctor(int ID, int IDDOCTOR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_DEFINICION_TRATAMIENTO.Where(x => x.ID == ID && x.IDDOCTOR == IDDOCTOR).OrderBy(x => x.FASE).ToListAsync();
                return obj;
            }
        }
        public async Task<T_DEFINICION_TRATAMIENTO> ConsultarPorId(int ID, int IDDOCTOR, int FASE)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.T_DEFINICION_TRATAMIENTO.FirstOrDefaultAsync(x => x.ID == ID && x.IDDOCTOR == IDDOCTOR && x.FASE == FASE);
                return obj == null ? new T_DEFINICION_TRATAMIENTO() : obj;
            }
            
        }

           
    }
    public interface IT_DEFINICION_TRATAMIENTOServicios
    {
        Task<List<T_DEFINICION_TRATAMIENTO>> ConsultarPorIdAnamnesisIdDoctor(int ID, int IDDOCTOR);
        Task<T_DEFINICION_TRATAMIENTO> ConsultarPorId(int ID, int IDDOCTOR, int FASE);
    }
}

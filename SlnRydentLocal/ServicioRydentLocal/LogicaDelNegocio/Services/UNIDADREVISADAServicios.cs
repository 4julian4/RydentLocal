using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class UNIDADREVISADAServicios : IUNIDADREVISADAServicios
    {
        protected readonly AppDbContext _dbcontext;
        private readonly AppDbContext _dbContext;
        public UNIDADREVISADAServicios(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        

        public async Task<int> Agregar(UNIDADREVISADA unidadrevisada)
        {
            

                _dbcontext.UNIDADREVISADA.Add(unidadrevisada);
                await _dbcontext.SaveChangesAsync();
                return unidadrevisada.IDUNIDADREVISADA;
            
        }

        public async Task Borrar(int IDUNIDADREVISADA)
        {
            
                var obj = await _dbcontext.UNIDADREVISADA.FirstOrDefaultAsync(x => x.IDUNIDADREVISADA == IDUNIDADREVISADA);
                _dbcontext.UNIDADREVISADA.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            
        }

        public async Task<UNIDADREVISADA> ConsultarPorId(int IDUNIDADREVISADA)
        {
            
                var obj = await _dbcontext.UNIDADREVISADA.FirstOrDefaultAsync(x => x.IDUNIDADREVISADA == IDUNIDADREVISADA);
                return obj == null ? new UNIDADREVISADA() : obj;
            
        }




        public async Task<bool> Editar(int IDUNIDADREVISADA, UNIDADREVISADA unidadrevisada)
        {
            
                var obj = await _dbcontext.UNIDADREVISADA.FirstOrDefaultAsync(x => x.IDUNIDADREVISADA == IDUNIDADREVISADA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(unidadrevisada);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            
        }
    }

    public interface IUNIDADREVISADAServicios
    {
        Task<int> Agregar(UNIDADREVISADA unidadrevisada);
        Task<bool> Editar(int IDUNIDADREVISADA, UNIDADREVISADA unidadrevisada);
        Task<UNIDADREVISADA> ConsultarPorId(int IDUNIDADREVISADA);
        Task Borrar(int IDUNIDADREVISADA);
    }
}

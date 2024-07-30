using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{

    public class TRESOLUCION_DIAN_OTROSServicios : ITRESOLUCION_DIAN_OTROSServicios
    {
        private readonly AppDbContext _dbcontext;
        public TRESOLUCION_DIAN_OTROSServicios()
        {
            
        }


        public async Task<int> Agregar(TRESOLUCION_DIAN_OTROS tresolucion_dian_otros)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TRESOLUCION_DIAN_OTROS.Add(tresolucion_dian_otros);
                await _dbcontext.SaveChangesAsync();
                return tresolucion_dian_otros.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TRESOLUCION_DIAN_OTROS.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TRESOLUCION_DIAN_OTROS.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TRESOLUCION_DIAN_OTROS> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TRESOLUCION_DIAN_OTROS.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TRESOLUCION_DIAN_OTROS() : obj;
            }
        }




        public async Task<bool> Editar(int ID, TRESOLUCION_DIAN_OTROS tresolucion_dian_otros)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TRESOLUCION_DIAN_OTROS.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tresolucion_dian_otros);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITRESOLUCION_DIAN_OTROSServicios
    {
        Task<int> Agregar(TRESOLUCION_DIAN_OTROS tresolucion_dian_otros);
        Task<bool> Editar(int ID, TRESOLUCION_DIAN_OTROS tresolucion_dian_otros);
        Task<TRESOLUCION_DIAN_OTROS> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

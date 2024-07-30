using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TCONFIGURACIONCALENDARIOServicios : ITCONFIGURACIONCALENDARIOServicios
    {
        private readonly AppDbContext _dbcontext;
        public TCONFIGURACIONCALENDARIOServicios(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<int> Agregar(TCONFIGURACIONCALENDARIO tconfiguracioncalendario)
        {


            _dbcontext.TCONFIGURACIONCALENDARIO.Add(tconfiguracioncalendario);
            await _dbcontext.SaveChangesAsync();
            return tconfiguracioncalendario.ID;

        }

        public async Task Borrar(int ID)
        {

            var obj = await _dbcontext.TCONFIGURACIONCALENDARIO.FirstOrDefaultAsync(x => x.ID == ID);
            _dbcontext.TCONFIGURACIONCALENDARIO.Remove(obj);
            await _dbcontext.SaveChangesAsync();

        }

        public async Task<TCONFIGURACIONCALENDARIO> ConsultarPorId(int ID)
        {

            var obj = await _dbcontext.TCONFIGURACIONCALENDARIO.FirstOrDefaultAsync(x => x.ID == ID);
            return obj == null ? new TCONFIGURACIONCALENDARIO() : obj;

        }




        public async Task<bool> Editar(int ID, TCONFIGURACIONCALENDARIO tconfiguracioncalendario)
        {

            var obj = await _dbcontext.TCONFIGURACIONCALENDARIO.FirstOrDefaultAsync(x => x.ID == ID);
            if (obj == null)
            {
                return false;
            }
            else
            {
                _dbcontext.Entry(obj).CurrentValues.SetValues(tconfiguracioncalendario);
                await _dbcontext.SaveChangesAsync();
                return true;
            }

        }
    }

    public interface ITCONFIGURACIONCALENDARIOServicios
    {
        Task<int> Agregar(TCONFIGURACIONCALENDARIO tconfiguracioncalendario);
        Task<bool> Editar(int ID, TCONFIGURACIONCALENDARIO tconfiguracioncalendario);
        Task<TCONFIGURACIONCALENDARIO> ConsultarPorId(int ID);
        Task Borrar(int ID);
    }
}

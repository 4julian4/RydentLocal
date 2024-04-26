using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TDETALLECITASServicios : ITDETALLECITASServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TDETALLECITASServicios()
        {
        }

        public async Task<TDETALLECITAS> Agregar(TDETALLECITAS tdetallecitas)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TDETALLECITAS.Add(tdetallecitas);
                await _dbcontext.SaveChangesAsync();
                return tdetallecitas;
            }
        }

        public async Task<bool> Borrar(DateTime FECHA, int SILLA, TimeSpan HORA)
        {
            if (FECHA != null && SILLA != null && HORA != null)
            {
                using (var _dbcontext = new AppDbContext())
                {
                    var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.FECHA == FECHA && x.SILLA == SILLA && x.HORA == HORA);
                    _dbcontext.TDETALLECITAS.Remove(obj);
                    var affectedRows = await _dbcontext.SaveChangesAsync();
                    return affectedRows > 0;
                }
            }
            else
            {
                return false;
            }
        }
        public async Task<TDETALLECITAS>ConsultarPorIdDetalleCitas(string ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TDETALLECITAS() : obj;
            }
        }

        
        public async Task<TDETALLECITAS> ConsultarPorId(DateTime FECHA, int SILLA, TimeSpan HORA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.FECHA == FECHA && x.SILLA == SILLA && x.HORA == HORA);
                return obj == null ? new TDETALLECITAS() : obj;
            }
        }


        public async Task<List<TDETALLECITAS>> ConsultarPacienteConCitaRepetida(string NOMBRE, DateTime FECHA, string historia)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS
                    .Where(x => x.FECHA >= FECHA.Date && x.NOMBRE == NOMBRE && x.ID == historia)
                    .ToListAsync();
                return obj ?? new List<TDETALLECITAS>();
            }
        }


        public async Task<bool> ConsultarDoctoresConCitaOtraUnidad(string DOCTOR,DateTime FECHA, TimeSpan H1, TimeSpan H2, TDETALLECITAS? citaEditar = null)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.Where(x => x.FECHA==FECHA.Date && x.DOCTOR==DOCTOR).ToListAsync();
                if (obj.Any())
                {
                    var obj1 = obj.Where(x =>
                        (
                            H1 >= x.HORA && H1 <= (x.HORA?.Add(TimeSpan.FromMinutes(60 * Convert.ToInt32(x.DURACION ?? "1")+1))) ||
                            H2 >= x.HORA?.Add(TimeSpan.FromMinutes(1)) && H1 <= (x.HORA?.Add(TimeSpan.FromMinutes(60 * Convert.ToInt32(x.DURACION ?? "1") -1))) ||
                            x.HORA >= H1 && x.HORA <= H2.Subtract(TimeSpan.FromMinutes(1))
                        )
                    ).ToList();
                    if (citaEditar != null && citaEditar.FECHA != null && citaEditar.SILLA != null && citaEditar.HORA != null)
                    {
                        obj1 = obj1.Where(x => !(x.FECHA == citaEditar.FECHA.Value.Date && x.SILLA == citaEditar.SILLA && x.HORA == citaEditar.HORA)).ToList();
                    }
                    return obj1.Any();
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<List<TDETALLECITAS>> ConsultarPorFechaySilla(DateTime FECHA, int SILLA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.Where(x => x.FECHA == FECHA && x.SILLA == SILLA).ToListAsync();
                return obj == null ? new List<TDETALLECITAS>() : obj;
            }
        }

        public async Task<List<TDETALLECITAS>> ConsultarPorFechaSillaHora(DateTime FECHA, int SILLA, TimeSpan HORA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.Where(x => x.FECHA == FECHA && x.SILLA == SILLA && x.HORA == HORA).ToListAsync();
                return obj == null ? new List<TDETALLECITAS>() : obj;
            }
        }




        public async Task<bool> Editar(DateTime FECHA, int SILLA, TimeSpan HORA, TDETALLECITAS tdetallecitas)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TDETALLECITAS.FirstOrDefaultAsync(x => x.FECHA == FECHA.Date && x.HORA == HORA && x.SILLA == SILLA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        // Eliminamos la entidad existente
                        _dbcontext.TDETALLECITAS.Remove(obj);
                        await _dbcontext.SaveChangesAsync();

                        // Creamos una nueva entidad con la nueva clave primaria
                        _dbcontext.TDETALLECITAS.Add(tdetallecitas);
                        await _dbcontext.SaveChangesAsync();

                        return true;
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }
            }
        }
    }

    public interface ITDETALLECITASServicios
    {
        Task<TDETALLECITAS> Agregar(TDETALLECITAS tdetallecitas);
        Task<bool> Editar(DateTime FECHA, int SILLA, TimeSpan HORA, TDETALLECITAS tdetallecitas);
        Task<TDETALLECITAS> ConsultarPorId(DateTime FECHA, int SILLA, TimeSpan HORA);
        Task<TDETALLECITAS> ConsultarPorIdDetalleCitas(string ID);
        Task<List<TDETALLECITAS>> ConsultarPorFechaySilla(DateTime FECHA, int SILLA);
        Task<List<TDETALLECITAS>> ConsultarPorFechaSillaHora(DateTime FECHA, int SILLA, TimeSpan HORA);
        Task<List<TDETALLECITAS>> ConsultarPacienteConCitaRepetida(string NOMBRE, DateTime FECHA, string historia);
        Task<bool> Borrar(DateTime FECHA, int SILLA, TimeSpan HORA);
    }
}

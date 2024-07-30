using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TEVOLUCIONServicios : ITEVOLUCIONServicios
    {
        private readonly AppDbContext _dbcontext;
        public TEVOLUCIONServicios()
        {
            
        }


        public async Task<int> Agregar(TEVOLUCION tevolucion)
        {
            using (var _dbcontext = new AppDbContext())
            {
                try
                {
                    int id = 0;
                    using (var command = _dbcontext.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = "SELECT GEN_ID(TEVOLUCION_GEN, 1) FROM RDB$DATABASE";
                        _dbcontext.Database.OpenConnection();
                        var result = await command.ExecuteScalarAsync();
                        id = Convert.ToInt32((long)result);
                        _dbcontext.Database.CloseConnection();
                    }
                    tevolucion.IDEVOLUCION = id;
                    //var id = await _dbcontext.Database.ExecuteSqlRawAsync("SELECT GEN_ID(TEVOLUCION_GEN, 1) FROM RDB$DATABASE");
                    _dbcontext.TEVOLUCION.Add(tevolucion);
                    await _dbcontext.SaveChangesAsync();
                    await _dbcontext.Entry(tevolucion).ReloadAsync();
                    return tevolucion.IDEVOLUCION ?? 0;
                }
                catch (Exception ex)
                {
                    // Manejar la excepción aquí
                    Console.WriteLine(ex.Message);
                    return 0;
                }

            }

        }

        public async Task Borrar(int IDEVOLUCION)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.FirstOrDefaultAsync(x => x.IDEVOLUCION == IDEVOLUCION);
                _dbcontext.TEVOLUCION.Remove(obj);
            }
        }

        public async Task<TEVOLUCION> ConsultarPorIdEvolucion(int IDEVOLUCION)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.FirstOrDefaultAsync(x => x.IDEVOLUCION == IDEVOLUCION);
                return obj == null ? new TEVOLUCION() : obj;
            }
        }

        public async Task<TEVOLUCION> ConsultarUltimaEvolucion(int IDEVOLUSECUND)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.Where(x => x.IDEVOLUSECUND == IDEVOLUSECUND).OrderByDescending(x => x.FECHA).ThenByDescending(x => x.HORA).FirstOrDefaultAsync();
                return obj == null ? new TEVOLUCION() : obj;
            }
        }

        public async Task<List<TEVOLUCION>> ConsultarPorAnamnesis(int IDEVOLUSECUND)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION
                .Where(x => x.IDEVOLUSECUND == IDEVOLUSECUND)
                .OrderByDescending(x => x.FECHA)
                .ToListAsync();
                return obj == null ? new List<TEVOLUCION>() : obj;
            }
        }

        public async Task<TEVOLUCION> ConsultarPorAnamnesisFechaYHora(int IDEVOLUSECUND, DateTime FECHA, TimeSpan HORA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION
                .FirstOrDefaultAsync(x => x.IDEVOLUSECUND == IDEVOLUSECUND && x.FECHA == FECHA && x.HORA == HORA);

                return obj ?? new TEVOLUCION();
            }
        }




        public async Task<bool> Editar(int IDEVOLUCION, TEVOLUCION tevolucion)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TEVOLUCION.FirstOrDefaultAsync(x => x.IDEVOLUCION == IDEVOLUCION);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tevolucion);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITEVOLUCIONServicios
    {
        Task<int> Agregar(TEVOLUCION tevolucion);
        Task<bool> Editar(int IDEVOLUCION, TEVOLUCION tevolucion);
        Task<TEVOLUCION> ConsultarPorIdEvolucion(int IDEVOLUCION);
        Task<List<TEVOLUCION>> ConsultarPorAnamnesis(int IDEVOLUSECUND);
        Task<TEVOLUCION> ConsultarPorAnamnesisFechaYHora(int IDEVOLUSECUND, DateTime FECHA, TimeSpan HORA);
        Task<TEVOLUCION> ConsultarUltimaEvolucion(int IDEVOLUSECUND);
        Task Borrar(int IDEVOLUCION);
    }
}

using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class THORARIOSAGENDAServicios : ITHORARIOSAGENDAServicios
    {
        protected readonly AppDbContext _dbcontext;
        public THORARIOSAGENDAServicios()
        {
            
        }


        public async Task<int> Agregar(THORARIOSAGENDA thorariosagenda)
        {
            using (var _dbcontext = new AppDbContext())
            {
                _dbcontext.THORARIOSAGENDA.Add(thorariosagenda);
                await _dbcontext.SaveChangesAsync();
                return thorariosagenda.SILLA;
            }
        }

        public async Task Borrar(int SILLA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.THORARIOSAGENDA.FirstOrDefaultAsync(x => x.SILLA == SILLA);
                _dbcontext.THORARIOSAGENDA.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<THORARIOSAGENDA> ConsultarPorId(int SILLA)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.THORARIOSAGENDA.FirstOrDefaultAsync(x => x.SILLA == SILLA);
                return obj == null ? new THORARIOSAGENDA() : obj;
            }
        }

        /*public async Task<List<THORARIOSAGENDA>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.THORARIOSAGENDA.ToListAsync();
                return obj == null ? new List<THORARIOSAGENDA>() : obj;
            }
        }*/

        public async Task<List<THORARIOSAGENDA>> ConsultarTodos()
        {
            try
            {
                using (var _dbcontext = new AppDbContext())
                {
                    var obj = await _dbcontext.THORARIOSAGENDA.ToListAsync();
                    return obj ?? new List<THORARIOSAGENDA>(); // Usamos el operador null-coalescing (??) para simplificar
                }
            }
            catch (Exception ex)
            {
                // Aquí puedes hacer un log o manejar el error como desees
                // Por ejemplo:
                Console.WriteLine($"Error al consultar los horarios de agenda: {ex.Message}");
                return new List<THORARIOSAGENDA>(); // En caso de error, retornamos una lista vacía
            }
        }





        public async Task<bool> Editar(int SILLA, THORARIOSAGENDA thorariosagenda)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.THORARIOSAGENDA.FirstOrDefaultAsync(x => x.SILLA == SILLA);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(thorariosagenda);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }
    }

    public interface ITHORARIOSAGENDAServicios
    {
        Task<int> Agregar(THORARIOSAGENDA thorariosagenda);
        Task<bool> Editar(int SILLA, THORARIOSAGENDA thorariosagenda);
        Task<THORARIOSAGENDA> ConsultarPorId(int SILLA);
        Task<List<THORARIOSAGENDA>> ConsultarTodos();
        Task Borrar(int SILLA);
    }
}

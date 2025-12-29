using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using ServicioRydentLocal.LogicaDelNegocio.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
    public class TINFORMACIONREPORTESServicios : ITINFORMACIONREPORTESServicios
    {
        protected readonly AppDbContext _dbcontext;
        public TINFORMACIONREPORTESServicios()
        {
            
        }


        public async Task<int> Agregar(TINFORMACIONREPORTES tinformacionreportes)
        {
            using (var _dbcontext = new AppDbContext())
            {

                _dbcontext.TINFORMACIONREPORTES.Add(tinformacionreportes);
                await _dbcontext.SaveChangesAsync();
                return tinformacionreportes.ID;
            }
        }

        public async Task Borrar(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.FirstOrDefaultAsync(x => x.ID == ID);
                _dbcontext.TINFORMACIONREPORTES.Remove(obj);
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task<List<TINFORMACIONREPORTES>> ConsultarTodos()
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.ToListAsync();
                return obj == null ? new List<TINFORMACIONREPORTES>() : obj;
            }
        }
        

        public async Task<TINFORMACIONREPORTES> ConsultarPorId(int ID)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.FirstOrDefaultAsync(x => x.ID == ID);
                return obj == null ? new TINFORMACIONREPORTES() : obj;
            }
        }




        public async Task<string> ConsultarCodigoPrestador(int IDDOCTOR)
        {
            using (var _dbcontext = new AppDbContext())
            {
                int codDoctor = IDDOCTOR;
                string query = "SELECT i.CODIGO_PRESTADOR FROM TINFORMACIONREPORTES i INNER JOIN TDATOSDOCTORES D ON d.IDREPORTE = i.ID WHERE D.id=@p0";

                var codigoPrestador = await _dbcontext.TINFORMACIONREPORTES
                    .FromSqlRaw(query, codDoctor)
                    .Select(i => i.CODIGO_PRESTADOR)
                    .FirstOrDefaultAsync();
                return codigoPrestador;
            }
        }

    


        public async Task<bool> Editar(int ID, TINFORMACIONREPORTES tinformacionreportes)
        {
            using (var _dbcontext = new AppDbContext())
            {
                var obj = await _dbcontext.TINFORMACIONREPORTES.FirstOrDefaultAsync(x => x.ID == ID);
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    _dbcontext.Entry(obj).CurrentValues.SetValues(tinformacionreportes);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
        }

		/// <summary>
		/// Obtiene todos los doctores y el código de prestador principal
		/// con el que están asociados (según TDATOSDOCTORES.IDREPORTE).
		/// </summary>
		public async Task<List<ListadoItemModel>> ObtenerDoctoresConCodigoPrestadorAsync()
		{
			// Lista que se va a devolver
			var lista = new List<ListadoItemModel>();

			const string sql = @"
                SELECT
                    TD.NOMBRE,
                    TR.CODIGO_PRESTADOR_PPAL
                FROM
                    TDATOSDOCTORES TD
                    INNER JOIN TINFORMACIONREPORTES TR 
                        ON TD.IDREPORTE = TR.ID";

			// Creamos el DbContext (si lo estás inyectando por DI, usa el inyectado)
			using (var db = new AppDbContext())
			{
				// Tomamos la conexión que EF ya maneja
				var connection = db.Database.GetDbConnection();

				if (connection.State != ConnectionState.Open)
					await connection.OpenAsync();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = sql;
					command.CommandType = CommandType.Text;

					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							// Columnas según el SELECT:
							// 0 -> NOMBRE
							// 1 -> CODIGO_PRESTADOR_PPAL
							var nombreDoctor = reader.IsDBNull(0)
								? string.Empty
								: reader.GetString(0);

							var codigoPrestadorPPAL = reader.IsDBNull(1)
								? string.Empty
								: reader.GetString(1);

							// Mapear al modelo que usas en tus combos/listados
							// AJUSTA las propiedades según tu ListadoItemModel real
							var item = new ListadoItemModel
							{
								id = codigoPrestadorPPAL,
								nombre = nombreDoctor
							};

							lista.Add(item);
						}
					}
				}
			}

			return lista
	            .GroupBy(x => new { x.id, x.nombre })
	            .Select(g => g.First())
	            .OrderBy(x => x.nombre)
	            .ToList();
		}
	}

    public interface ITINFORMACIONREPORTESServicios
    {
        Task<int> Agregar(TINFORMACIONREPORTES tinformacionreportes);
        Task<bool> Editar(int ID, TINFORMACIONREPORTES tinformacionreportes);
        Task<List<TINFORMACIONREPORTES>> ConsultarTodos();
        Task<TINFORMACIONREPORTES> ConsultarPorId(int ID);
        Task<string> ConsultarCodigoPrestador(int IDDOCTOR);
        Task Borrar(int ID);
        Task<List<ListadoItemModel>> ObtenerDoctoresConCodigoPrestadorAsync();
    }
}

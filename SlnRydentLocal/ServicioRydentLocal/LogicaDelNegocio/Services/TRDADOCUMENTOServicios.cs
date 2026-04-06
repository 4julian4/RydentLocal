using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public class TRDADOCUMENTOServicios : ITRDADOCUMENTOServicios
	{
		public TRDADOCUMENTOServicios()
		{
		}

		public async Task<int> Agregar(TRDA_DOCUMENTO documento)
		{
			using (var _dbcontext = new AppDbContext())
			{
				try
				{
					_dbcontext.TRDA_DOCUMENTO.Add(documento);
					await _dbcontext.SaveChangesAsync();
					return documento.ID;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return 0;
				}
			}
		}

		public async Task<bool> Editar(int id, TRDA_DOCUMENTO documento)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj == null)
					return false;

				_dbcontext.Entry(obj).CurrentValues.SetValues(documento);
				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}

		public async Task<TRDA_DOCUMENTO> ConsultarPorId(int id)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.FirstOrDefaultAsync(x => x.ID == id);

				return obj ?? new TRDA_DOCUMENTO();
			}
		}

		public async Task<TRDA_DOCUMENTO> ConsultarUltimoPorAnamnesis(int idAnamnesis)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.Where(x => x.IDANAMNESIS == idAnamnesis)
					.OrderByDescending(x => x.FECHA_GENERACION)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();

				return obj ?? new TRDA_DOCUMENTO();
			}
		}

		public async Task<TRDA_DOCUMENTO> ConsultarPorAnamnesisYEvolucion(int idAnamnesis, int? idEvolucion, string? tipoDocumento)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.IDEVOLUCION == idEvolucion &&
						x.TIPO_DOCUMENTO == tipoDocumento)
					.OrderByDescending(x => x.FECHA_GENERACION)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();

				return obj ?? new TRDA_DOCUMENTO();
			}
		}

		public async Task<TRDA_DOCUMENTO> ConsultarConsultaExistente(
			int idAnamnesis,
			DateTime? fechaAtencion,
			TimeSpan? horaAtencion,
			string? tipoDocumento)
		{
			using (var _dbcontext = new AppDbContext())
			{
				if (!fechaAtencion.HasValue)
					return new TRDA_DOCUMENTO();

				var fechaSolo = fechaAtencion.Value.Date;
				var fechaFin = fechaSolo.AddDays(1);

				var queryBase = _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.TIPO_DOCUMENTO == tipoDocumento &&
						x.FECHA_ATENCION >= fechaSolo &&
						x.FECHA_ATENCION < fechaFin);

				TRDA_DOCUMENTO? obj = null;

				if (horaAtencion.HasValue)
				{
					obj = await queryBase
						.Where(x => x.HORA_ATENCION == horaAtencion.Value)
						.OrderByDescending(x => x.FECHA_GENERACION)
						.ThenByDescending(x => x.ID)
						.FirstOrDefaultAsync();

					if (obj == null)
					{
						obj = await queryBase
							.Where(x => x.HORA_ATENCION == null)
							.OrderByDescending(x => x.FECHA_GENERACION)
							.ThenByDescending(x => x.ID)
							.FirstOrDefaultAsync();
					}
				}
				else
				{
					obj = await queryBase
						.Where(x => x.HORA_ATENCION == null)
						.OrderByDescending(x => x.FECHA_GENERACION)
						.ThenByDescending(x => x.ID)
						.FirstOrDefaultAsync();

					if (obj == null)
					{
						obj = await queryBase
							.OrderByDescending(x => x.FECHA_GENERACION)
							.ThenByDescending(x => x.ID)
							.FirstOrDefaultAsync();
					}
				}

				return obj ?? new TRDA_DOCUMENTO();
			}
		}

		public async Task<TRDA_DOCUMENTO> ConsultarPacienteExistente(int idAnamnesis)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.Where(x =>
						x.IDANAMNESIS == idAnamnesis &&
						x.TIPO_DOCUMENTO == "RDA_PACIENTE_INTERNO")
					.OrderByDescending(x => x.FECHA_GENERACION)
					.ThenByDescending(x => x.ID)
					.FirstOrDefaultAsync();

				return obj ?? new TRDA_DOCUMENTO();
			}
		}

		public async Task<bool> GuardarRequestApi(int id, string? requestApi)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj == null)
					return false;

				obj.REQUEST_APIstr = requestApi;
				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}

		public async Task<bool> MarcarEnvio(
			int id,
			string estado,
			string? mensajeError,
			int intentos,
			DateTime? fechaEnvio,
			int? codigoHttp,
			string? respuestaApi,
			string? requestApi = null)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj == null)
					return false;

				obj.ESTADO = estado;
				obj.MENSAJE_ERROR = mensajeError;
				obj.INTENTOS = intentos;
				obj.FECHA_ENVIO = fechaEnvio;
				obj.CODIGO_HTTP = codigoHttp;
				obj.RESPUESTA_APIstr = respuestaApi;

				if (requestApi != null)
					obj.REQUEST_APIstr = requestApi;

				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}

		public async Task<bool> MarcarNoReintentar(
			int id,
			string? mensajeError,
			int? codigoHttp,
			string? respuestaApi = null)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj == null)
					return false;

				obj.ESTADO = "NO_REINTENTAR";
				obj.MENSAJE_ERROR = mensajeError;
				obj.CODIGO_HTTP = codigoHttp;
				obj.RESPUESTA_APIstr = respuestaApi;

				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}

		public async Task<bool> ActualizarEstado(
			int id,
			string estado,
			string? mensajeError = null,
			int? intentos = null)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var obj = await _dbcontext.TRDA_DOCUMENTO.FirstOrDefaultAsync(x => x.ID == id);
				if (obj == null)
					return false;

				obj.ESTADO = estado;

				if (mensajeError != null)
					obj.MENSAJE_ERROR = mensajeError;

				if (intentos.HasValue)
					obj.INTENTOS = intentos.Value;

				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}

		public async Task<TRDA_DOCUMENTO[]> ConsultarPendientesEnvio(int maxRegistros = 50)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var estados = new[] { "GENERADO", "ERROR_ENVIO" };

				return await _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.Where(x => x.ESTADO != null && estados.Contains(x.ESTADO))
					.OrderBy(x => x.FECHA_GENERACION)
					.ThenBy(x => x.ID)
					.Take(maxRegistros <= 0 ? 50 : maxRegistros)
					.ToArrayAsync();
			}
		}

		public async Task<bool> EliminarConsultaExistente(
			int idAnamnesis,
			DateTime fechaAtencion,
			TimeSpan? horaAtencion,
			string tipoDocumento)
		{
			using (var _dbcontext = new AppDbContext())
			{
				var fechaSolo = fechaAtencion.Date;
				var fechaFin = fechaSolo.AddDays(1);

				var queryBase = _dbcontext.TRDA_DOCUMENTO.Where(x =>
					x.IDANAMNESIS == idAnamnesis &&
					x.TIPO_DOCUMENTO == tipoDocumento &&
					x.FECHA_ATENCION >= fechaSolo &&
					x.FECHA_ATENCION < fechaFin);

				List<TRDA_DOCUMENTO> rows;

				if (horaAtencion.HasValue)
				{
					rows = await queryBase
						.Where(x => x.HORA_ATENCION == horaAtencion.Value)
						.ToListAsync();

					if (!rows.Any())
					{
						rows = await queryBase
							.Where(x => x.HORA_ATENCION == null)
							.ToListAsync();
					}
				}
				else
				{
					rows = await queryBase
						.Where(x => x.HORA_ATENCION == null)
						.ToListAsync();

					if (!rows.Any())
					{
						rows = await queryBase.ToListAsync();
					}
				}

				if (!rows.Any())
					return false;

				_dbcontext.TRDA_DOCUMENTO.RemoveRange(rows);
				await _dbcontext.SaveChangesAsync();
				return true;
			}
		}
	}

	public interface ITRDADOCUMENTOServicios
	{
		Task<int> Agregar(TRDA_DOCUMENTO documento);
		Task<bool> Editar(int id, TRDA_DOCUMENTO documento);
		Task<TRDA_DOCUMENTO> ConsultarPorId(int id);
		Task<TRDA_DOCUMENTO> ConsultarUltimoPorAnamnesis(int idAnamnesis);
		Task<TRDA_DOCUMENTO> ConsultarPorAnamnesisYEvolucion(int idAnamnesis, int? idEvolucion, string? tipoDocumento);
		Task<TRDA_DOCUMENTO> ConsultarConsultaExistente(int idAnamnesis, DateTime? fechaAtencion, TimeSpan? horaAtencion, string? tipoDocumento);
		Task<TRDA_DOCUMENTO> ConsultarPacienteExistente(int idAnamnesis);
		Task<bool> GuardarRequestApi(int id, string? requestApi);
		Task<bool> MarcarEnvio(int id, string estado, string? mensajeError, int intentos, DateTime? fechaEnvio, int? codigoHttp, string? respuestaApi, string? requestApi = null);
		Task<bool> MarcarNoReintentar(int id, string? mensajeError, int? codigoHttp, string? respuestaApi = null);
		Task<bool> ActualizarEstado(int id, string estado, string? mensajeError = null, int? intentos = null);
		Task<TRDA_DOCUMENTO[]> ConsultarPendientesEnvio(int maxRegistros = 50);
		Task<bool> EliminarConsultaExistente(int idAnamnesis, DateTime fechaAtencion, TimeSpan? horaAtencion, string tipoDocumento);
	}
}
using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaControlConsultaService : IRdaControlConsultaService
	{
		public async Task<RdaControlRespuesta> ConsultarAsync(RdaControlFiltro filtro)
		{
			filtro ??= new RdaControlFiltro();

			var max = filtro.MaxRegistros <= 0 ? 100 : filtro.MaxRegistros;

			using (var _dbcontext = new AppDbContext())
			{
				var query =
					from r in _dbcontext.TRDA_DOCUMENTO.AsNoTracking()
					join a in _dbcontext.TANAMNESIS.AsNoTracking()
						on r.IDANAMNESIS equals a.IDANAMNESIS into ga
					from a in ga.DefaultIfEmpty()

					let facturaDx = _dbcontext.T_RIPS_DX
						.AsNoTracking()
						.Where(x => x.IDANAMNESIS == r.IDANAMNESIS)
						.OrderByDescending(x => x.FECHACONSULTA)
						.ThenByDescending(x => x.ID)
						.Select(x => x.FACTURA)
						.FirstOrDefault()

					let doctorDxId = _dbcontext.T_RIPS_DX
						.AsNoTracking()
						.Where(x => x.IDANAMNESIS == r.IDANAMNESIS)
						.OrderByDescending(x => x.FECHACONSULTA)
						.ThenByDescending(x => x.ID)
						.Select(x => x.IDDOCTOR)
						.FirstOrDefault()

					let doctorNombre = _dbcontext.TDATOSDOCTORES
						.AsNoTracking()
						.Where(d => d.ID == doctorDxId)
						.Select(d => d.NOMBRE)
						.FirstOrDefault()

					select new RdaControlItem
					{
						ID = r.ID,
						IDANAMNESIS = r.IDANAMNESIS,
						IDEVOLUCION = r.IDEVOLUCION,
						FECHA_ATENCION = r.FECHA_ATENCION,
						TIPO_DOCUMENTO = r.TIPO_DOCUMENTO,
						ESTADO = r.ESTADO,
						FECHA_GENERACION = r.FECHA_GENERACION,
						FECHA_ENVIO = r.FECHA_ENVIO,
						INTENTOS = r.INTENTOS,
						CODIGO_HTTP = r.CODIGO_HTTP,
						MENSAJE_ERROR = r.MENSAJE_ERROR,
						NOMBRE_PACIENTE = a != null ? a.NOMBRE_PACIENTE : null,
						DOCUMENTO_PACIENTE = a != null ? a.CEDULA_NUMERO : null,
						NUMERO_HISTORIA = a != null ? a.IDANAMNESIS_TEXTO : null,
						FACTURA = facturaDx,
						DOCTOR = doctorNombre
					};

				if (filtro.FechaDesde.HasValue)
				{
					var desde = filtro.FechaDesde.Value.Date;
					query = query.Where(x =>
						x.FECHA_GENERACION.HasValue &&
						x.FECHA_GENERACION.Value.Date >= desde);
				}

				if (filtro.FechaHasta.HasValue)
				{
					var hasta = filtro.FechaHasta.Value.Date;
					query = query.Where(x =>
						x.FECHA_GENERACION.HasValue &&
						x.FECHA_GENERACION.Value.Date <= hasta);
				}

				if (!string.IsNullOrWhiteSpace(filtro.Estado))
				{
					var estado = filtro.Estado.Trim().ToUpper();
					query = query.Where(x => x.ESTADO != null && x.ESTADO.ToUpper() == estado);
				}

				if (!string.IsNullOrWhiteSpace(filtro.Texto))
				{
					var txt = filtro.Texto.Trim().ToUpper();

					query = query.Where(x =>
						(x.NOMBRE_PACIENTE ?? "").ToUpper().Contains(txt) ||
						(x.DOCUMENTO_PACIENTE ?? "").ToUpper().Contains(txt) ||
						(x.NUMERO_HISTORIA ?? "").ToUpper().Contains(txt) ||
						(x.TIPO_DOCUMENTO ?? "").ToUpper().Contains(txt) ||
						(x.ESTADO ?? "").ToUpper().Contains(txt) ||
						(x.MENSAJE_ERROR ?? "").ToUpper().Contains(txt) ||
						(x.FACTURA ?? "").ToUpper().Contains(txt) ||
						(x.DOCTOR ?? "").ToUpper().Contains(txt));
				}

				var items = await query
					.OrderByDescending(x => x.FECHA_GENERACION)
					.ThenByDescending(x => x.ID)
					.Take(max)
					.ToListAsync();

				return new RdaControlRespuesta
				{
					Items = items
				};
			}
		}
	}

	public interface IRdaControlConsultaService
	{
		Task<RdaControlRespuesta> ConsultarAsync(RdaControlFiltro filtro);
	}
}
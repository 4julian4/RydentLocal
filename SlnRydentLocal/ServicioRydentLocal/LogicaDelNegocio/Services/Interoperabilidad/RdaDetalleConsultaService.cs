using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaDetalleConsultaService : IRdaDetalleConsultaService
	{
		public async Task<RdaDetalleRespuesta> ConsultarPorIdAsync(int idRda)
		{
			if (idRda <= 0)
			{
				return new RdaDetalleRespuesta
				{
					Ok = false,
					Mensaje = "IdRda inválido."
				};
			}

			using (var _dbcontext = new AppDbContext())
			{
				var item =
					await (
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

						where r.ID == idRda
						select new RdaDetalleRespuesta
						{
							Ok = true,
							Mensaje = null,

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
							DOCTOR = doctorNombre,
							FACTURA = facturaDx,

							JSON_RDA = r.JSON_RDAstr,
							JSON_SNAPSHOT = r.JSON_SNAPSHOTstr,
							REQUEST_API = r.REQUEST_APIstr,
							RESPUESTA_API = r.RESPUESTA_APIstr
						})
						.FirstOrDefaultAsync();

				if (item == null)
				{
					return new RdaDetalleRespuesta
					{
						Ok = false,
						Mensaje = "No existe el documento RDA."
					};
				}

				return item;
			}
		}
	}

	public interface IRdaDetalleConsultaService
	{
		Task<RdaDetalleRespuesta> ConsultarPorIdAsync(int idRda);
	}
}
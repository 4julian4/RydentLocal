using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaHistorialConsultaService : IRdaHistorialConsultaService
	{
		public async Task<RdaHistorialRespuesta> ConsultarPorDocumentoAsync(int idRda)
		{
			if (idRda <= 0)
			{
				return new RdaHistorialRespuesta
				{
					Ok = false,
					Mensaje = "IdRda inválido."
				};
			}

			using (var _dbcontext = new AppDbContext())
			{
				var doc = await _dbcontext.TRDA_DOCUMENTO
					.AsNoTracking()
					.FirstOrDefaultAsync(x => x.ID == idRda);

				if (doc == null)
				{
					return new RdaHistorialRespuesta
					{
						Ok = false,
						Mensaje = "No existe el documento RDA."
					};
				}

				var patronRda = $"IdRda={idRda}";
				var patronAnamnesis = $"IdAnamnesis={doc.IDANAMNESIS}";

				var items = await _dbcontext.THISTORIAL
					.AsNoTracking()
					.Where(x =>
						(x.DESCRIPCION != null && x.DESCRIPCION.Contains(patronRda)) ||
						(x.DESCRIPCION != null && x.DESCRIPCION.Contains(patronAnamnesis)) ||
						(x.DESCRIPCION != null && x.DESCRIPCION.Contains("RDA ")))
					.OrderByDescending(x => x.FECHA)
					.ThenByDescending(x => x.HORA)
					.Select(x => new RdaHistorialItem
					{
						Fecha = x.FECHA,
						Hora = x.HORA.ToString(@"hh\:mm\:ss"),
						Usuario = x.USUARIO,
						Descripcion = x.DESCRIPCION,
						Tipo = ClasificarTipo(x.DESCRIPCION)
					})
					.Take(200)
					.ToListAsync();

				// filtro final en memoria para no traer historias de otros pacientes/RDA
				items = items
					.Where(x =>
						(x.Descripcion ?? "").Contains(patronRda) ||
						(x.Descripcion ?? "").Contains(patronAnamnesis) ||
						EsMensajeGeneralRdaRelacionado(x.Descripcion, doc.IDANAMNESIS))
					.ToList();

				return new RdaHistorialRespuesta
				{
					Ok = true,
					Mensaje = null,
					Items = items
				};
			}
		}

		private static bool EsMensajeGeneralRdaRelacionado(string? descripcion, int idAnamnesis)
		{
			if (string.IsNullOrWhiteSpace(descripcion))
				return false;

			var txt = descripcion.Trim();

			if (!txt.Contains("RDA"))
				return false;

			return txt.Contains($"IdAnamnesis={idAnamnesis}");
		}

		private static string ClasificarTipo(string? descripcion)
		{
			if (string.IsNullOrWhiteSpace(descripcion))
				return "INFO";

			var txt = descripcion.ToUpperInvariant();

			if (txt.Contains("TIPO=ERROR"))
				return "ERROR";

			if (txt.Contains("TIPO=ENVIO"))
				return "ENVIO";

			if (txt.Contains("TIPO=FHIR"))
				return "FHIR";

			if (txt.Contains("TIPO=PERSISTENCIA"))
				return "PERSISTENCIA";

			if (txt.Contains("TIPO=CONTEXTO"))
				return "CONTEXTO";

			if (txt.Contains("TIPO=MANUAL"))
				return "MANUAL";

			if (txt.Contains("TIPO=MASIVO"))
				return "MASIVO";

			if (txt.Contains("ERROR"))
				return "ERROR";

			return "INFO";
		}
	}

	public interface IRdaHistorialConsultaService
	{
		Task<RdaHistorialRespuesta> ConsultarPorDocumentoAsync(int idRda);
	}
}
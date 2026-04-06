using Microsoft.EntityFrameworkCore;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaDiagnosticoSourceService : IRdaDiagnosticoSourceService
	{
		public RdaDiagnosticoSourceService()
		{
		}

		public async Task<RdaDiagnosticoSource> ConsultarPorAnamnesis(int idAnamnesis)
		{
			if (idAnamnesis <= 0)
				return new RdaDiagnosticoSource();

			using (var db = new AppDbContext())
			{
				var obj = await db.TDIAGNOSTICO
					.AsNoTracking()
					.Where(x => x.IDDIAGNOS == idAnamnesis)
					.OrderByDescending(x => x.FECHA)
					.FirstOrDefaultAsync();

				if (obj == null)
					return new RdaDiagnosticoSource();

				var catalogoLookup = new RdaCatalogoLookupService();

				var diagnostico1 = !string.IsNullOrWhiteSpace(obj.DIAGNOSTICO1) ? obj.DIAGNOSTICO1.Trim() : null;
				var diagnostico2 = !string.IsNullOrWhiteSpace(obj.DIAGNOSTICO2) ? obj.DIAGNOSTICO2.Trim() : null;
				var diagnostico3 = !string.IsNullOrWhiteSpace(obj.DIAGNOSTICO3) ? obj.DIAGNOSTICO3.Trim() : null;
				var diagnostico4 = !string.IsNullOrWhiteSpace(obj.DIAGNOSTICO4) ? obj.DIAGNOSTICO4.Trim() : null;
				var codigoConsulta = !string.IsNullOrWhiteSpace(obj.CODIGO_CONSULTA) ? obj.CODIGO_CONSULTA.Trim() : null;

				return new RdaDiagnosticoSource
				{
					IdAnamnesis = idAnamnesis,
					NumeroAutorizacion = obj.NUMERO_AUTORIZACION,
					CodigoConsulta = codigoConsulta,
					NombreCodigoConsulta = await catalogoLookup.ConsultarNombreConsultaAsync(codigoConsulta),
					CodigoFinalidad = obj.CODIGO_FINALIDAD,
					CodigoCausa = obj.CODIGO_CAUSA,

					Diagnostico1 = diagnostico1,
					NombreDiagnostico1 = await catalogoLookup.ConsultarNombreDiagnosticoAsync(diagnostico1),

					Diagnostico2 = diagnostico2,
					NombreDiagnostico2 = await catalogoLookup.ConsultarNombreDiagnosticoAsync(diagnostico2),

					Diagnostico3 = diagnostico3,
					NombreDiagnostico3 = await catalogoLookup.ConsultarNombreDiagnosticoAsync(diagnostico3),

					Diagnostico4 = diagnostico4,
					NombreDiagnostico4 = await catalogoLookup.ConsultarNombreDiagnosticoAsync(diagnostico4),

					CodigoTipoDiagnostico = obj.CODIGO_TIPO_DIAGNOSTICO,
					Pronostico = obj.PRONOSTICO,
					Observaciones = obj.OBSERVACIONESstr
				};
			}
		}
	}

	public interface IRdaDiagnosticoSourceService
	{
		Task<RdaDiagnosticoSource> ConsultarPorAnamnesis(int idAnamnesis);
	}
}
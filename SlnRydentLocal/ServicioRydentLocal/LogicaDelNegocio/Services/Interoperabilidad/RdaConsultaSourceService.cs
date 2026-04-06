using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaConsultaSourceService : IRdaConsultaSourceService
	{
		public RdaConsultaSourceService()
		{
		}

		public async Task<RdaConsultaSource> ConstruirDesdeContexto(RdaGeneracionContext context)
		{
			if (context == null || context.IdAnamnesis <= 0)
				return new RdaConsultaSource();

			var encounterService = new RdaEncounterSourceService();
			var antecedentesService = new RdaAntecedentesSourceService();
			var diagnosticoService = new RdaDiagnosticoSourceService();
			var procedimientoService = new RdaProcedimientoSourceService();

			var encounter = await encounterService.ConstruirDesdeContexto(context);
			var antecedentes = await antecedentesService.ConsultarPorAnamnesis(context.IdAnamnesis);
			var diagnostico = await diagnosticoService.ConsultarPorAnamnesis(context.IdAnamnesis);
			var procedimiento = await procedimientoService.ConsultarPorAnamnesis(context.IdAnamnesis);

			return new RdaConsultaSource
			{
				Encounter = encounter,
				Antecedentes = antecedentes,
				Diagnostico = diagnostico,
				Procedimiento = procedimiento
			};
		}
	}

	public interface IRdaConsultaSourceService
	{
		Task<RdaConsultaSource> ConstruirDesdeContexto(RdaGeneracionContext context);
	}
}
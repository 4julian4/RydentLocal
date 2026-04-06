using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaDocumentoCompletoSourceService : IRdaDocumentoCompletoSourceService
	{
		public RdaDocumentoCompletoSourceService()
		{
		}

		public async Task<RdaDocumentoCompletoSource> ConstruirDesdeContexto(RdaGeneracionContext context)
		{
			if (context == null || context.IdAnamnesis <= 0)
				return new RdaDocumentoCompletoSource();

			var consultaSourceService = new RdaConsultaSourceService();
			var prestadorSourceService = new RdaPrestadorSourceService();

			var consulta = await consultaSourceService.ConstruirDesdeContexto(context);
			var prestador = await prestadorSourceService.ConsultarPorDoctor(context.IdDoctor);

			return new RdaDocumentoCompletoSource
			{
				Consulta = consulta,
				Prestador = prestador
			};
		}
	}

	public interface IRdaDocumentoCompletoSourceService
	{
		Task<RdaDocumentoCompletoSource> ConstruirDesdeContexto(RdaGeneracionContext context);
	}
}
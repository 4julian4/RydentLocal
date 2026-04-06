using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaDocumentoInternoService : IRdaDocumentoInternoService
	{
		public RdaDocumentoInternoService()
		{
		}

		public async Task<RdaDocumentoInterno> ConstruirDesdeContexto(RdaGeneracionContext context)
		{
			if (context == null || context.IdAnamnesis <= 0)
				return new RdaDocumentoInterno();

			var documentoCompletoService = new RdaDocumentoCompletoSourceService();
			var documento = await documentoCompletoService.ConstruirDesdeContexto(context);

			return new RdaDocumentoInterno
			{
				TipoDocumento = "RDA_CONSULTA_INTERNO",
				FechaGeneracion = DateTime.Now,
				Documento = documento
			};
		}
	}

	public interface IRdaDocumentoInternoService
	{
		Task<RdaDocumentoInterno> ConstruirDesdeContexto(RdaGeneracionContext context);
	}
}
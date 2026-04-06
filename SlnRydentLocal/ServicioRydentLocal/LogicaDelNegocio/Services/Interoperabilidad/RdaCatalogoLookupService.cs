using ServicioRydentLocal.LogicaDelNegocio.Services;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaCatalogoLookupService : IRdaCatalogoLookupService
	{
		public async Task<string?> ConsultarNombreDiagnosticoAsync(string? codigoDiagnostico)
		{
			if (string.IsNullOrWhiteSpace(codigoDiagnostico))
				return null;

			var servicio = new TCODIGOS_CONSLUTAS_TEMPServicios();
			return await servicio.ConsultarNombrePorCodigo(codigoDiagnostico);
		}

		public async Task<string?> ConsultarNombreConsultaAsync(string? codigoConsulta)
		{
			if (string.IsNullOrWhiteSpace(codigoConsulta))
				return null;

			var servicio = new TCODIGOS_PROCEDIMIENTOSServicios();
			return await servicio.ConsultarNombrePorCodigo(codigoConsulta);
		}

		
		public async Task<string?> ConsultarNombreProcedimientoAsync(string? codigoProcedimiento)
		{
			if (string.IsNullOrWhiteSpace(codigoProcedimiento))
				return null;

			var servicio = new TCODIGOS_PROCEDIMIENTOSServicios();
			return await servicio.ConsultarNombrePorCodigo(codigoProcedimiento);
		}

		public async Task<string?> ConsultarNombreCiudadAsync(string? codigoDepartamento, string? codigoCiudad)
		{
			if (string.IsNullOrWhiteSpace(codigoDepartamento) || string.IsNullOrWhiteSpace(codigoCiudad))
				return null;

			var servicio = new TCODIGOS_CIUDADServicios();
			return await servicio.ConsultarNombrePorCodigo(codigoDepartamento, codigoCiudad);
		}

		public async Task<string?> ConsultarNombreDepartamentoAsync(string? codigoDepartamento)
		{
			if (string.IsNullOrWhiteSpace(codigoDepartamento))
				return null;

			var servicio = new TCODIGOS_DEPARTAMENTOServicios();
			return await servicio.ConsultarNombrePorCodigo(codigoDepartamento);
		}

		public async Task<string?> ConsultarNombreEpsAsync(string? codigoEps)
		{
			if (string.IsNullOrWhiteSpace(codigoEps))
				return null;

			var servicio = new TCODIGOS_EPSServicios();
			return await servicio.ConsultarNombrePorCodigo(codigoEps);
		}
	}

	public interface IRdaCatalogoLookupService
	{
		Task<string?> ConsultarNombreDiagnosticoAsync(string? codigoDiagnostico);
		Task<string?> ConsultarNombreConsultaAsync(string? codigoConsulta);
		Task<string?> ConsultarNombreProcedimientoAsync(string? codigoProcedimiento);
		Task<string?> ConsultarNombreCiudadAsync(string? codigoDepartamento, string? codigoCiudad);
		Task<string?> ConsultarNombreDepartamentoAsync(string? codigoDepartamento);
		Task<string?> ConsultarNombreEpsAsync(string? codigoEps);
	}
}
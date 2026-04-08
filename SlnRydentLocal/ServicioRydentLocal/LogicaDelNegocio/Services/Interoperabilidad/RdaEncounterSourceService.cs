using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaEncounterSourceService : IRdaEncounterSourceService
	{
		public RdaEncounterSourceService()
		{
		}

		public async Task<RdaEncounterSource> ConstruirDesdeContexto(RdaGeneracionContext context)
		{
			if (context == null || context.IdAnamnesis <= 0)
				return new RdaEncounterSource();

			var objTAnamnesisServicios = new TANAMNESISServicios();
			var evolucionService = new RdaEvolucionSourceService();
			var catalogoLookup = new RdaCatalogoLookupService();

			var paciente = await objTAnamnesisServicios.ConsultarPorId(context.IdAnamnesis);
			var evolucion = context.FechaConsulta.HasValue
				? await evolucionService.ConsultarPorPacienteYFecha(context.IdAnamnesis, context.FechaConsulta.Value)
				: new RdaEvolucionSource();

			var codigoCiudad = !string.IsNullOrWhiteSpace(paciente?.CODIGO_CIUDAD)
				? paciente.CODIGO_CIUDAD.Trim()
				: null;

			var codigoDepartamento = !string.IsNullOrWhiteSpace(paciente?.CODIGO_DEPARTAMENTO)
				? paciente.CODIGO_DEPARTAMENTO.Trim()
				: null;

			var codigoEps = !string.IsNullOrWhiteSpace(paciente?.CODIGO_EPS)
				? paciente.CODIGO_EPS.Trim()
				: null;

			var codigoConsulta = !string.IsNullOrWhiteSpace(context.CodigoConsulta)
				? context.CodigoConsulta.Trim()
				: null;

			var codigoDiagnosticoPrincipal = !string.IsNullOrWhiteSpace(context.CodigoDiagnosticoPrincipal)
				? context.CodigoDiagnosticoPrincipal.Trim()
				: null;

			var codigoProcedimiento = !string.IsNullOrWhiteSpace(context.CodigoProcedimiento)
				? context.CodigoProcedimiento.Trim()
				: null;

			var nombreCiudad = await catalogoLookup.ConsultarNombreCiudadAsync(codigoDepartamento, codigoCiudad);
			var nombreDepartamento = await catalogoLookup.ConsultarNombreDepartamentoAsync(codigoDepartamento);
			var nombreEps = await catalogoLookup.ConsultarNombreEpsAsync(codigoEps);
			var nombreConsulta = await catalogoLookup.ConsultarNombreConsultaAsync(codigoConsulta);
			var nombreDiagnosticoPrincipal = await catalogoLookup.ConsultarNombreDiagnosticoAsync(codigoDiagnosticoPrincipal);
			var nombreProcedimiento = await catalogoLookup.ConsultarNombreProcedimientoAsync(codigoProcedimiento);

			return new RdaEncounterSource
			{
				IdAnamnesis = context.IdAnamnesis,
				FechaConsulta = context.FechaConsulta,
				HoraConsulta = context.HoraConsulta,
				CodigoConsulta = codigoConsulta,
				NombreConsulta = nombreConsulta,
				CodigoDiagnosticoPrincipal = codigoDiagnosticoPrincipal,
				NombreDiagnosticoPrincipal = nombreDiagnosticoPrincipal,
				CodigoProcedimiento = codigoProcedimiento,
				NombreProcedimiento = nombreProcedimiento,
				Factura = context.Factura,

				IdEvolucion = evolucion.IdEvolucion,
				NotaEvolucion = evolucion.Evolucion,

				TipoDocumento = !string.IsNullOrWhiteSpace(paciente?.DOCUMENTO_IDENTIDAD)
					? paciente.DOCUMENTO_IDENTIDAD.Trim()
					: null,

				NumeroDocumento = !string.IsNullOrWhiteSpace(paciente?.CEDULA_NUMERO)
					? paciente.CEDULA_NUMERO.Trim()
					: paciente?.IDANAMNESIS_TEXTO,

				Nombres = paciente?.NOMBRES,
				Apellidos = paciente?.APELLIDOS,

				FechaNacimiento = RdaPacienteFechaHelper.ConstruirFechaNacimiento(
					paciente?.FECHAN_DIA,
					paciente?.FECHAN_MES,
					paciente?.FECHAN_ANO),

				Sexo = paciente?.SEXO,
				Direccion = paciente?.DIRECCION_PACIENTE,
				Telefono = paciente?.TELF_P,
				Celular = paciente?.CELULAR_P,

				CodigoCiudad = codigoCiudad,
				CodigoDepartamento = codigoDepartamento,
				NombreCiudad = nombreCiudad,
				NombreDepartamento = nombreDepartamento,
				ZonaRecidencial = !string.IsNullOrWhiteSpace(paciente?.ZONA_RECIDENCIAL)
					? paciente.ZONA_RECIDENCIAL.Trim()
					: null,

				CodigoEps = codigoEps,
				NombreEps = nombreEps,

				NroAfiliacion = paciente?.NRO_AFILIACION,
				TipoUsuario = paciente?.PARENTESCO,

				Doctor = !string.IsNullOrWhiteSpace(evolucion.Doctor)
					? evolucion.Doctor.Trim()
					: null
			};
		}
	}

	public interface IRdaEncounterSourceService
	{
		Task<RdaEncounterSource> ConstruirDesdeContexto(RdaGeneracionContext context);
	}
}
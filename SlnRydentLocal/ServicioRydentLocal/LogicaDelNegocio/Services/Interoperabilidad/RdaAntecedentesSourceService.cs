using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaAntecedentesSourceService : IRdaAntecedentesSourceService
	{
		public RdaAntecedentesSourceService()
		{
		}

		public async Task<RdaAntecedentesSource> ConsultarPorAnamnesis(int idAnamnesis)
		{
			if (idAnamnesis <= 0)
				return new RdaAntecedentesSource();

			var objTAnamnesisServicios = new TANAMNESISServicios();
			var paciente = await objTAnamnesisServicios.ConsultarPorId(idAnamnesis);

			if (paciente == null)
				return new RdaAntecedentesSource();

			return new RdaAntecedentesSource
			{
				IdAnamnesis = idAnamnesis,

				MotivoConsulta = Limpiar(
					FirstNonEmpty(
						paciente.MOTIVO_DE_CONSULTA,
						paciente.IMPORTANTE
					)
				),

				EnfermedadActual = Limpiar(
					FirstNonEmpty(
						paciente.ENFERMEDAD_ACTUAL
					)
				),

				AlergiasTexto = Limpiar(
					FirstNonEmpty(
						paciente.REACC_ALERGIC_CUALES,
						paciente.REACC_ALERGIC_CUALES_S
					)
				),

				AlergiaBandera = Limpiar(
					FirstNonEmpty(
						paciente.ALERGIA,
						paciente.REACC_ALERGIC_CUALES_S
					)
				),

				MedicamentosActualesTexto = Limpiar(
					FirstNonEmpty(
						paciente.RECIBE_ALGUN_MEDIC_CUAL,
						paciente.RECIBE_ALGUN_MEDIC_CUAL_S
					)
				),

				EnfermedadesPreviasTexto = Limpiar(
					FirstNonEmpty(
						paciente.PADC_ENFERM_CUALES,
						paciente.PADC_ENFERM_CUALES_S,
						paciente.ENFERMEDADESHERE,
						paciente.ENFERMEDADESHERE_S
					)
				),

				CirugiasTexto = Limpiar(
					FirstNonEmpty(
						paciente.CIRUGIAS,
						paciente.CIRUGIAS_S,
						paciente.CIRUGIA_ORAL,
						paciente.CIRUGIA_ORAL_S
					)
				),

				Fuma = Limpiar(
					FirstNonEmpty(
						paciente.FUMA
					)
				),

				Diabetes = Limpiar(
					FirstNonEmpty(
						paciente.DIABETES
					)
				),

				Presion = Limpiar(
					FirstNonEmpty(
						paciente.PRESION,
						paciente.CARDIOP
					)
				),

				Asma = Limpiar(
					FirstNonEmpty(
						paciente.ASMA
					)
				),

				Hepatitis = Limpiar(
					FirstNonEmpty(
						paciente.HEPATITIS
					)
				),

				Hemorragias = Limpiar(
					FirstNonEmpty(
						paciente.HEMORRAGIAS,
						paciente.ALTERACIONES_HEMATOLOGICAS
					)
				),

				Gastricos = Limpiar(
					FirstNonEmpty(
						paciente.GASTRICOS
					)
				),

				Embarazo = Limpiar(
					FirstNonEmpty(
						paciente.EMBARAZO
					)
				),

				Peso = Limpiar(
					FirstNonEmpty(
						paciente.PESO
					)
				),

				Altura = Limpiar(
					FirstNonEmpty(
						paciente.ALTURA
					)
				),

				RevisionSistemas = Limpiar(
					FirstNonEmpty(
						paciente.REVISION_SISTEMAS
					)
				),

				ObservacionesAntecedentes = Limpiar(
					FirstNonEmpty(
						paciente.OBS_ANTESEDENTES,
						paciente.OBS1,
						paciente.OBS2,
						paciente.OBS3
					)
				)
			};
		}

		private static string? FirstNonEmpty(params string?[] values)
		{
			foreach (var value in values)
			{
				if (!string.IsNullOrWhiteSpace(value))
					return value.Trim();
			}

			return null;
		}

		private static string? Limpiar(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			var txt = value.Trim();

			// Limpia valores muy comunes que realmente significan vacío
			if (txt == "-" || txt == "--" || txt == "." || txt == "N/A")
				return null;

			return txt;
		}
	}

	public interface IRdaAntecedentesSourceService
	{
		Task<RdaAntecedentesSource> ConsultarPorAnamnesis(int idAnamnesis);
	}
}
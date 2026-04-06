using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaHistorialLogger
	{
		public const string TipoContexto = "CONTEXTO";
		public const string TipoPersistencia = "PERSISTENCIA";
		public const string TipoFhir = "FHIR";
		public const string TipoEnvio = "ENVIO";
		public const string TipoManual = "MANUAL";
		public const string TipoMasivo = "MASIVO";
		public const string TipoError = "ERROR";
		public const string TipoInfo = "INFO";

		public static async Task LogAsync(
			string evento,
			string tipo,
			int idAnamnesis,
			int? idRda = null,
			string? estado = null,
			string? origen = null,
			string? detalle = null,
			string? usuario = null)
		{
			var objHistorialServicios = new THISTORIALServicios();

			var descripcion = BuildMessage(
				evento: evento,
				tipo: tipo,
				idAnamnesis: idAnamnesis,
				idRda: idRda,
				estado: estado,
				origen: origen,
				detalle: detalle);

			await objHistorialServicios.Agregar(new THISTORIAL()
			{
				FECHA = DateTime.Now.Date,
				HORA = DateTime.Now.TimeOfDay,
				USUARIO = usuario ?? "",
				IDANAMNESIS = idAnamnesis,
				DESCRIPCION = descripcion
			});
		}

		public static string BuildMessage(
			string evento,
			string tipo,
			int idAnamnesis,
			int? idRda = null,
			string? estado = null,
			string? origen = null,
			string? detalle = null)
		{
			var partes = new List<string>
			{
				$"RDA_EVENTO={Clean(evento)}",
				$"Tipo={Clean(tipo)}",
				$"IdAnamnesis={idAnamnesis}"
			};

			if (idRda.HasValue && idRda.Value > 0)
				partes.Add($"IdRda={idRda.Value}");

			if (!string.IsNullOrWhiteSpace(estado))
				partes.Add($"Estado={Clean(estado)}");

			if (!string.IsNullOrWhiteSpace(origen))
				partes.Add($"Origen={Clean(origen)}");

			if (!string.IsNullOrWhiteSpace(detalle))
				partes.Add($"Detalle={Clean(detalle)}");

			return string.Join(" | ", partes);
		}

		private static string Clean(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return "";

			return value
				.Replace("\r", " ")
				.Replace("\n", " ")
				.Replace("|", "/")
				.Trim();
		}
	}
}
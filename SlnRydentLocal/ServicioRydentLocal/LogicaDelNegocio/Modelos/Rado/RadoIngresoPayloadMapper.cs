using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado;
using System;
using System.Globalization;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	internal static class RadoIngresoPayloadMapper
	{
		public static RadoIngresoPayload FromRow(AnamnesisIngresoRow row)
		{
			string pais = row.Codigo_pais.Trim();
			string dpto = pais + row.Codigo_departamento.Trim();
			string ciudad = dpto +  row.Codigo_ciudad.Trim();
			return new RadoIngresoPayload
			{
				// strings (limpias y recortadas)
				Numero_documento = Clean(row.Numero_documento, 20),
				Tipo_documento = Clean(row.Tipo_documento, 30),
				Primer_nombre = Clean(row.PRIMER_NOMBRE, 50),
				Segundo_nombre = Clean(row.SEGUNDO_NOMBRE, 50),
				Primer_apellido = Clean(row.PRIMER_APELLIDO, 50),
				Segundo_apellido = Clean(row.SEGUNDO_APELLIDO, 50),

				// fecha: la dejamos en formato 12 (yyyyMMdd) que es lo que tú ya generas
				Fecha_nacimiento = ToFecha12(row.Fecha_nacimiento),

				// genero: tu SQL manda "M"/"F" o "1"/"2"? Lo hacemos robusto
				Genero = ToGeneroShort(row.Genero),

				Indicativo_pais = Clean(row.Indicativo_pais, 10),

				// integers: si viene texto no numérico => null
				Codigo_pais = ToIntOrNull(pais),
				Codigo_departamento = ToIntOrNull(dpto),
				Codigo_ciudad = ToIntOrNull(ciudad),

				Direccion = Clean(row.direccion, 100),
				Telefono = Clean(row.telefono, 20),
				Correo_electronico = Clean(row.Correo_electronico, 70),

				Personal_codigo = Clean(row.Personal_codigo, 30),
				Personal = Clean(row.Personal, 100),
				Especialidad = Clean(row.ESPECIALIDAD, 100),

				Entidad = ToIntOrNull(row.Entidad),
				Regimen = ToIntOrNull(row.Regimen),

				Centro_Atencion = ToIntOrNull(row.CentroAtencion),
				Tipo_Estudio = ToIntOrNull(row.Tipo_Estudio),
				Codigo_Servicio = ToIntOrNull(row.Codigo_Servicio),

				Servicio_Ips = Clean(row.Servicio_Ips, 100),

				Cantidad = row.cantidad <= 0 ? 1 : row.cantidad,

				Fecha_Solicitud = ToFecha12(row.Fecha_Solicitud),

				// Id_orden en tu row es string
				Id_orden = ToIntOrNull(row.Id_Orden),

				Id_paciente = row.Id_paciente,
				Id_ingreso = row.Id_ingreso,

				Ingreso = row.ingreso,

				// no disponibles => null por ahora
				Hora_estudio = null,
				Minuto_estudio = null,
				Duracion_estudio = null,
				Equipo_modalidad = null
			};
		}

		// ---------------- helpers ----------------

		private static string? Clean(string? s, int maxLen)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;
			s = s.Trim();
			if (s.Length > maxLen) s = s.Substring(0, maxLen);
			return s;
		}

		private static int? ToIntOrNull(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;
			s = s.Trim();

			// a veces llegan con espacios o ceros a la izquierda: OK
			if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
				return v;

			return null;
		}

		private static string? ToFecha12(string? s)
		{
			// Queremos: yyyyMMdd (12 en tu tabla dice 12, pero realmente es 8… igual lo aceptamos)
			// Acepta: "yyyyMMdd" o "yyyy-MM-dd" o DateTime parseable.
			if (string.IsNullOrWhiteSpace(s)) return null;
			s = s.Trim();

			// ya viene yyyyMMdd
			if (s.Length == 8 && DateTime.TryParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1))
				return d1.ToString("yyyyMMdd");

			// viene yyyy-MM-dd
			if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2))
				return d2.ToString("yyyyMMdd");

			// intento general
			if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d3))
				return d3.ToString("yyyyMMdd");

			// si es basura, lo mandamos null para no romper
			return null;
		}

		private static short? ToGeneroShort(string? genero)
		{
			// Rado dice smallint(1). Definimos:
			// 1 = M, 2 = F (puedes cambiar esto si Rado usa otra convención)
			if (string.IsNullOrWhiteSpace(genero)) return null;
			var g = genero.Trim().ToUpperInvariant();

			if (g == "M" || g == "1") return 1;
			if (g == "F" || g == "2") return 2;

			// a veces: "Masculino", "Femenino"
			if (g.StartsWith("M")) return 1;
			if (g.StartsWith("F")) return 2;

			return null;
		}
	}
}

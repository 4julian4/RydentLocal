using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaRetryPolicy
	{
		public static bool EsErrorFuncionalNoReintentable(int? codigoHttp, string? mensaje)
		{
			if (codigoHttp == 400 || codigoHttp == 404 || codigoHttp == 409 || codigoHttp == 422)
				return true;

			if (codigoHttp == 401 || codigoHttp == 403)
				return true;

			var txt = (mensaje ?? "").ToUpperInvariant();

			if (txt.Contains("VALIDACION") ||
				txt.Contains("VALIDACIÓN") ||
				txt.Contains("ESTRUCTURA") ||
				txt.Contains("FHIR INVALIDO") ||
				txt.Contains("FHIR INVÁLIDO") ||
				txt.Contains("PAYLOAD INVALIDO") ||
				txt.Contains("PAYLOAD INVÁLIDO"))
			{
				return true;
			}

			return false;
		}

		public static bool EsErrorTecnicoReintentable(int? codigoHttp, string? mensaje)
		{
			if (codigoHttp == null)
				return true;

			if (codigoHttp == 408 || codigoHttp == 429)
				return true;

			if (codigoHttp >= 500)
				return true;

			var txt = (mensaje ?? "").ToUpperInvariant();

			if (txt.Contains("TIMEOUT") ||
				txt.Contains("TIMED OUT") ||
				txt.Contains("CONNECTION") ||
				txt.Contains("SOCKET") ||
				txt.Contains("NETWORK") ||
				txt.Contains("NO FUE POSIBLE OBTENER TOKEN"))
			{
				return true;
			}

			return false;
		}
	}
}
namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaOptions
	{
		public bool Enabled { get; set; }

		// Compatibilidad vieja
		public string? Endpoint { get; set; }
		public string? ApiKey { get; set; }

		// Base nueva
		public string? BaseUrl { get; set; }

		// Si todavía usas solo 1 endpoint
		public string? EnviarRdaPath { get; set; }

		// Nuevos endpoints separados
		public string? EnviarRdaConsultaPath { get; set; }
		public string? EnviarRdaPacientePath { get; set; }

		public string? ConsultarPacienteExactoPath { get; set; }
		public string? ConsultarPacienteSimilarPath { get; set; }
		public string? ConsultarRdaPacientePath { get; set; }
		public string? ConsultarEncuentrosClinicosPath { get; set; }

		// OAuth2
		public string? TokenUrl { get; set; }
		public string? TokenUrlTemplate { get; set; }
		public string? TenantId { get; set; }
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string? Scope { get; set; }

		//provicional para prueba
		public int? DefaultIdInformacionReporteConsulta { get; set; }

		// APIM
		public string? SubscriptionKey { get; set; }
		public string? SubscriptionKeyHeaderName { get; set; } = "Ocp-Apim-Subscription-Key";

		// HTTP
		public int TimeoutSeconds { get; set; } = 30;

		// Persistencia / operación
		public bool GuardarSnapshot { get; set; } = true;
		public bool EnviarAutomaticamente { get; set; } = false;
		public bool UsarOAuth2 { get; set; } = true;
		public bool RegistrarRequest { get; set; } = true;
		public bool RegistrarResponse { get; set; } = true;

		// Headers
		public string ContentType { get; set; } = "application/fhir+json";
		public string Accept { get; set; } = "application/fhir+json";

		// Ambiente
		public string Ambiente { get; set; } = "QA";

		// Reintentos
		public int MaxIntentosEnvio { get; set; } = 3;
		public bool ReintentarSoloErroresTransitorios { get; set; } = true;

		// Documento soporte consulta
		public bool IncluirDocumentoSoporteConsulta { get; set; } = true;
		public string? DocumentoSoporteBase64 { get; set; }

		public string? ResolveTokenUrl()
		{
			if (!string.IsNullOrWhiteSpace(TokenUrl))
				return TokenUrl.Trim();

			if (!string.IsNullOrWhiteSpace(TokenUrlTemplate) && !string.IsNullOrWhiteSpace(TenantId))
			{
				return TokenUrlTemplate.Replace("{tenantId}", TenantId.Trim());
			}

			if (!string.IsNullOrWhiteSpace(TenantId))
				return $"https://login.microsoftonline.com/{TenantId.Trim()}/oauth2/v2.0/token";

			return null;
		}

		public string? ResolveEnviarUrl(string? tipoDocumento = null)
		{
			if (!string.IsNullOrWhiteSpace(Endpoint))
				return Endpoint.Trim();

			if (string.IsNullOrWhiteSpace(BaseUrl))
				return null;

			var tipo = (tipoDocumento ?? string.Empty).Trim().ToUpperInvariant();
			string? path = null;

			if (tipo == "RDA_PACIENTE_INTERNO")
				path = EnviarRdaPacientePath;
			else if (tipo == "RDA_CONSULTA_INTERNO")
				path = EnviarRdaConsultaPath;

			if (string.IsNullOrWhiteSpace(path))
				path = EnviarRdaPath;

			if (string.IsNullOrWhiteSpace(path))
				return BaseUrl.Trim().TrimEnd('/');

			return $"{BaseUrl.Trim().TrimEnd('/')}/{path.Trim().TrimStart('/')}";
		}

		public string? ResolveConsultarPacienteExactoUrl()
		{
			if (string.IsNullOrWhiteSpace(BaseUrl) || string.IsNullOrWhiteSpace(ConsultarPacienteExactoPath))
				return null;

			return $"{BaseUrl.Trim().TrimEnd('/')}/{ConsultarPacienteExactoPath.Trim().TrimStart('/')}";
		}

		public string? ResolveConsultarPacienteSimilarUrl()
		{
			if (string.IsNullOrWhiteSpace(BaseUrl) || string.IsNullOrWhiteSpace(ConsultarPacienteSimilarPath))
				return null;

			return $"{BaseUrl.Trim().TrimEnd('/')}/{ConsultarPacienteSimilarPath.Trim().TrimStart('/')}";
		}

		public string? ResolveConsultarRdaPacienteUrl()
		{
			if (string.IsNullOrWhiteSpace(BaseUrl) || string.IsNullOrWhiteSpace(ConsultarRdaPacientePath))
				return null;

			return $"{BaseUrl.Trim().TrimEnd('/')}/{ConsultarRdaPacientePath.Trim().TrimStart('/')}";
		}

		public string? ResolveConsultarEncuentrosClinicosUrl()
		{
			if (string.IsNullOrWhiteSpace(BaseUrl) || string.IsNullOrWhiteSpace(ConsultarEncuentrosClinicosPath))
				return null;

			return $"{BaseUrl.Trim().TrimEnd('/')}/{ConsultarEncuentrosClinicosPath.Trim().TrimStart('/')}";
		}
	}
}
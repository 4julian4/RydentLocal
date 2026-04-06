using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class RdaEnvioService : IRdaEnvioService
	{
		private readonly IConfiguration _cfg;

		public RdaEnvioService(IConfiguration cfg)
		{
			_cfg = cfg;
		}

		public async Task<RdaEnvioResultado> EnviarDocumentoPorIdAsync(int idRda)
		{
			var resultado = new RdaEnvioResultado();

			if (idRda <= 0)
			{
				resultado.Ok = false;
				resultado.Mensaje = "IdRda inválido.";
				return resultado;
			}

			var options = new RdaOptions();
			_cfg.GetSection("Interoperabilidad:Rda").Bind(options);

			if (!options.Enabled)
			{
				resultado.Ok = false;
				resultado.Mensaje = "Interoperabilidad:Rda:Enabled = false.";
				return resultado;
			}

			var rdaServicios = new TRDADOCUMENTOServicios();
			var doc = await rdaServicios.ConsultarPorId(idRda);

			if (doc.ID <= 0)
			{
				resultado.Ok = false;
				resultado.Mensaje = "No existe TRDA_DOCUMENTO.";
				return resultado;
			}

			if (string.Equals(doc.ESTADO, "ENVIADO", StringComparison.OrdinalIgnoreCase))
			{
				resultado.Ok = false;
				resultado.Mensaje = "El documento ya fue enviado.";
				return resultado;
			}

			var infoReporteServicios = new TINFORMACIONREPORTESServicios();
			var credencialesCliente = await infoReporteServicios.ConsultarPorId(doc.IDINFORMACIONREPORTE ?? 0);

			if ((doc.IDINFORMACIONREPORTE ?? 0) <= 0 || credencialesCliente.ID <= 0)
			{
				resultado.Ok = false;
				resultado.Mensaje = "El documento RDA no tiene IDINFORMACIONREPORTE válido.";
				return resultado;
			}

			// Habilitación por cliente
			if ((credencialesCliente.RDA_ENABLED ?? 0) == 0)
			{
				resultado.Ok = false;
				resultado.Mensaje = "RDA deshabilitado para este cliente en TINFORMACIONREPORTES.";
				return resultado;
			}

			// Mezclar configuración general + credenciales por cliente
			AplicarCredencialesCliente(options, credencialesCliente);

			var enviarUrl = options.ResolveEnviarUrl(doc.TIPO_DOCUMENTO);
			if (string.IsNullOrWhiteSpace(enviarUrl))
			{
				resultado.Ok = false;
				resultado.Mensaje = "No se encontró URL de envío RDA.";
				return resultado;
			}

			var json = doc.JSON_RDAstr;
			if (string.IsNullOrWhiteSpace(json))
			{
				resultado.Ok = false;
				resultado.Mensaje = "JSON_RDA vacío.";
				return resultado;
			}

			var intentosActuales = doc.INTENTOS ?? 0;
			var maxIntentos = options.MaxIntentosEnvio <= 0 ? 3 : options.MaxIntentosEnvio;

			if (intentosActuales >= maxIntentos)
			{
				await rdaServicios.MarcarNoReintentar(
					doc.ID,
					$"Se alcanzó el máximo de intentos ({maxIntentos}).",
					doc.CODIGO_HTTP,
					doc.RESPUESTA_APIstr);

				resultado.Ok = false;
				resultado.Mensaje = $"Se alcanzó el máximo de intentos ({maxIntentos}).";
				return resultado;
			}

			var intentos = intentosActuales + 1;
			string? requestId = null;

			await rdaServicios.ActualizarEstado(
				doc.ID,
				"ENVIANDO",
				null,
				intentos);

			try
			{
				using (var http = new HttpClient())
				{
					http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds);

					var token = await ObtenerTokenAsync(http, options);
					if (!string.IsNullOrWhiteSpace(token))
					{
						http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
					}
					else if (!string.IsNullOrWhiteSpace(options.ApiKey))
					{
						http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
					}

					if (!string.IsNullOrWhiteSpace(options.SubscriptionKey))
					{
						var headerName = string.IsNullOrWhiteSpace(options.SubscriptionKeyHeaderName)
							? "Ocp-Apim-Subscription-Key"
							: options.SubscriptionKeyHeaderName.Trim();

						if (!http.DefaultRequestHeaders.Contains(headerName))
							http.DefaultRequestHeaders.Add(headerName, options.SubscriptionKey.Trim());
					}

					http.DefaultRequestHeaders.Accept.Clear();
					http.DefaultRequestHeaders.Accept.Add(
						new MediaTypeWithQualityHeaderValue(
							string.IsNullOrWhiteSpace(options.Accept) ? "application/fhir+json" : options.Accept.Trim()));

					using (var request = new HttpRequestMessage(HttpMethod.Post, enviarUrl))
					{
						request.Content = new StringContent(
							json,
							Encoding.UTF8,
							string.IsNullOrWhiteSpace(options.ContentType) ? "application/fhir+json" : options.ContentType.Trim());

						var requestGuardado = options.RegistrarRequest ? json : null;

						if (!string.IsNullOrWhiteSpace(requestGuardado))
						{
							await rdaServicios.GuardarRequestApi(doc.ID, requestGuardado);
						}

						var response = await http.SendAsync(request);
						var responseBody = await response.Content.ReadAsStringAsync();
						var statusCode = (int)response.StatusCode;

						requestId = TryGetHeader(response, "requestId")
							?? TryGetHeader(response, "x-request-id")
							?? TryGetHeader(response, "x-correlation-id");

						if (response.IsSuccessStatusCode)
						{
							var respuestaConRequestId = BuildStoredResponse(responseBody, requestId);

							await rdaServicios.MarcarEnvio(
								doc.ID,
								"ENVIADO",
								null,
								intentos,
								DateTime.Now,
								statusCode,
								options.RegistrarResponse ? respuestaConRequestId : requestId,
								null);

							resultado.Ok = true;
							resultado.Mensaje = "Enviado correctamente.";
							resultado.Respuesta = responseBody;
							resultado.CodigoHttp = statusCode;
							resultado.RequestId = requestId;
							return resultado;
						}
						else
						{
							var apiError = ParseApiError(responseBody, requestId);
							var error = $"HTTP {statusCode} - {apiError.Message}";
							var respuestaGuardada = options.RegistrarResponse
								? BuildStoredResponse(apiError.RawBody, apiError.RequestId)
								: apiError.RequestId;

							var estadoFinal = DeterminarEstadoFinalPorHttp(
								statusCode,
								intentos,
								maxIntentos,
								options);

							if (estadoFinal == "NO_REINTENTAR")
							{
								await rdaServicios.MarcarNoReintentar(
									doc.ID,
									error,
									statusCode,
									respuestaGuardada);
							}
							else
							{
								await rdaServicios.MarcarEnvio(
									doc.ID,
									estadoFinal,
									error,
									intentos,
									null,
									statusCode,
									respuestaGuardada,
									null);
							}

							resultado.Ok = false;
							resultado.Mensaje = error;
							resultado.Respuesta = apiError.RawBody;
							resultado.CodigoHttp = statusCode;
							resultado.RequestId = apiError.RequestId;
							return resultado;
						}
					}
				}
			}
			catch (Exception ex)
			{
				var detalle = BuildStoredResponse(ex.ToString(), requestId);
				var estadoFinal = DeterminarEstadoFinalPorExcepcion(intentos, maxIntentos);

				if (estadoFinal == "NO_REINTENTAR")
				{
					await rdaServicios.MarcarNoReintentar(
						doc.ID,
						ex.Message,
						null,
						detalle);
				}
				else
				{
					await rdaServicios.MarcarEnvio(
						doc.ID,
						estadoFinal,
						ex.Message,
						intentos,
						null,
						null,
						detalle,
						null);
				}

				resultado.Ok = false;
				resultado.Mensaje = ex.Message;
				resultado.Respuesta = ex.ToString();
				resultado.CodigoHttp = null;
				resultado.RequestId = requestId;
				return resultado;
			}
		}

		private static void AplicarCredencialesCliente(RdaOptions options, Entidades.TINFORMACIONREPORTES info)
		{
			options.TenantId = Limpiar(info.RDA_TENANT_ID);
			options.ClientId = Limpiar(info.RDA_CLIENT_ID);
			options.ClientSecret = Limpiar(info.RDA_CLIENT_SECRET);
			options.Scope = Limpiar(info.RDA_SCOPE);
			options.SubscriptionKey = Limpiar(info.RDA_SUBSCRIPTION_KEY);
		}

		private static string? Limpiar(string? valor)
		{
			return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
		}

		private static string DeterminarEstadoFinalPorHttp(int statusCode, int intentos, int maxIntentos, RdaOptions options)
		{
			if (!options.ReintentarSoloErroresTransitorios)
				return intentos >= maxIntentos ? "NO_REINTENTAR" : "ERROR_ENVIO";

			if (EsHttpTransitorio(statusCode))
				return intentos >= maxIntentos ? "NO_REINTENTAR" : "ERROR_ENVIO";

			return "NO_REINTENTAR";
		}

		private static string DeterminarEstadoFinalPorExcepcion(int intentos, int maxIntentos)
		{
			return intentos >= maxIntentos ? "NO_REINTENTAR" : "ERROR_ENVIO";
		}

		private static bool EsHttpTransitorio(int statusCode)
		{
			return statusCode == 408 ||
				   statusCode == 429 ||
				   statusCode == 500 ||
				   statusCode == 502 ||
				   statusCode == 503 ||
				   statusCode == 504;
		}

		private static async Task<string?> ObtenerTokenAsync(HttpClient http, RdaOptions options)
		{
			if (!options.UsarOAuth2)
				return null;

			var tokenUrl = options.ResolveTokenUrl();
			if (string.IsNullOrWhiteSpace(tokenUrl))
				return null;

			if (string.IsNullOrWhiteSpace(options.ClientId) ||
				string.IsNullOrWhiteSpace(options.ClientSecret) ||
				string.IsNullOrWhiteSpace(options.Scope))
			{
				throw new InvalidOperationException(
					"Faltan datos OAuth2 del cliente. Revise TINFORMACIONREPORTES (tenant/client/secret/scope).");
			}

			using (var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl))
			{
				tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["grant_type"] = "client_credentials",
					["client_id"] = options.ClientId.Trim(),
					["client_secret"] = options.ClientSecret.Trim(),
					["scope"] = options.Scope.Trim()
				});

				var tokenResponse = await http.SendAsync(tokenRequest);
				var tokenBody = await tokenResponse.Content.ReadAsStringAsync();

				if (!tokenResponse.IsSuccessStatusCode)
				{
					throw new InvalidOperationException(
						$"No fue posible obtener token OAuth2. HTTP {(int)tokenResponse.StatusCode}. {tokenBody}");
				}

				var tokenObj = JsonConvert.DeserializeObject<RdaOAuthTokenResponse>(tokenBody);
				if (string.IsNullOrWhiteSpace(tokenObj?.AccessToken))
				{
					throw new InvalidOperationException("La respuesta de autenticación no contiene access_token.");
				}

				return tokenObj.AccessToken;
			}
		}

		private static string? TryGetHeader(HttpResponseMessage response, string headerName)
		{
			if (response.Headers.TryGetValues(headerName, out var values))
			{
				foreach (var value in values)
				{
					if (!string.IsNullOrWhiteSpace(value))
						return value.Trim();
				}
			}

			if (response.Content?.Headers != null &&
				response.Content.Headers.TryGetValues(headerName, out var contentValues))
			{
				foreach (var value in contentValues)
				{
					if (!string.IsNullOrWhiteSpace(value))
						return value.Trim();
				}
			}

			return null;
		}

		private static RdaApiError ParseApiError(string? responseBody, string? requestId)
		{
			if (string.IsNullOrWhiteSpace(responseBody))
			{
				return new RdaApiError
				{
					Message = "Sin detalle en respuesta del receptor.",
					RequestId = requestId,
					RawBody = responseBody
				};
			}

			return new RdaApiError
			{
				Message = responseBody,
				RequestId = requestId,
				RawBody = responseBody
			};
		}

		private static string BuildStoredResponse(string? body, string? requestId)
		{
			if (string.IsNullOrWhiteSpace(requestId))
				return body ?? "";

			if (string.IsNullOrWhiteSpace(body))
				return $"requestId={requestId}";

			return $"requestId={requestId}\n{body}";
		}
	}

	public interface IRdaEnvioService
	{
		Task<RdaEnvioResultado> EnviarDocumentoPorIdAsync(int idRda);
	}

	public sealed class RdaEnvioResultado
	{
		public bool Ok { get; set; }
		public string? Mensaje { get; set; }
		public string? Respuesta { get; set; }
		public int? CodigoHttp { get; set; }
		public string? RequestId { get; set; }
	}
}
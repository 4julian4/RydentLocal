using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad;
using ServicioRydentLocal.LogicaDelNegocio.Entidades;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public class InteroperabilidadConsultaPacienteService
	{
		private readonly IConfiguration _configuration;

		public InteroperabilidadConsultaPacienteService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<InteroperabilidadConsultaPacienteRespuesta> ConsultarPacienteExactoAsync(
			InteroperabilidadPacienteFiltro filtro)
		{
			try
			{
				var filtroConsulta = CrearFiltroConsulta(filtro);
				var options = await ConstruirOpcionesConsultaAsync(filtro.IdDoctor);

				ValidarFiltroConsulta(filtroConsulta);

				var url = options.ResolveConsultarPacienteExactoUrl();
				if (string.IsNullOrWhiteSpace(url))
					throw new Exception("No se configuró URL de consulta paciente exacto.");

				var requestBody = BuildPatientParametersRequest(filtroConsulta);
				var jsonRequest = JsonConvert.SerializeObject(requestBody);

				using (var http = await CrearHttpClientAutenticadoAsync(options))
				{
					using (var request = new HttpRequestMessage(HttpMethod.Post, url))
					{
						request.Content = new StringContent(
							jsonRequest,
							Encoding.UTF8,
							string.IsNullOrWhiteSpace(options.ContentType) ? "application/fhir+json" : options.ContentType.Trim());

						var response = await http.SendAsync(request);
						var responseBody = await response.Content.ReadAsStringAsync();

						if (!response.IsSuccessStatusCode)
						{
							return new InteroperabilidadConsultaPacienteRespuesta
							{
								Ok = false,
								Mensaje = $"HTTP {(int)response.StatusCode} - {responseBody}",
								Paciente = null
							};
						}

						return new InteroperabilidadConsultaPacienteRespuesta
						{
							Ok = true,
							Mensaje = "Consulta realizada contra IHCE.",
							Paciente = new InteroperabilidadPacienteResumen
							{
								Encontrado = true,
								Exacto = true,
								RawJson = responseBody
							}
						};
					}
				}
			}
			catch (Exception ex)
			{
				return new InteroperabilidadConsultaPacienteRespuesta
				{
					Ok = false,
					Mensaje = ex.Message,
					Paciente = null
				};
			}
		}

		public async Task<InteroperabilidadConsultaPacienteSimilarRespuesta> ConsultarPacienteSimilarAsync(
			InteroperabilidadPacienteFiltro filtro)
		{
			try
			{
				var filtroConsulta = CrearFiltroConsulta(filtro);
				var options = await ConstruirOpcionesConsultaAsync(filtro.IdDoctor);

				ValidarFiltroConsulta(filtroConsulta);

				var url = options.ResolveConsultarPacienteSimilarUrl();
				if (string.IsNullOrWhiteSpace(url))
					throw new Exception("No se configuró URL de consulta paciente similar.");

				var requestBody = BuildPatientParametersRequest(filtroConsulta);
				var jsonRequest = JsonConvert.SerializeObject(requestBody);

				using (var http = await CrearHttpClientAutenticadoAsync(options))
				{
					using (var request = new HttpRequestMessage(HttpMethod.Post, url))
					{
						request.Content = new StringContent(
							jsonRequest,
							Encoding.UTF8,
							string.IsNullOrWhiteSpace(options.ContentType) ? "application/fhir+json" : options.ContentType.Trim());

						var response = await http.SendAsync(request);
						var responseBody = await response.Content.ReadAsStringAsync();

						if (!response.IsSuccessStatusCode)
						{
							return new InteroperabilidadConsultaPacienteSimilarRespuesta
							{
								Ok = false,
								Mensaje = $"HTTP {(int)response.StatusCode} - {responseBody}",
								Items = new List<InteroperabilidadPacienteSimilarItem>()
							};
						}

						return new InteroperabilidadConsultaPacienteSimilarRespuesta
						{
							Ok = true,
							Mensaje = "Consulta similar realizada contra IHCE.",
							Items = new List<InteroperabilidadPacienteSimilarItem>
							{
								new InteroperabilidadPacienteSimilarItem
								{
									RawJson = responseBody
								}
							}
						};
					}
				}
			}
			catch (Exception ex)
			{
				return new InteroperabilidadConsultaPacienteSimilarRespuesta
				{
					Ok = false,
					Mensaje = ex.Message,
					Items = new List<InteroperabilidadPacienteSimilarItem>()
				};
			}
		}

		public async Task<InteroperabilidadConsultaRdaPacienteRespuesta> ConsultarRdaPacienteAsync(
			InteroperabilidadPacienteFiltro filtro)
		{
			try
			{
				var filtroConsulta = CrearFiltroConsulta(filtro);
				var options = await ConstruirOpcionesConsultaAsync(filtro.IdDoctor);

				ValidarFiltroConsulta(filtroConsulta);

				var url = options.ResolveConsultarRdaPacienteUrl();
				if (string.IsNullOrWhiteSpace(url))
					throw new Exception("No se configuró URL de consulta RDA.");

				var requestBody = BuildPatientParametersRequest(filtroConsulta);
				var jsonRequest = JsonConvert.SerializeObject(requestBody);

				using (var http = new HttpClient())
				{
					http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds);

					var token = await ObtenerTokenAsync(http, options);

					if (!string.IsNullOrWhiteSpace(token))
					{
						http.DefaultRequestHeaders.Authorization =
							new AuthenticationHeaderValue("Bearer", token);
					}

					if (!string.IsNullOrWhiteSpace(options.SubscriptionKey))
					{
						var headerName = options.SubscriptionKeyHeaderName ?? "Ocp-Apim-Subscription-Key";

						if (!http.DefaultRequestHeaders.Contains(headerName))
							http.DefaultRequestHeaders.Add(headerName, options.SubscriptionKey);
					}

					http.DefaultRequestHeaders.Accept.Clear();
					http.DefaultRequestHeaders.Accept.Add(
						new MediaTypeWithQualityHeaderValue("application/fhir+json"));

					var content = new StringContent(jsonRequest, Encoding.UTF8, "application/fhir+json");

					var response = await http.PostAsync(url, content);
					var responseBody = await response.Content.ReadAsStringAsync();

					if (!response.IsSuccessStatusCode)
					{
						return new InteroperabilidadConsultaRdaPacienteRespuesta
						{
							Ok = false,
							Mensaje = $"HTTP {(int)response.StatusCode} - {responseBody}",
							Items = new List<InteroperabilidadRdaPacienteItem>()
						};
					}

					return new InteroperabilidadConsultaRdaPacienteRespuesta
					{
						Ok = true,
						Mensaje = "Consulta RDA OK",
						Items = new List<InteroperabilidadRdaPacienteItem>
						{
							new InteroperabilidadRdaPacienteItem
							{
								RawJson = responseBody
							}
						}
					};
				}
			}
			catch (Exception ex)
			{
				return new InteroperabilidadConsultaRdaPacienteRespuesta
				{
					Ok = false,
					Mensaje = ex.Message,
					Items = new List<InteroperabilidadRdaPacienteItem>()
				};
			}
		}

		public async Task<InteroperabilidadConsultaEncuentrosRespuesta> ConsultarEncuentrosClinicosAsync(
			InteroperabilidadPacienteFiltro filtro)
		{
			try
			{
				var filtroConsulta = CrearFiltroConsulta(filtro);
				var options = await ConstruirOpcionesConsultaAsync(filtro.IdDoctor);

				ValidarFiltroConsulta(filtroConsulta);

				var url = options.ResolveConsultarEncuentrosClinicosUrl();
				if (string.IsNullOrWhiteSpace(url))
					throw new Exception("No se configuró URL de consulta encuentros clínicos.");

				var requestBody = BuildEncuentrosParametersRequest(filtroConsulta);
				var jsonRequest = JsonConvert.SerializeObject(requestBody);

				using (var http = await CrearHttpClientAutenticadoAsync(options))
				{
					using (var request = new HttpRequestMessage(HttpMethod.Post, url))
					{
						request.Content = new StringContent(
							jsonRequest,
							Encoding.UTF8,
							string.IsNullOrWhiteSpace(options.ContentType) ? "application/fhir+json" : options.ContentType.Trim());

						var response = await http.SendAsync(request);
						var responseBody = await response.Content.ReadAsStringAsync();

						if (!response.IsSuccessStatusCode)
						{
							return new InteroperabilidadConsultaEncuentrosRespuesta
							{
								Ok = false,
								Mensaje = $"HTTP {(int)response.StatusCode} - {responseBody}",
								Items = new List<InteroperabilidadEncuentroItem>()
							};
						}

						return new InteroperabilidadConsultaEncuentrosRespuesta
						{
							Ok = true,
							Mensaje = "Consulta de encuentros realizada contra IHCE.",
							Items = new List<InteroperabilidadEncuentroItem>
							{
								new InteroperabilidadEncuentroItem
								{
									RawJson = responseBody
								}
							}
						};
					}
				}
			}
			catch (Exception ex)
			{
				return new InteroperabilidadConsultaEncuentrosRespuesta
				{
					Ok = false,
					Mensaje = ex.Message,
					Items = new List<InteroperabilidadEncuentroItem>()
				};
			}
		}

		private InteroperabilidadPacienteFiltroConsulta CrearFiltroConsulta(InteroperabilidadPacienteFiltro filtro)
		{
			ValidarFiltroEntrada(filtro);

			return new InteroperabilidadPacienteFiltroConsulta
			{
				TipoDocumento = filtro.TipoDocumento?.Trim() ?? "",
				NumeroDocumento = filtro.NumeroDocumento?.Trim() ?? "",
				Humanuser = filtro.Humanuser?.Trim() ?? ""
			};
		}

		private async Task<RdaOptions> ConstruirOpcionesConsultaAsync(string idDoctor)
		{
			var options = new RdaOptions();
			_configuration.GetSection("Interoperabilidad:Rda").Bind(options);

			if (!options.Enabled)
				throw new Exception("Interoperabilidad:Rda:Enabled = false.");

			if (string.IsNullOrWhiteSpace(idDoctor))
				throw new Exception("IdDoctor requerido.");

			if (!int.TryParse(idDoctor, out var doctorId))
				throw new Exception("IdDoctor inválido.");

			var doctoresServicios = new TDATOSDOCTORESServicios();
			var doctor = await doctoresServicios.ConsultarPorId(doctorId);

			if (doctor == null || doctor.ID <= 0)
				throw new Exception("No se encontró TDATOSDOCTORES para el doctor enviado.");

			var idInfo = doctor.IDREPORTE ?? 0;
			if (idInfo <= 0)
				throw new Exception("El doctor no tiene IDREPORTE configurado.");

			var infoReporteServicios = new TINFORMACIONREPORTESServicios();
			var credencialesCliente = await infoReporteServicios.ConsultarPorId(idInfo);

			if (credencialesCliente == null || credencialesCliente.ID <= 0)
				throw new Exception("No se encontró TINFORMACIONREPORTES para consulta interoperable.");

			if ((credencialesCliente.RDA_ENABLED ?? 0) == 0)
				throw new Exception("RDA deshabilitado para este cliente en TINFORMACIONREPORTES.");

			AplicarCredencialesCliente(options, credencialesCliente);

			return options;
		}

		private async Task<HttpClient> CrearHttpClientAutenticadoAsync(RdaOptions options)
		{
			var http = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds)
			};

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

			return http;
		}

		private static void AplicarCredencialesCliente(RdaOptions options, TINFORMACIONREPORTES info)
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

		private void ValidarFiltroEntrada(InteroperabilidadPacienteFiltro filtro)
		{
			if (filtro == null)
				throw new Exception("Filtro requerido.");

			if (string.IsNullOrWhiteSpace(filtro.IdDoctor))
				throw new Exception("IdDoctor requerido.");

			if (string.IsNullOrWhiteSpace(filtro.TipoDocumento))
				throw new Exception("TipoDocumento requerido.");

			if (string.IsNullOrWhiteSpace(filtro.NumeroDocumento))
				throw new Exception("NumeroDocumento requerido.");

			if (string.IsNullOrWhiteSpace(filtro.Humanuser))
				throw new Exception("Humanuser requerido.");
		}

		private void ValidarFiltroConsulta(InteroperabilidadPacienteFiltroConsulta filtro)
		{
			if (filtro == null)
				throw new Exception("Filtro requerido.");

			if (string.IsNullOrWhiteSpace(filtro.TipoDocumento))
				throw new Exception("TipoDocumento requerido.");

			if (string.IsNullOrWhiteSpace(filtro.NumeroDocumento))
				throw new Exception("NumeroDocumento requerido.");

			if (string.IsNullOrWhiteSpace(filtro.Humanuser))
				throw new Exception("Humanuser requerido.");
		}

		private object BuildPatientParametersRequest(InteroperabilidadPacienteFiltroConsulta filtro)
		{
			return new
			{
				resourceType = "Parameters",
				parameter = new object[]
				{
					new
					{
						name = "identifier",
						part = new object[]
						{
							new { name = "type", valueString = filtro.TipoDocumento?.Trim() },
							new { name = "value", valueString = filtro.NumeroDocumento?.Trim() }
						}
					},
					new
					{
						name = "humanuser",
						valueString = filtro.Humanuser?.Trim()
					}
				}
			};
		}

		private object BuildEncuentrosParametersRequest(InteroperabilidadPacienteFiltroConsulta filtro)
		{
			return new
			{
				resourceType = "Parameters",
				parameter = new object[]
				{
					new
					{
						name = "identifier",
						part = new object[]
						{
							new { name = "type", valueString = filtro.TipoDocumento?.Trim() },
							new { name = "value", valueString = filtro.NumeroDocumento?.Trim() }
						}
					},
					new
					{
						name = "humanuser",
						valueString = filtro.Humanuser?.Trim()
					}
				}
			};
		}
	}
}
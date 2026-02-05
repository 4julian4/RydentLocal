using Microsoft.Extensions.Configuration;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Worker;

namespace ServicioRydentLocal.LogicaDelNegocio.Services
{
	public interface IRadoIntegrationService
	{
		Task TryEnviarIngresoPorIdRelacionAsync(long idRelacion, CancellationToken ct = default);

		// Para que puedas inspeccionar qué pasó (provisional)
		string? LastDebugStage { get; }
		string? LastDebugError { get; }
		string? LastDebugJson { get; }
	}

	public sealed class RadoIntegrationService : IRadoIntegrationService
	{
		private readonly IRadoQueryService _radoQuery;
		private readonly IConfiguration _cfg;
		private readonly HttpClient _http;

		public string? LastDebugStage { get; private set; }
		public string? LastDebugError { get; private set; }
		public string? LastDebugJson { get; private set; }

		public RadoIntegrationService(
			IRadoQueryService radoQuery,
			IConfiguration cfg,
			HttpClient http)
		{
			_radoQuery = radoQuery;
			_cfg = cfg;
			_http = http;

			_http.Timeout = TimeSpan.FromSeconds(20);
		}

		public async Task TryEnviarIngresoPorIdRelacionAsync(long idRelacion, CancellationToken ct = default)
		{
			// Reset debug cada vez
			LastDebugStage = null;
			LastDebugError = null;
			LastDebugJson = null;

			if (idRelacion <= 0)
			{
				LastDebugStage = "config";
				LastDebugError = "idRelacion inválido.";
				return;
			}

			// 0) Leer config
			var opt = new RadoOptions();
			try
			{
				LastDebugStage = "config";
				_cfg.GetSection("Integraciones:Rado").Bind(opt);

				if (!opt.Enabled)
				{
					LastDebugError = "Integración Rado deshabilitada (Enabled=false).";
					return;
				}

				if (string.IsNullOrWhiteSpace(opt.Endpoint))
				{
					LastDebugError = "Endpoint vacío en Integraciones:Rado:Endpoint.";
					return;
				}
			}
			catch (Exception ex)
			{
				LastDebugStage = "config";
				LastDebugError = ex.ToString();
				return;
			}

			// 1) Consultar ingreso (AQUÍ capturamos el error exacto)
			AnamnesisIngresoRow? row = null;
			try
			{
				LastDebugStage = "query";
				row = await _radoQuery.ConsultarIngresoPorIdRelacion(idRelacion, ct);
			}
			catch (Exception ex)
			{
				LastDebugStage = "query";
				LastDebugError = ex.ToString(); // stacktrace completo
				return;
			}

			if (row == null)
			{
				LastDebugStage = "query";
				LastDebugError = "No hubo datos (row == null).";
				return;
			}

			// 2) Mapear + serializar
			string payloadJson;
			try
			{
				LastDebugStage = "map";

				// Si ya tienes mapper robusto:
				var payload = RadoIngresoPayloadMapper.FromRow(row);
				// Agrego token y tipo transaccion y idTransaccion
				payload.Token = opt.ApiKey;
				payload.TipoTransaccion = "ABONO";
				payload.IdTransaccion = idRelacion;

				LastDebugStage = "serialize";
				payloadJson = JsonSerializer.Serialize(payload, JsonHelper.Options);

				// guardamos para inspección
				LastDebugJson = payloadJson;
			}
			catch (Exception ex)
			{
				LastDebugStage = "serialize";
				LastDebugError = ex.ToString();
				return;
			}

			// 3) HTTP
			try
			{
				LastDebugStage = "http";

				_http.DefaultRequestHeaders.Authorization = null; // evita token viejo
				if (!string.IsNullOrWhiteSpace(opt.ApiKey))
					_http.DefaultRequestHeaders.Authorization =
						new AuthenticationHeaderValue("Bearer", opt.ApiKey);

				using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
				using var resp = await _http.PostAsync(opt.Endpoint, content, ct);

				if (!resp.IsSuccessStatusCode)
				{
					var body = await resp.Content.ReadAsStringAsync(ct);
					LastDebugError = $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} | {body}";
					return;
				}

				// ok
				LastDebugError = null;
			}
			catch (Exception ex)
			{
				LastDebugStage = "http";
				LastDebugError = ex.ToString();
				return;
			}
		}
	}
}

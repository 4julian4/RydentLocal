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

			// 1) Consultar cabecera (igual que antes)
			AnamnesisIngresoRow? row;
			try
			{
				LastDebugStage = "query";
				row = await _radoQuery.ConsultarIngresoPorIdRelacion(idRelacion, ct);
			}
			catch (Exception ex)
			{
				LastDebugStage = "query";
				LastDebugError = ex.ToString();
				return;
			}

			if (row == null)
			{
				LastDebugStage = "query";
				LastDebugError = "No hubo datos (row == null).";
				return;
			}

			// 2) Consultar motivos (servicios)
			IReadOnlyList<AbonoMotivoRow> motivos;
			try
			{
				LastDebugStage = "query_motivos";
				motivos = await _radoQuery.ConsultarMotivosPorIdRelacion(idRelacion, ct);
			}
			catch (Exception ex)
			{
				LastDebugStage = "query_motivos";
				LastDebugError = ex.ToString();
				return;
			}

			// 3) Preparar headers una sola vez
			try
			{
				LastDebugStage = "http_prep";
				_http.DefaultRequestHeaders.Authorization = null;
				if (!string.IsNullOrWhiteSpace(opt.ApiKey))
					_http.DefaultRequestHeaders.Authorization =
						new AuthenticationHeaderValue("Bearer", opt.ApiKey);
			}
			catch (Exception ex)
			{
				LastDebugStage = "http_prep";
				LastDebugError = ex.ToString();
				return;
			}

			// 4) Si NO hay motivos -> enviar 1 como hoy (compatibilidad)
			if (motivos == null || motivos.Count == 0)
			{
				try
				{
					await EnviarUnoAsync(row, opt, idRelacion, ct, motivo: null);
					LastDebugError = null;
					return;
				}
				catch
				{
					// EnviarUnoAsync ya llenó LastDebugStage/LastDebugError
					return;
				}
			}

			// 5) Si hay motivos -> enviar 1 JSON por cada motivo
			foreach (var m in motivos)
			{
				try
				{
					await EnviarUnoAsync(row, opt, idRelacion, ct, motivo: m);
				}
				catch
				{
					// paro al primer error para que sea fácil depurar
					return;
				}
			}

			// ok
			LastDebugError = null;
		}

		private async Task EnviarUnoAsync(
			AnamnesisIngresoRow baseRow,
			RadoOptions opt,
			long idRelacion,
			CancellationToken ct,
			AbonoMotivoRow? motivo = null)
		{
			string payloadJson;

			try
			{
				LastDebugStage = "map";

				// 1) construyo payload desde la cabecera (igual que antes)
				var payload = RadoIngresoPayloadMapper.FromRow(baseRow);

				// 2) si hay motivo, PISO los campos que cambian por servicio
				if (motivo != null)
				{
					// Tipo_Estudio es int? en tu payload y DESCRIPCION es texto -> no lo tocamos por ahora.
					payload.Codigo_Servicio = TryInt(motivo.Codigo) ?? payload.Codigo_Servicio;
					payload.Cantidad = motivo.Cantidad <= 0 ? 1 : motivo.Cantidad;
					payload.Ingreso = motivo.Valor ?? payload.Ingreso;
				}

				payload.Token = opt.ApiKey;
				payload.TipoTransaccion = "ABONO";
				payload.IdTransaccion = idRelacion;

				LastDebugStage = "serialize";
				payloadJson = JsonSerializer.Serialize(payload, JsonHelper.Options);
				LastDebugJson = payloadJson;
			}
			catch (Exception ex)
			{
				LastDebugStage = "serialize";
				LastDebugError = ex.ToString();
				throw;
			}

			try
			{
				LastDebugStage = "http";

				using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
				using var resp = await _http.PostAsync(opt.Endpoint, content, ct);

				if (!resp.IsSuccessStatusCode)
				{
					var body = await resp.Content.ReadAsStringAsync(ct);
					LastDebugError = $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} | {body}";
					throw new Exception(LastDebugError);
				}
			}
			catch (Exception ex)
			{
				LastDebugStage = "http";
				LastDebugError = ex.ToString();
				throw;
			}
		}

		private static int? TryInt(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return null;
			s = s.Trim();
			return int.TryParse(s, out var v) ? v : null;
		}
	}
}
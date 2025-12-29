using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Dataico
{
	internal sealed class WorkerValidationError
	{
		public string? Field { get; set; }
		public string? Error { get; set; }
		// por si la API intermedia usa "path" (array) en algún momento:
		public IEnumerable<object>? Path { get; set; }
	}

	internal sealed class WorkerApiResponse
	{
		public bool Success { get; set; }
		public string? Code { get; set; }        // <-- NUEVO: leemos "code"
		public string? Message { get; set; }
		public int StatusCode { get; set; }
		public string? Uuid { get; set; }
		public string? Id { get; set; }
		public List<WorkerValidationError>? Errors { get; set; }
	}

	public class ApiIntermediaClient
	{
		private readonly HttpClient _http;
		private readonly IConfiguration _cfg;

		public ApiIntermediaClient(HttpClient http, IConfiguration cfg)
		{
			_http = http;
			_cfg = cfg;
		}

		public async Task<(bool ok, string? mensaje, string? externalId)>
			PostHealthInvoiceAsync(string codigoPrestador, string bodyJson, CancellationToken ct = default)
		{
			var tenantHeader = string.IsNullOrWhiteSpace(codigoPrestador)
				? (_cfg["ApiIntermedia:DefaultTenantCode"] ?? string.Empty)
				: codigoPrestador;

			using var req = new HttpRequestMessage(HttpMethod.Post, "/api/fes/documents");
			req.Headers.TryAddWithoutValidation("X-Tenant-Code", tenantHeader);
			req.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

			using var res = await _http.SendAsync(req, ct);
			var content = await res.Content.ReadAsStringAsync(ct);

			WorkerApiResponse? parsed = null;
			try { parsed = JsonConvert.DeserializeObject<WorkerApiResponse>(content); } catch { /* tolerante */ }

			// ===== ÉXITO (2xx) =====
			if (res.IsSuccessStatusCode)
			{
				if (parsed?.Success == true)
				{
					var uuid = !string.IsNullOrWhiteSpace(parsed.Uuid)
						? parsed.Uuid
						: (!string.IsNullOrWhiteSpace(parsed.Id) ? parsed.Id : ExtractJsonValue(content, "uuid") ?? ExtractJsonValue(content, "id"));

					var msg = !string.IsNullOrWhiteSpace(parsed.Message)
						? parsed.Message
						: "Documento enviado correctamente.";

					return (true, msg, uuid);
				}

				// 2xx pero success=false (raro). Devolvemos lo que haya.
				var fallbackUuid = parsed?.Uuid
					?? parsed?.Id
					?? ExtractJsonValue(content, "uuid")
					?? ExtractJsonValue(content, "id");

				var msg2xx = !string.IsNullOrWhiteSpace(parsed?.Message) ? parsed!.Message : "Operación completada con observaciones.";
				return (true, msg2xx, fallbackUuid);
			}

			// ===== ERROR HTTP (4xx/5xx) -> construir mensaje claro =====
			string mensaje;

			if (parsed is not null)
			{
				// Detalle corto (primer error)
				string? firstDetail = null;
				if (parsed.Errors is { Count: > 0 })
				{
					var e = parsed.Errors[0];
					if (e != null)
					{
						if (e.Path != null)
						{
							var path = string.Join(".", e.Path.Select(p => p?.ToString()));
							if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(e.Error))
								firstDetail = $"{path}: {e.Error}";
						}
						// fallback a field + error
						if (firstDetail == null && !string.IsNullOrWhiteSpace(e.Field) && !string.IsNullOrWhiteSpace(e.Error))
							firstDetail = $"{e.Field}: {e.Error}";
						// o solo el error
						if (firstDetail == null && !string.IsNullOrWhiteSpace(e.Error))
							firstDetail = e.Error!.Trim();
					}
				}

				// Mensajes específicos por code de la API intermedia
				switch (parsed.Code)
				{
					case "TENANT_HEADER_MISSING":
						mensaje = "Falta cabecera X-Tenant-Code";
						break;

					case "TENANT_NOT_FOUND":
						mensaje = $"Tenant '{tenantHeader}' no existe";
						break;

					case "TENANT_FORBIDDEN":
						// Tu API manda message = "Tenant no encontrado" con este code.
						// Mostramos algo súper claro para el usuario final:
						mensaje = $"Tenant '{tenantHeader}' sin autorización o no existe";
						break;

					case "DATAICO_ERROR":
						mensaje = firstDetail != null
							? $"Validación fallida · {firstDetail}"
							: (!string.IsNullOrWhiteSpace(parsed.Message) ? parsed.Message! : "Validación fallida");
						break;

					default:
						// Si hay code desconocido, lo incluimos para soporte
						if (!string.IsNullOrWhiteSpace(parsed.Code) && !string.IsNullOrWhiteSpace(parsed.Message))
							mensaje = $"{parsed.Code}: {parsed.Message}";
						else if (!string.IsNullOrWhiteSpace(parsed.Message))
							mensaje = parsed.Message!;
						else
							mensaje = "Error al crear la factura.";
						break;
				}
			}
			else
			{
				// Sin contrato JSON válido: intenta sacar "message" crudo
				var msgApi = ExtractJsonValue(content, "message");
				mensaje = !string.IsNullOrWhiteSpace(msgApi) ? msgApi! : "Error al crear la factura.";
			}

			return (false, mensaje, null);
		}

		private static string? ExtractJsonValue(string json, string key)
		{
			var token = $"\"{key}\"";
			var idx = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
			if (idx < 0) return null;

			var colon = json.IndexOf(':', idx);
			if (colon < 0) return null;

			var after = json.Substring(colon + 1).TrimStart();
			if (!after.StartsWith("\"")) return null;

			var end = after.IndexOf('\"', 1);
			if (end <= 1) return null;

			return after.Substring(1, end - 1);
		}
	}
}

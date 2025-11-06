using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Dataico
{
	/// <summary>
	/// Cliente HTTP hacia tu API central (Billing.Api).
	/// </summary>
	public class ApiIntermediaClient
	{
		private readonly HttpClient _http;
		private readonly IConfiguration _cfg;

		public ApiIntermediaClient(HttpClient http, IConfiguration cfg)
		{
			_http = http;
			_cfg = cfg;
		}

		/// <summary>
		/// Envía una FES a /api/fes/documents.
		/// - X-Tenant-Code = codigoPrestador (o DefaultTenantCode si viene vacío)
		/// - bodyJson = JSON de HealthInvoiceDto
		/// </summary>
		public async Task<(bool ok, string? mensaje, string? externalId)>
	PostHealthInvoiceAsync(string codigoPrestador, string bodyJson, CancellationToken ct = default)
		{
			var tenantHeader = string.IsNullOrWhiteSpace(codigoPrestador)
				? (_cfg["ApiIntermedia:DefaultTenantCode"] ?? string.Empty)
				: codigoPrestador;

			using var req = new HttpRequestMessage(HttpMethod.Post, "/api/fes/documents");
			req.Headers.Add("X-Tenant-Code", tenantHeader);
			req.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

			using var res = await _http.SendAsync(req, ct);
			var content = await res.Content.ReadAsStringAsync(ct);

			// Caso 2xx: la API intermedia devuelve tu DataicoResponse
			if (res.IsSuccessStatusCode)
			{
				// Intenta extraer un identificador útil (uuid/id)
				var uuid = ExtractJsonValue(content, "uuid")
						   ?? ExtractJsonValue(content, "id");
				return (true, null, uuid);
			}

			// Caso error (400/401/403/404/500): la API intermedia ahora devuelve:
			// { success:false, code:"...", message:"...", errors:[...] }
			var msgApi = ExtractJsonValue(content, "message");
			var codeApi = ExtractJsonValue(content, "code");

			// Mensaje amigable para el front (en español)
			string mensaje =
				res.StatusCode switch
				{
					System.Net.HttpStatusCode.BadRequest => msgApi ?? "Solicitud inválida.",
					System.Net.HttpStatusCode.Unauthorized => msgApi ?? "No autorizado.",
					System.Net.HttpStatusCode.Forbidden => msgApi ?? "El tenant no está autorizado.",
					System.Net.HttpStatusCode.NotFound => msgApi ?? "Tenant no encontrado.",
					_ => msgApi ?? "Error interno al crear la factura."
				};

			// Incluye el code para depuración si vino
			if (!string.IsNullOrWhiteSpace(codeApi))
				mensaje = $"{mensaje} (code: {codeApi})";

			return (false, $"HTTP {(int)res.StatusCode}: {mensaje}", null);
		}

		/// <summary>
		/// Extrae un valor string de primer nivel de un JSON sin depender de libs externas.
		/// </summary>
		private static string? ExtractJsonValue(string json, string key)
		{
			// Búsqueda tolerante a mayúsculas/minúsculas de "key":"valor"
			// (si el valor no es string se ignora en este helper)
			var token = $"\"{key}\"";
			var idx = json.IndexOf(token, StringComparison.OrdinalIgnoreCase);
			if (idx < 0) return null;

			// Avanza hasta el ':' y el primer '"'
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

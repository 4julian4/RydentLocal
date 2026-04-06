using Newtonsoft.Json;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaOAuthTokenResponse
	{
		[JsonProperty("access_token")]
		public string? AccessToken { get; set; }

		[JsonProperty("token_type")]
		public string? TokenType { get; set; }

		[JsonProperty("expires_in")]
		public int? ExpiresIn { get; set; }
	}

	public sealed class RdaApiError
	{
		public string? Message { get; set; }
		public string? RequestId { get; set; }
		public string? RawBody { get; set; }
	}
}
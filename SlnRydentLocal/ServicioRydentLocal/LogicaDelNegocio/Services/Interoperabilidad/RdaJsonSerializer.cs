using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ServicioRydentLocal.LogicaDelNegocio.Services.Interoperabilidad
{
	public static class RdaJsonSerializer
	{
		private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore,
			Formatting = Formatting.Indented,
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public static string Serialize(object value)
		{
			return JsonConvert.SerializeObject(value, _settings);
		}
	}
}
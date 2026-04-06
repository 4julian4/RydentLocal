using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir
{
	public sealed class FhirBundleRdaConsulta
	{
		public string resourceType { get; set; } = "Bundle";
		public string language { get; set; } = "es-CO";
		public FhirIdentifier? identifier { get; set; }
		public string type { get; set; } = "document";
		public DateTimeOffset timestamp { get; set; }
		public List<FhirBundleEntry> entry { get; set; } = new List<FhirBundleEntry>();
	}

	public sealed class FhirBundleEntry
	{
		// IHCE rechaza cualquier propiedad adicional en entry para envío RDA.
		// La dejamos en el modelo pero NO se serializa.
		[JsonIgnore]
		public string? fullUrl { get; set; }

		public object? resource { get; set; }
	}

	
}
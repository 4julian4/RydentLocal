namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad.Fhir
{
	public sealed class RdaFhirValidationResult
	{
		public bool Ok { get; set; }
		public string? Error { get; set; }
	}
}
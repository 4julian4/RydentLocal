namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaAntecedentesSource
	{
		public int IdAnamnesis { get; set; }

		public string? MotivoConsulta { get; set; }
		public string? EnfermedadActual { get; set; }

		public string? AlergiasTexto { get; set; }
		public string? AlergiaBandera { get; set; }

		public string? MedicamentosActualesTexto { get; set; }
		public string? EnfermedadesPreviasTexto { get; set; }
		public string? CirugiasTexto { get; set; }

		public string? Fuma { get; set; }
		public string? Diabetes { get; set; }
		public string? Presion { get; set; }
		public string? Asma { get; set; }
		public string? Hepatitis { get; set; }
		public string? Hemorragias { get; set; }
		public string? Gastricos { get; set; }
		public string? Embarazo { get; set; }

		public string? Peso { get; set; }
		public string? Altura { get; set; }

		public string? RevisionSistemas { get; set; }
		public string? ObservacionesAntecedentes { get; set; }
	}
}
namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaDiagnosticoSource
	{
		public int IdAnamnesis { get; set; }

		public string? NumeroAutorizacion { get; set; }

		public string? CodigoConsulta { get; set; }
		public string? NombreCodigoConsulta { get; set; }

		public string? CodigoFinalidad { get; set; }
		public string? CodigoCausa { get; set; }

		public string? Diagnostico1 { get; set; }
		public string? NombreDiagnostico1 { get; set; }

		public string? Diagnostico2 { get; set; }
		public string? NombreDiagnostico2 { get; set; }

		public string? Diagnostico3 { get; set; }
		public string? NombreDiagnostico3 { get; set; }

		public string? Diagnostico4 { get; set; }
		public string? NombreDiagnostico4 { get; set; }

		public string? CodigoTipoDiagnostico { get; set; }

		public string? Pronostico { get; set; }
		public string? Observaciones { get; set; }
	}
}
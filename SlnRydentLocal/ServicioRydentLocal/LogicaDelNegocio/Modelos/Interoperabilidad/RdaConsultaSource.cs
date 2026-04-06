namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaConsultaSource
	{
		public RdaEncounterSource Encounter { get; set; } = new RdaEncounterSource();
		public RdaAntecedentesSource Antecedentes { get; set; } = new RdaAntecedentesSource();
		public RdaDiagnosticoSource Diagnostico { get; set; } = new RdaDiagnosticoSource();
		public RdaProcedimientoSource Procedimiento { get; set; } = new RdaProcedimientoSource();
	}
}
namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaDocumentoCompletoSource
	{
		public RdaConsultaSource Consulta { get; set; } = new RdaConsultaSource();
		public RdaPrestadorSource Prestador { get; set; } = new RdaPrestadorSource();
	}
}
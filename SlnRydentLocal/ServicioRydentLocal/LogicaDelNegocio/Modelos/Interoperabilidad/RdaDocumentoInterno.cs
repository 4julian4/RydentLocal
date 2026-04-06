using System;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaDocumentoInterno
	{
		public string TipoDocumento { get; set; } = "RDA_CONSULTA_INTERNO";
		public DateTime FechaGeneracion { get; set; } = DateTime.Now;
		public RdaDocumentoCompletoSource Documento { get; set; } = new RdaDocumentoCompletoSource();
	}
}
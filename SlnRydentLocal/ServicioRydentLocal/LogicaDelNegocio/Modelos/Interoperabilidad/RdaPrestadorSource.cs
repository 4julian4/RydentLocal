namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaPrestadorSource
	{
		public string? CodigoPrestador { get; set; }
		public string? NombrePrestador { get; set; }
		public string? NitPrestador { get; set; }
		public string? TipoDocumentoPrestador { get; set; }
		public int? IdInformacionReporte { get; set; }

		public string? DireccionPrestador { get; set; }
		public string? TelefonoPrestador { get; set; }
		public string? CiudadPrestador { get; set; }

		public int? IdDoctor { get; set; }
		public string? NombreDoctor { get; set; }
		public string? TipoDocumentoDoctor { get; set; }
		public string? NumeroDocumentoDoctor { get; set; }
		public string? EspecialidadDoctor { get; set; }
	}
}
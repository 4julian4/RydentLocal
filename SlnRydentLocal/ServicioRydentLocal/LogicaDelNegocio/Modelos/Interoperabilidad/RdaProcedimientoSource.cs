namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaProcedimientoSource
	{
		public int IdAnamnesis { get; set; }

		public string? NumeroAutorizacion { get; set; }

		public string? CodigoProcedimiento { get; set; }
		public string? NombreProcedimiento { get; set; }

		public string? AmbitoRealizacion { get; set; }
		public string? FinalidadProcedimiento { get; set; }
		public string? PersonalQueAtiende { get; set; }

		public string? DxPrincipal { get; set; }
		public string? NombreDxPrincipal { get; set; }

		public string? DxRelacionado { get; set; }
		public string? NombreDxRelacionado { get; set; }

		public string? Complicacion { get; set; }

		public string? FormaRealizacionActoQuir { get; set; }
		public double? ValorProcedimiento { get; set; }

		public string? CodigoEntidad { get; set; }
		public string? NombreEntidad { get; set; }

		public string? Extranjero { get; set; }
		public string? Pais { get; set; }
	}
}
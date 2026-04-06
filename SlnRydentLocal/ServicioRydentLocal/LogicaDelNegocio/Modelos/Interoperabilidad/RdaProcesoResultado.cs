namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Interoperabilidad
{
	public sealed class RdaProcesoResultado
	{
		public bool Ok { get; set; }
		public int? IdRda { get; set; }
		public string? Estado { get; set; }
		public string? Mensaje { get; set; }
	}
}
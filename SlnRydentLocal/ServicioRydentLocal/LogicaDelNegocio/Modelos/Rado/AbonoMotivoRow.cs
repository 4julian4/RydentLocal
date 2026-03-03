namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Rado
{
	public sealed class AbonoMotivoRow
	{
		public string? Descripcion { get; set; }
		public string? Codigo { get; set; }
		public int Cantidad { get; set; }
		public decimal? Valor { get; set; }
		public decimal? ValorIva { get; set; }
		public decimal? PorcentajeIva { get; set; }
	}
}
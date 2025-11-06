namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Resultados
{
	public class ResultadoPresentacionItem
	{
		public int idRelacion { get; set; }
		public string? factura { get; set; }
		public string codigoPrestador { get; set; } = "";
		public bool ok { get; set; }
		public string? mensaje { get; set; }
		public string? externalId { get; set; } // id/uuid devuelto por la API
	}
}

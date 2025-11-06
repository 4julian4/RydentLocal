namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico.Solicitudes
{
	public class PresentarFacturaItem
	{
		public int idRelacion { get; set; }              // clave para armar el body (SP)
		public string codigoPrestador { get; set; } = ""; // header X-Tenant-Code
		public string? factura { get; set; }             // opcional (traza)
		public int? tipoFactura { get; set; }            // por si luego manejas otros tipos
		public string? operation { get; set; }           // "SS_SIN_APORTE" (si quieres forzar)
	}
}

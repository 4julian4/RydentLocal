public class CrearEstadoCuentaResponse
{
	public bool Ok { get; set; }
	public string? Mensaje { get; set; }

	public int PacienteId { get; set; }
	public int DoctorId { get; set; }
	public int Fase { get; set; }

	public int? Consecutivo { get; set; }

	// 👇 para que el front lo muestre / refresque igual que Delphi
	public string? Factura { get; set; }
}

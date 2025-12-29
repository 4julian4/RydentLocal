public class CrearEstadoCuentaRequest
{
	// Claves
	public int PacienteId { get; set; }
	public int DoctorId { get; set; }
	public int Fase { get; set; }

	// Datos principales
	public DateTime FechaInicio { get; set; }
	public decimal ValorTratamiento { get; set; }
	public decimal ValorCuotaInicial { get; set; }

	// Financiación
	public int NumeroCuotas { get; set; } = 1;
	public decimal ValorCuota { get; set; }

	// 👇 En Delphi esto SIEMPRE existe
	public int NumeroCuotaInicial { get; set; } = 1;      // CBCuotasIni
	public int IntervaloInicialDias { get; set; } = 30;   // CBIntervaloIni

	// Intervalo general
	public int IntervaloTiempoDias { get; set; } = 30;

	// Texto
	public string Descripcion { get; set; } = "";
	public string? Observaciones { get; set; }

	// Documento (factura o compromiso)
	public string? Documento { get; set; }

	// Convenio (-1 si ninguno)
	public int ConvenioId { get; set; } = -1;

	// Otros
	public int IdPresupuestoMaestra { get; set; } = -1;
	public bool? FacturaVieja { get; set; }

	public string? NumeroHistoria { get; set; }
}

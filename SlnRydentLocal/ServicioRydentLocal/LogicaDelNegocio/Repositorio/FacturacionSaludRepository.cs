using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico;

// Entidades de salida del SP
using ServicioRydentLocal.LogicaDelNegocio.Entidades.SP;

namespace ServicioRydentLocal.LogicaDelNegocio.Repositorio
{
	public class FacturacionSaludRepository
	{
		private readonly AppDbContext _db;

		// AHORA inyectamos el DbContext (no el connection string)
		public FacturacionSaludRepository(AppDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Construye el DTO para Dataico usando tu SP P_ADICIONALES_ABONOS_DIAN.
		/// </summary>
		public async Task<HealthInvoiceDto> BuildHealthInvoiceDtoSinAporteAsync(int idRelacion, CancellationToken ct = default)
		{
			// Ejecuta tu SP via AppDbContext (ya lo tienes listo)
			// Nota: tu método es síncrono; lo envolvemos para no bloquear el hilo.
			var rows = await Task.Run(() => _db.P_ADICIONALES_ABONOS_DIAN(idRelacion), ct);

			var r = rows.FirstOrDefault();
			if (r == null)
				throw new Exception($"No existe IDRELACION={idRelacion} en P_ADICIONALES_ABONOS_DIAN");

			// Lectura SEGURA con null-coalescing
			string factura = r.FACTURA ?? "";
			DateTime fechaFactura = r.FECHAFACTURA; // tu SP la retorna como DATE válido
			string prefijo = r.PREFIJO ?? "";
			string resolucion = r.RESOLUCION ?? "";
			string nit = r.NIT ?? "";
			string departamento = r.DEPARTAMENTO ?? "";
			string ciudad = r.CIUDAD ?? "";
			string direccion = r.DIRECCION ?? "";
			string pais = r.CODIGOPAIS ?? "CO";
			string telefono = r.TELF_P ?? "0000000";
			string descripcion = string.IsNullOrWhiteSpace(r.DESCRIPCION) ? "Paquete sin aporte" : r.DESCRIPCION;
			decimal valor = r.VALOR;
			string regimen = string.IsNullOrWhiteSpace(r.REGIMEN) ? "ORDINARIO" : r.REGIMEN;
			string tipoPersona = string.IsNullOrWhiteSpace(r.TIPOPERSONA) ? "PERSONA_JURIDICA" : r.TIPOPERSONA;

			// Construcción del DTO para Dataico (ajusta campos si tu operación requiere otros)
			var dto = new HealthInvoiceDto
			{
				SendToDian = false, // cambia a true cuando quieras enviar realmente
				SendEmail = false,
				Number = factura,
				IssueDate = DdMMyyyy(fechaFactura),
				PaymentDate = DdMMyyyy(fechaFactura),
				InvoiceTypeCode = "FACTURA_VENTA",
				PaymentMeans = "BANK_TRANSFER",
				PaymentMeansType = "DEBITO",
				OrderReference = null,

				Numbering = new NumberingDto
				{
					ResolutionNumber = resolucion,
					Prefix = prefijo,
					Flexible = true
				},

				Customer = new CustomerDto
				{
					PartyIdentificationType = "NIT",
					PartyIdentification = nit,
					PartyType = tipoPersona,          // del SP
					TaxLevelCode = "COMUN",
					Regimen = regimen,
					CompanyName = r.NOMBRE ?? "CLIENTE EPS/ENTIDAD",
					Email = r.CORREO ?? "no-reply@example.com",
					Phone = telefono,
					Department = departamento,
					City = ciudad,
					AddressLine = direccion,
					CountryCode = pais
				},

				Items =
				{
					new ItemDto
					{
						Sku         = "931004",
						Quantity    = 1,
						Description = descripcion,
						Price       = valor
					}
				},

				Operation = "SS_SIN_APORTE",

				Health = new HealthDto
				{
					Version = "API_SALUD_V2",
					Coverage = "PLAN_DE_BENEFICIOS",
					PaymentModality = "PAGO_POR_CAPITACION",
					ProviderCode = (r.CODIGO_PRESTADOR ?? "").Trim(), // si tu SP lo trae
					PeriodStartDate = DdMMyyyy(fechaFactura),
					PeriodEndDate = DdMMyyyy(fechaFactura)
				},

				Currency = string.IsNullOrWhiteSpace(r.CODIGOMONEDA) ? "COP" : r.CODIGOMONEDA
			};

			return dto;
		}

		private static string DdMMyyyy(DateTime dt)
			=> dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Modelos.Dataico
{
	/// <summary>
	/// DTO para representar una factura electrónica en sector salud (FES).
	/// Soporta operaciones: SS_RECAUDO, SS_CUDE, SS_CUFE, SS_POS, SS_SNUM, SS_SIN_APORTE.
	/// Diseño: en la RAÍZ se dejan como no-nullable SOLO los campos realmente comunes a los 6 casos
	/// para que el model binder de ASP.NET Core no marque "required" en otros.
	/// La obligatoriedad específica por operación se controla en FluentValidation.
	/// </summary>
	public class HealthInvoiceDto
	{
		/// <summary>Indica si se debe enviar a la DIAN.</summary>
		public bool SendToDian { get; set; }

		/// <summary>Indica si se debe enviar por correo electrónico.</summary>
		public bool SendEmail { get; set; }

		/// <summary>Número de la factura (consecutivo interno). Común a los 6 casos.</summary>
		public string Number { get; set; } = default!;

		/// <summary>Fecha de emisión (dd/MM/yyyy). Común a los 6 casos.</summary>
		public string IssueDate { get; set; } = default!;

		/// <summary>Fecha de pago (dd/MM/yyyy). Común a los 6 casos.</summary>
		public string PaymentDate { get; set; } = default!;

		/// <summary>Código del tipo de factura (ej.: "FACTURA_VENTA"). Común a los 6 casos.</summary>
		public string InvoiceTypeCode { get; set; } = default!;

		/// <summary>
		/// Medio de pago (CASH, BANK_TRANSFER, CHECK, CREDIT_CARD, DEBIT_CARD, CREDIT_ACH, DEBIT_ACH...).
		/// Común a los 6 casos (se normaliza en el mapper).
		/// </summary>
		public string PaymentMeans { get; set; } = default!;

		/// <summary>
		/// Tipo de medio de pago (DEBITO, CREDITO, CONTADO).
		/// Común a los 6 casos (se normaliza en el mapper).
		/// </summary>
		public string PaymentMeansType { get; set; } = default!;

		/// <summary>Referencia de orden (opcional).</summary>
		public string? OrderReference { get; set; }

		/// <summary>Numeración (prefijo, flexible, resolución). Común a los 6 casos.</summary>
		public NumberingDto Numbering { get; set; } = default!;

		/// <summary>Cliente receptor. Común a los 6 casos.</summary>
		public CustomerDto Customer { get; set; } = default!;

		/// <summary>Ítems (≥1). Común a los 6 casos. Reglas finas por operación en el validador.</summary>
		public List<ItemDto> Items { get; set; } = new();

		/// <summary>Notas libres (opcional).</summary>
		public List<string> Notes { get; set; } = new();

		/// <summary>
		/// Operación FES: SS_RECAUDO, SS_CUDE, SS_CUFE, SS_POS, SS_SNUM, SS_SIN_APORTE.
		/// Común a los 6 casos (el mapper normaliza SS-XXXX → SS_XXXX).
		/// </summary>
		public string Operation { get; set; } = default!;

		/// <summary>Bloque de salud. Común a los 6 casos; su contenido varía por operación.</summary>
		public HealthDto Health { get; set; } = new();

		/// <summary>Moneda (opcional). Ej: "COP", "USD", "EUR".</summary>
		public string? Currency { get; set; }

		/// <summary>Tasa de cambio (opcional, &gt; 0).</summary>
		public decimal? CurrencyExchangeRate { get; set; }

		/// <summary>Fecha de la tasa de cambio (opcional, dd/MM/yyyy).</summary>
		public string? CurrencyExchangeRateDate { get; set; }

		/// <summary>Cargos/descuentos globales (opcional).</summary>
		public List<ChargeDto> Charges { get; set; } = new();

		/// <summary>Retenciones globales (opcional).</summary>
		public List<TaxDto> Retentions { get; set; } = new();

		/// <summary>Prepagos (opcional). Se exporta a invoice.prepayments.</summary>
		public List<PrepaymentDto> Prepayments { get; set; } = new();
	}

	public class ItemDto
	{
		/// <summary>
		/// SKU (obligatorio)
		/// </summary>
		public string Sku { get; set; }

		/// <summary>
		/// Cantidad (obligatorio)
		/// </summary>
		public decimal Quantity { get; set; }

		/// <summary>
		/// Descripción (obligatorio)
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Precio (obligatorio)
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// Unidad de medida (opcional)
		/// </summary>
		public string? MeasuringUnit { get; set; }

		/// <summary>
		/// Precio original (opcional)
		/// </summary>
		public decimal? OriginalPrice { get; set; }

		/// <summary>
		/// Tasa de descuento (opcional)
		/// </summary>
		public string? DiscountRate { get; set; }

		/// <summary>
		/// Impuestos (opcional en tu factura básica)
		/// </summary>
		public List<TaxDto>? Taxes { get; set; }

		/// <summary>
		/// Retenciones (opcional por ítem)
		/// </summary>
		public List<TaxDto>? Retentions { get; set; }

		/// <summary>
		/// Identificación del mandante (opcional)
		/// </summary>
		public string? MandanteIdentification { get; set; }

		/// <summary>
		/// Tipo de identificación del mandante (opcional)
		/// </summary>
		public string? MandanteIdentificationType { get; set; }

		/// <summary>
		/// Indica si es administración AIU (opcional)
		/// </summary>
		public bool? AiuAdministration { get; set; }

		/// <summary>
		/// Contrato AIU (opcional)
		/// </summary>
		public string? AiuContract { get; set; }
	}

	public class CustomerDto
	{
		/// <summary>
		/// Tipo de identificación (obligatorio)
		/// </summary>
		public string PartyIdentificationType { get; set; }

		/// <summary>
		/// Número de identificación (obligatorio)
		/// </summary>
		public string PartyIdentification { get; set; }

		/// <summary>
		/// Tipo de persona (obligatorio)
		/// </summary>
		public string PartyType { get; set; }

		/// <summary>
		/// Código de nivel tributario (obligatorio)
		/// </summary>
		public string TaxLevelCode { get; set; }

		/// <summary>
		/// Régimen (obligatorio)
		/// </summary>
		public string Regimen { get; set; }

		/// <summary>
		/// Nombre de la empresa
		/// (obligatorio solo si PartyType = PERSONA_JURIDICA)
		/// </summary>
		public string? CompanyName { get; set; }

		/// <summary>
		/// Primer nombre
		/// (obligatorio solo si PartyType = PERSONA_NATURAL)
		/// </summary>
		public string? FirstName { get; set; }

		/// <summary>
		/// Apellido
		/// (obligatorio solo si PartyType = PERSONA_NATURAL)
		/// </summary>
		public string? FamilyName { get; set; }

		/// <summary>
		/// Departamento (opcional para Dataico)
		/// Puede ser "11" o "BOGOTA"
		/// </summary>
		public string? Department { get; set; }

		/// <summary>
		/// Ciudad (opcional para Dataico)
		/// Puede ser "001" o "BOGOTA, D.C."
		/// </summary>
		public string? City { get; set; }

		/// <summary>
		/// Dirección (opcional)
		/// </summary>
		public string? AddressLine { get; set; }

		/// <summary>
		/// Código de país (opcional)
		/// </summary>
		public string? CountryCode { get; set; }

		/// <summary>
		/// Correo electrónico (obligatorio)
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Teléfono (obligatorio)
		/// </summary>
		public string Phone { get; set; }
	}

	public class PrepaymentDto
	{
		/// <summary>
		/// Monto del anticipo
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Descripción del anticipo
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Fecha de recepción
		/// </summary>
		public string ReceivedDate { get; set; }
	}

	public class TaxDto
	{
		/// <summary>
		/// Categoría del impuesto (ej: IVA, RET_FUENTE)
		/// </summary>
		public string TaxCategory { get; set; }

		/// <summary>
		/// Tasa del impuesto (porcentaje) - opcional según el tipo
		/// </summary>
		public decimal? TaxRate { get; set; }

		/// <summary>
		/// Base del impuesto (%) - opcional
		/// </summary>
		public decimal? TaxBase { get; set; }

		/// <summary>
		/// Monto base (valor numérico base) - opcional
		/// </summary>
		public decimal? BaseAmount { get; set; }

		/// <summary>
		/// Monto del impuesto calculado - opcional
		/// </summary>
		public decimal? TaxAmount { get; set; }
	}

	public class ChargeDto
	{
		/// <summary>
		/// Razón del cargo (obligatorio si mandas charges[])
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// Monto base (obligatorio si mandas charges[])
		/// </summary>
		public decimal BaseAmount { get; set; }

		/// <summary>
		/// Indica si es un descuento (true/false)
		/// </summary>
		public bool Discount { get; set; }
	}

	/// <summary>
	/// Información de salud para FES.
	/// IMPORTANTE: muchos campos son opcionales y su obligatoriedad depende de la operación.
	/// </summary>
	/// 
	public class NumberingDto
	{
		/// <summary>
		/// Número de resolución
		/// </summary>
		public string? ResolutionNumber { get; set; }

		/// <summary>
		/// Prefijo
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Indica si es flexible
		/// </summary>
		public bool Flexible { get; set; }
	}
	public class HealthDto
	{
		/// <summary>Versión de la API de salud. Por defecto "API_SALUD_V2".</summary>
		public string? Version { get; set; } = "API_SALUD_V2";

		/// <summary>Cobertura (opcional). Ej: PLAN_DE_BENEFICIOS, PRESUPUESTO_MAXIMO...</summary>
		public string? Coverage { get; set; }

		/// <summary>Código del proveedor (opcional).</summary>
		public string? ProviderCode { get; set; }

		/// <summary>Número de contrato (opcional).</summary>
		public string? ContractNumber { get; set; }

		/// <summary>Número de póliza (opcional).</summary>
		public string? PolicyNumber { get; set; }

		/// <summary>Modalidad de pago (catálogo Minsalud/Dataico). Opcional salvo políticas internas.</summary>
		public string? PaymentModality { get; set; }

		/// <summary>Fecha de inicio del período (dd/MM/yyyy, opcional).</summary>
		public string? PeriodStartDate { get; set; }

		/// <summary>Fecha de fin del período (dd/MM/yyyy, opcional).</summary>
		public string? PeriodEndDate { get; set; }

		public string? UserType { get; set; }

		/// <summary>Recaudos (obligatorio ≥1 solo en SS_RECAUDO; opcional en otras).</summary>
		public List<RecaudoDto> Recaudos { get; set; } = new();

		/// <summary>Usuarios asociados (opcional). Puede mapearse a associated_users.</summary>
		public List<AssociatedUserDto> AssociatedUsers { get; set; } = new();

		/// <summary>
		/// Persona (usuario principal). Requerida SOLO en SS_RECAUDO.
		/// Se deja nullable para no forzarla en las demás operaciones.
		/// </summary>
		public PersonDto? Person { get; set; }

		/// <summary>
		/// Documentos asociados (obligatorio ≥1 en SS_CUFE/SS_CUDE/SS_POS/SS_SNUM; opcional en otras).
		/// </summary>
		public List<AssociatedDocumentDto> AssociatedDocuments { get; set; } = new();
	}

	/// <summary>
	/// Documento asociado en FES (ej. CUFE/CUDE/POS/SNUM, copagos referenciados, etc.).
	/// Los campos se dejan opcionales aquí y se controlan con FluentValidation por operación.
	/// </summary>
	public class AssociatedDocumentDto
	{
		/// <summary>Número del documento asociado (opcional).</summary>
		public string? Number { get; set; }

		/// <summary>Fecha del documento asociado (dd/MM/yyyy, opcional).</summary>
		public string? IssueDate { get; set; }

		/// <summary>Monto del documento asociado (opcional).</summary>
		public decimal? Amount { get; set; }

		/// <summary>Código de cargo médico (COPAGO, PAGOS_COMPARTIDOS, etc., opcional aquí).</summary>
		public string? MedicalFeeCode { get; set; }

		/// <summary>Descripción (opcional).</summary>
		public string? Description { get; set; }

		/// <summary>CUFE (opcional).</summary>
		public string? Cufe { get; set; }

		/// <summary>CUDE (opcional).</summary>
		public string? Cude { get; set; }

		/// <summary>Tipo de documento (opcional). Ej: "SS-CUFE".</summary>
		public string? TypeCode { get; set; }

		/// <summary>Número de autorización DIAN (opcional).</summary>
		public string? DianAuthorizationNumber { get; set; }

		/// <summary>Identificación del usuario médico (opcional).</summary>
		public string? MedicalUserIdentification { get; set; }
	}

	/// <summary>
	/// Recaudo (para SS_RECAUDO; opcional en otras operaciones).
	/// </summary>
	public class RecaudoDto
	{
		/// <summary>Monto del recaudo.</summary>
		public decimal Amount { get; set; }

		/// <summary>Fecha del recaudo (dd/MM/yyyy).</summary>
		public string IssueDate { get; set; } = default!;

		/// <summary>Código del cargo médico: COPAGO, CUOTA_MODERADORA, PAGOS_COMPARTIDOS, etc.</summary>
		public string MedicalFeeCode { get; set; } = default!;
	}

	/// <summary>
	/// Usuario asociado. La obligatoriedad de campos la controla el validador cuando la lista venga poblada.
	/// </summary>
	public class AssociatedUserDto
	{
		/// <summary>Número de contrato (opcional).</summary>
		public string? ContractNumber { get; set; }

		/// <summary>Código del proveedor (opcional).</summary>
		public string? ProviderCode { get; set; }

		/// <summary>Tipo de usuario (ej.: CONTRIBUTIVO_COTIZANTE). Opcional aquí; se valida si aplica.</summary>
		public string? UserType { get; set; }

		/// <summary>Número de autorización (opcional).</summary>
		public string? AuthorizationNumber { get; set; }

		/// <summary>Datos de la persona asociada (opcional aquí; se valida si aplica).</summary>
		public PersonDto? Person { get; set; }

		/// <summary>Recaudo individual asociado (opcional).</summary>
		public RecaudoDto? Recaudo { get; set; }
	}

	/// <summary>
	/// Persona (usuario). Se deja flexible; el validador exige lo necesario en SS_RECAUDO.
	/// </summary>
	public class PersonDto
	{
		/// <summary>Identificación (opcional aquí; se valida si aplica).</summary>
		public string? Identification { get; set; }

		/// <summary>Tipo de identificación (dominio salud). Ej: CEDULA_CIUDADANIA (opcional aquí).</summary>
		public string? IdentificationType { get; set; }

		/// <summary>Tipo de identificación DIAN. Ej: CC, TI, NIT (opcional aquí).</summary>
		public string? DianIdentificationType { get; set; }

		/// <summary>País de origen de la identificación (opcional).</summary>
		public string? IdentificationOriginCountry { get; set; }

		/// <summary>Primer nombre (opcional).</summary>
		public string? FirstName { get; set; }

		/// <summary>Segundo nombre (opcional).</summary>
		public string? SecondFirstName { get; set; }

		/// <summary>Apellido (opcional).</summary>
		public string? LastName { get; set; }

		/// <summary>Segundo apellido (opcional).</summary>
		public string? SecondLastName { get; set; }
	}
}
